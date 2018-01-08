using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ImgAzyobuziNet.Core.SupportServices;
using ImgAzyobuziNet.TestFramework;
using Jil;
using Microsoft.Extensions.Options;

namespace ImgAzyobuziNet.Core.Resolvers
{
    public class MobypictureProvider : PatternProviderBase<MobypictureResolver>
    {
        public override string ServiceId => "Mobypicture";

        public override string ServiceName => "Mobypicture";

        public override string Pattern => @"^https?://(?:www\.)?(?:moby\.to/(\w+)(?:\:\w*)?(?:[\?#]|$)|mobypicture\.com/user/\w+/view/(\d+)(?:[/\?#]|$))";

        #region Tests

        [TestMethod(TestCategory.Static)]
        private void RegexTest()
        {
            var match = this.GetRegex().Match("http://www.mobypicture.com/user/rockon_cpa/view/19785094?");
            Assert.True(() => match.Success);
            Assert.True(() => !match.Groups[1].Success);
            match.Groups[2].Value.Is("19785094");
        }

        [TestMethod(TestCategory.Static)]
        private void RegexTestWithSuffix()
        {
            var match = this.GetRegex().Match("http://www.mobypicture.com/user/rockon_cpa/view/19785094/hoge");
            Assert.True(() => match.Success);
            Assert.True(() => !match.Groups[1].Success);
            match.Groups[2].Value.Is("19785094");
        }

        [TestMethod(TestCategory.Static)]
        private void RegexTestTiny()
        {
            var match = this.GetRegex().Match("http://moby.to/715vsq");
            Assert.True(() => match.Success);
            Assert.True(() => !match.Groups[2].Success);
            match.Groups[1].Value.Is("715vsq");
        }

        #endregion
    }

    public class MobypictureResolver : IResolver
    {
        private readonly string _developerKey;
        private readonly IHttpClient _httpClient;
        private readonly IResolverCache _resolverCache;

        public MobypictureResolver(IOptions<ImgAzyobuziNetOptions> options, IHttpClient httpClient, IResolverCache resolverCache)
        {
            this._developerKey = options?.Value?.ApiKeys?.MobypictureDeveloperKey;
            this._httpClient = httpClient;
            this._resolverCache = resolverCache;
        }

        public async ValueTask<ImageInfo[]> GetImages(Match match)
        {
            if (string.IsNullOrEmpty(this._developerKey))
                throw new NotConfiguredException();

            // 誰か短縮の法則を見つけてくれ～
            CacheItem result;
            bool exists;
            if (match.Groups[1].Success)
            {
                var tinyCode = match.Groups[1].Value;
                var key = CreateTinyKey(tinyCode);
                (exists, result) = await this._resolverCache.TryGetValue<CacheItem>(key).ConfigureAwait(false);
                if (!exists)
                {
                    result = await this.Fetch(tinyCode, true).ConfigureAwait(false);
                    // TODO: キャッシュ保存失敗してもログだけ吐いて握りつぶしたいが、 Logger を引き回すのやだ…やだ…
                    await this._resolverCache.Set(key, result).ConfigureAwait(false);
                    await this._resolverCache.Set(CreateIdKey(result.Id), result).ConfigureAwait(false);
                }
            }
            else
            {
                var id = match.Groups[2].Value;
                var key = CreateIdKey(id);
                (exists, result) = await this._resolverCache.TryGetValue<CacheItem>(key).ConfigureAwait(false);
                if (!exists)
                {
                    result = await this.Fetch(id, false).ConfigureAwait(false);
                    await this._resolverCache.Set(key, result).ConfigureAwait(false);
                    await this._resolverCache.Set(CreateTinyKey(result.TinyCode), result).ConfigureAwait(false);
                }
            }

            return new[]
            {
                new ImageInfo(
                    result.UrlFull,
                    "http://moby.to/" + result.TinyCode + ":medium",
                    result.UrlThumbnail,
                    result.UrlVideo,
                    result.UrlVideo,
                    result.UrlVideo
                )
            };
        }

        private static string CreateIdKey(string id) => "mobypicture-id-" + id;
        private static string CreateTinyKey(string tinyCode) => "mobypicture-tiny-" + tinyCode;

        private class CacheItem
        {
            public string Id;
            public string TinyCode;
            public string UrlThumbnail;
            public string UrlFull;
            public string UrlVideo;
        }

        public class MobypictureException : Exception
        {
            public string Result { get; }

            public MobypictureException(string result, string message) : base(message)
            {
                this.Result = result;
            }
        }

        private class GetMediaInfoResponse
        {
            public string result;
            public string message;
            public Post post;
        }

        private class Post
        {
            public string id;
            public Media media;
            public string link_tiny;
        }

        private class Media
        {
            public string url_thumbnail;
            public string url_full;
            public string url_video;
        }

        private static readonly int s_linkTinyPrefixLength = "http://moby.to/".Length;

        private async Task<CacheItem> Fetch(string id, bool isTiny)
        {
            string json;
            var req = new HttpRequestMessage(
                HttpMethod.Get,
                "https://api.mobypicture.com/?action=getMediaInfo&format=json&key="
                    + this._developerKey
                    + (isTiny ? "&tinyurl_code=" : "&post_id=")
                    + Uri.EscapeDataString(id)
            );

            using (var res = await this._httpClient.SendAsync(req).ConfigureAwait(false))
            {
                json = await res.EnsureSuccessStatusCode().Content
                    .ReadAsStringAsync().ConfigureAwait(false);
            }

            var resObj = JSON.Deserialize<GetMediaInfoResponse>(json);

            if (resObj.result != "0")
            {
                if (resObj.result == "302")
                    throw new ImageNotFoundException();

                throw new MobypictureException(resObj.result, resObj.message);
            }

            var post = resObj.post;
            return new CacheItem
            {
                Id = post.id,
                TinyCode = post.link_tiny.Substring(s_linkTinyPrefixLength),
                UrlThumbnail = post.media.url_thumbnail,
                UrlFull = post.media.url_full,
                UrlVideo = post.media.url_video
            };
        }

        #region Tests

        [TestMethod(TestCategory.Network)]
        private async Task FetchPictureTest()
        {
            // http://moby.to/wywhsh
            var result = await this.Fetch("19785122", false).ConfigureAwait(false);
            result.Id.Is("19785122");
            result.TinyCode.Is("wywhsh");
            result.UrlThumbnail.NotNullOrEmpty();
            result.UrlFull.NotNullOrEmpty();
            Assert.True(() => result.UrlVideo == null);
        }

        [TestMethod(TestCategory.Network)]
        private async Task FetchVideoTest()
        {
            // http://moby.to/715vsq
            var result = await this.Fetch("715vsq", true).ConfigureAwait(false);
            result.Id.Is("19785094");
            result.TinyCode.Is("715vsq");
            result.UrlThumbnail.NotNullOrEmpty();
            result.UrlFull.NotNullOrEmpty();
            result.UrlVideo.NotNullOrEmpty();
        }

        #endregion
    }
}
