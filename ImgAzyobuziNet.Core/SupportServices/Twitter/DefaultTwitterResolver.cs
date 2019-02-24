using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CoreTweet;
using CoreTweet.Core;
using ImgAzyobuziNet.TestFramework;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shouldly;

namespace ImgAzyobuziNet.Core.SupportServices.Twitter
{
    internal class DefaultTwitterResolver : ITwitterResolver
    {
        #region Logging

        private static readonly Func<ILogger, string, IDisposable> s_beginScope =
            LoggerMessage.DefineScope<string>("GetImagesByStatusId {StatusId}");

        private static readonly Action<ILogger, Exception> s_startRequest =
            LoggerMessage.Define(LogLevel.Information, new EventId(120, "StartRequest"), "Request statuses/show");

        private static readonly Action<ILogger, int, TimeSpan, Exception> s_endRequest =
            LoggerMessage.Define<int, TimeSpan>(LogLevel.Information, new EventId(121, "EndRequest"), "Status: {StatusCode}, Elapsed: {Elapsed}");

        private static readonly Action<ILogger, int, string, Exception> s_errorResponse =
            LoggerMessage.Define<int, string>(LogLevel.Error, new EventId(122, "ErrorResponse"), "Status: {StatusCode}\n{ResponseBody}");

        #endregion

        private readonly ApiKeys _apiKeys;
        private readonly TwitterCredentialsManager _credentialsManager;
        private readonly ILogger _logger;

        public DefaultTwitterResolver(IOptionsSnapshot<ImgAzyobuziNetOptions> options, TwitterCredentialsManager credentialsManager, ILogger<DefaultTwitterResolver> logger)
        {
            this._apiKeys = options?.Value?.ApiKeys;
            this._credentialsManager = credentialsManager;
            this._logger = logger;
        }

        public async Task<ImageInfo[]> GetImagesByStatusIdAsync(string statusId)
        {
            var statusIdLong = long.Parse(statusId, CultureInfo.InvariantCulture);
            var client = await this.CreateTwitterClientAsync().ConfigureAwait(false);

            StatusResponse status;

            using (this._logger != null ? s_beginScope(this._logger, statusId) : null)
            {
                Stopwatch stopwatch = null;

                if (this._logger != null)
                {
                    s_startRequest(this._logger, null);
                    stopwatch = Stopwatch.StartNew();
                }

                try
                {
                    status = await client.Statuses.ShowAsync(statusIdLong, include_entities: true).ConfigureAwait(false);
                }
                catch (TwitterException ex)
                {
                    stopwatch.Stop();

                    // ツイートが存在しない / アクセス権がない →  ImageNotFound
                    var errorCode = ex.Errors?.FirstOrDefault()?.Code;
                    if (errorCode == (int)ErrorCode.NoStatusFoundWithThatId || errorCode == (int)ErrorCode.NotAuthorizedToSeeStatus)
                    {
                        if (this._logger != null) s_endRequest(this._logger, (int)ex.Status, stopwatch.Elapsed, null);
                        throw new ImageNotFoundException();
                    }

                    if (this._logger != null)
                    {
                        s_errorResponse(this._logger, (int)ex.Status, ex.Json, ex);
                    }

                    throw;
                }

                if (this._logger != null)
                {
                    stopwatch.Stop();
                    s_endRequest(this._logger, 200, stopwatch.Elapsed, null);
                }
            }

            var mediaEntities = status.ExtendedEntities?.Media ?? status.Entities?.Media;

            if ((mediaEntities?.Length ?? 0) == 0) return Array.Empty<ImageInfo>();

            return Array.ConvertAll(
                status.ExtendedEntities.Media,
                entity =>
                {
                    var image = new ImageInfo(
                        entity.MediaUrlHttps + ":orig",
                        entity.MediaUrlHttps,
                        entity.MediaUrlHttps + ":thumb"
                    );

                    if (entity.VideoInfo != null)
                    {
                        var videoVariants = entity.VideoInfo.Variants
                            .Where(x => x.Bitrate.HasValue)
                            .OrderByDescending(x => x.Bitrate.Value)
                            .Select(x => x.Url)
                            .ToArray();

                        if (videoVariants.Length == 0)
                            throw new Exception("VideoInfo がおかしい: " + status.Json);

                        image.VideoFull = videoVariants[0];
                        image.VideoLarge = videoVariants[Math.Min(1, videoVariants.Length - 1)];
                        image.VideoMobile = videoVariants[Math.Min(2, videoVariants.Length - 1)];
                    }

                    return image;
                });
        }

