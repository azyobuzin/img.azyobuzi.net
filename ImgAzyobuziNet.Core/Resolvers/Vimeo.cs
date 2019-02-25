using System;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngleSharp.Html.Dom;
using ImgAzyobuziNet.Core.SupportServices;
using ImgAzyobuziNet.TestFramework;
using Shouldly;

namespace ImgAzyobuziNet.Core.Resolvers
{
    public class VimeoProvider : PatternProviderBase<VimeoResolver>
    {
        public override string ServiceId => "Vimeo";

        public override string ServiceName => "Vimeo";

        public override string Pattern => @"^https?://(?:www\.)?vimeo\.com/(?:channels/[\w+\-]+/)?(\d+)/?(?:[\?#]|$)";

        #region Tests

        [TestMethod(TestCategory.Static)]
        private void RegexTest()
        {
            var match = this.GetRegex().Match("https://vimeo.com/319453339?ref=tw-share");
            match.Success.ShouldBeTrue();
            match.Groups[1].Value.ShouldBe("319453339");
        }

        [TestMethod(TestCategory.Static)]
        private void RegexFromChannelTest()
        {
            var match = this.GetRegex().Match("https://vimeo.com/channels/staffpicks/318604033");
            match.Success.ShouldBeTrue();
            match.Groups[1].Value.ShouldBe("318604033");
        }

        #endregion
    }

    public class VimeoResolver : IResolver
    {
        private readonly IImgAzyobuziNetHttpClient _httpClient;
        private readonly IResolverCache _resolverCache;

        public VimeoResolver(IImgAzyobuziNetHttpClient httpClient, IResolverCache resolverCache)
        {
            this._httpClient = httpClient;
            this._resolverCache = resolverCache;
        }

        public async ValueTask<ImageInfo[]> GetImages(Match match)
        {
            var videoId = match.Groups[1].Value;
            var thumbnailId = await this._resolverCache.GetOrSet(
                "vimeo-" + videoId,
                () => this.FetchThumbnailId(videoId)
            ).ConfigureAwait(false);

            return new[]
            {
                new ImageInfo(
                    $"https://i.vimeocdn.com/video/{thumbnailId}_1920x1080.jpg?r=pad",
                    $"https://i.vimeocdn.com/video/{thumbnailId}_960x540.jpg?r=pad",
                    $"https://i.vimeocdn.com/video/{thumbnailId}_200x150.jpg?r=pad"
                )
            };
        }

        private async Task<string> FetchThumbnailId(string videoId)
        {
            IHtmlDocument document;
            var req = new HttpRequestMessage(
                HttpMethod.Get,
                "https://vimeo.com/" + videoId
            );

            using (var res = await this._httpClient.SendAsync(req).ConfigureAwait(false))
            {
                if (res.StatusCode == HttpStatusCode.NotFound)
                    throw new ImageNotFoundException();

                res.EnsureSuccessStatusCode();
                document = await res.Content.ReadAsHtmlDocument().ConfigureAwait(false);
            }

            var ogImage = document.GetOpenGraphImage();
            return Regex.Match(ogImage, @"(?<=[\?&]src0=https%3A%2F%2Fi\.vimeocdn\.com%2Fvideo%2F)\d+").Value
                ?? throw new Exception("仕様変更？ " + ogImage);
        }

        #region Tests

        [TestMethod(TestCategory.Network)]
        private async Task FetchThumbnailIdTest()
        {
            // https://vimeo.com/319453339
            var result = await this.FetchThumbnailId("319453339").ConfigureAwait(false);
            result.ShouldBe("762016255");
        }

        #endregion
    }
}