        private ValueTask<TokensBase> CreateTwitterClientAsync()
        {
            var consumerKey = this._apiKeys?.TwitterConsumerKey;
            var consumerSecret = this._apiKeys?.TwitterConsumerSecret;

            if (string.IsNullOrEmpty(consumerKey))
                throw new NotConfiguredException(nameof(ImgAzyobuziNetOptions.ApiKeys) + ":" + nameof(ApiKeys.TwitterConsumerKey));
            if (string.IsNullOrEmpty(consumerSecret))
                throw new NotConfiguredException(nameof(ImgAzyobuziNetOptions.ApiKeys) + ":" + nameof(ApiKeys.TwitterConsumerSecret));

            var accessToken = this._apiKeys.TwitterAccessToken;

            if (!string.IsNullOrEmpty(accessToken))
            {
                return new ValueTask<TokensBase>(
                    new OAuth2Token()
                    {
                        ConsumerKey = consumerKey,
                        ConsumerSecret = consumerSecret,
                        BearerToken = accessToken,
                    });
            }

            return this._credentialsManager.CreateTwitterClient(consumerKey, consumerSecret);
        }

        #region Tests

        [TestMethod(TestCategory.Network)]
        private async Task SingleImageTest()
        {
            // https://twitter.com/azyobuzin/status/1032869397981851653
            var results = await this.GetImagesByStatusIdAsync("1032869397981851653").ConfigureAwait(false);
            results.Length.ShouldBe(1);
            results[0].Full.ShouldNotBeNullOrEmpty();
            results[0].Large.ShouldNotBeNullOrEmpty();
            results[0].Thumb.ShouldNotBeNullOrEmpty();
            results[0].VideoFull.ShouldBeNull();
            results[0].VideoLarge.ShouldBeNull();
            results[0].VideoMobile.ShouldBeNull();
        }

        [TestMethod(TestCategory.Network)]
        private async Task MultipleImageTest()
        {
            // https://twitter.com/azyobuzin/status/1009350357984526336
            var results = await this.GetImagesByStatusIdAsync("1009350357984526336").ConfigureAwait(false);
            results.Length.ShouldBe(2);
            results.ShouldAllBe(x =>
                !string.IsNullOrEmpty(x.Full) &&
                !string.IsNullOrEmpty(x.Large) &&
                !string.IsNullOrEmpty(x.Thumb) &&
                x.VideoFull == null &&
                x.VideoLarge == null &&
                x.VideoMobile == null);
        }

        [TestMethod(TestCategory.Network)]
        private async Task VideoTest()
        {
            // https://twitter.com/azyobuzin/status/1020740660238798848
            var results = await this.GetImagesByStatusIdAsync("1020740660238798848").ConfigureAwait(false);
            results.Length.ShouldBe(1);
            results[0].Full.ShouldNotBeNullOrEmpty();
            results[0].Large.ShouldNotBeNullOrEmpty();
            results[0].Thumb.ShouldNotBeNullOrEmpty();
            results[0].VideoFull.ShouldNotBeNullOrEmpty();
            results[0].VideoLarge.ShouldNotBeNullOrEmpty();
            results[0].VideoMobile.ShouldNotBeNullOrEmpty();
        }

        [TestMethod(TestCategory.Network)]
        private async Task GifAnimeTest()
        {
            // https://twitter.com/azyobuzin/status/1007263294162255872
            var results = await this.GetImagesByStatusIdAsync("1007263294162255872").ConfigureAwait(false);
            results.Length.ShouldBe(1);
            results[0].Full.ShouldNotBeNullOrEmpty();
            results[0].Large.ShouldNotBeNullOrEmpty();
            results[0].Thumb.ShouldNotBeNullOrEmpty();
            results[0].VideoFull.ShouldNotBeNullOrEmpty();
            results[0].VideoLarge.ShouldNotBeNullOrEmpty();
            results[0].VideoMobile.ShouldNotBeNullOrEmpty();
        }

        [TestMethod(TestCategory.Network)]
        private async Task TweetWithNoMediaTest()
        {
            // ネガティブキャッシュとして記録するので、例外は発生させない
            // 呼び出し側が NotPictureException をスローする
            // https://twitter.com/azyobuzin/status/1098598679366328322
            var results = await this.GetImagesByStatusIdAsync("1098598679366328322").ConfigureAwait(false);
            results.ShouldBeEmpty();
        }

        #endregion
    }
}
