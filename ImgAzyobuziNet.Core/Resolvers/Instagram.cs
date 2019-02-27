using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ImgAzyobuziNet.Core.SupportServices;
using ImgAzyobuziNet.TestFramework;
using Jil;
using Microsoft.Extensions.Options;

namespace ImgAzyobuziNet.Core.Resolvers
{
    // Instagram は API が動かないのでしばらくお休み
    /*
    public class InstagramProvider : PatternProviderBase<InstagramResolver>
    {
        public override string ServiceId => "Instagram";

        public override string ServiceName => "Instagram";

        public override string Pattern => @"^https?://(?:www\.)?instagr(?:\.am|am\.com)/p/([\w\-]+)(?:/(?:media/?)?)?(?:[\?#]|$)";

        #region Tests

        [TestMethod(TestCategory.Static)]
        private void RegexTest()
        {
            var regex = this.GetRegex();

            {
                var match = regex.Match("https://www.instagram.com/p/UIetfMAHz2/?taken-by=azyobuzin");
                Assert.True(() => match.Success);
                match.Groups[1].Value.Is("UIetfMAHz2");
            }

            {
                var match = regex.Match("https://www.instagram.com/p/UIau5sgHwW/media/?size=l");
                Assert.True(() => match.Success);
                match.Groups[1].Value.Is("UIau5sgHwW");
            }
        }

        #endregion
    }
    */

    public class InstagramResolver : IResolver
    {
        private readonly string _accessToken;
        private readonly IImgAzyobuziNetHttpClient _httpClient;
        private readonly IResolverCache _resolverCache;

        public InstagramResolver(IOptionsSnapshot<ApiKeyOptions> options, IImgAzyobuziNetHttpClient httpClient, IResolverCache resolverCache)
        {
            this._accessToken = options?.Value?.InstagramAccessToken;
            this._httpClient = httpClient;
            this._resolverCache = resolverCache;
        }

        public async ValueTask<ImageInfo[]> GetImages(Match match)
        {
            if (string.IsNullOrEmpty(this._accessToken))
                throw new NotConfiguredException(nameof(ImgAzyobuziNetOptions.ApiKeys) + ":" + nameof(ApiKeyOptions.InstagramAccessToken));

            var id = match.Groups[1].Value;
            var result = await this._resolverCache.GetOrSet(
                "instagram-" + id,
                () => this.Fetch(id)
            ).ConfigureAwait(false);

            return new[]
            {
                result != null
                    ? new ImageInfo(
                        result.images.standard_resolution.url,
                        result.images.standard_resolution.url,
                        result.images.thumbnail.url,
                        result.videos?.standard_resolution.url,
                        result.videos?.standard_resolution.url,
                        result.videos?.low_resolution.url
                    )
                    : new ImageInfo(
                        "https://www.instagram.com/p/" + id + "/media/?size=l",
                        "https://www.instagram.com/p/" + id + "/media/?size=l",
                        "https://www.instagram.com/p/" + id + "/media/?size=t"
                    )
            };
        }

        private struct Response
        {
            public CacheItem data;
        }

        private class CacheItem
        {
            public Videos? videos;
            public Images images;
        }

        private struct Image
        {
            public string url;
        }

        private struct Videos
        {
            public Image low_resolution;
            public Image standard_resolution;
        }

        private struct Images
        {
            public Image thumbnail;
            public Image standard_resolution;
        }

        private async Task<CacheItem> Fetch(string id)
        {
            string json;
            var req = new HttpRequestMessage(
                HttpMethod.Get,
                "https://api.instagram.com/v1/media/shortcode/" + id
                    + "?access_token=" + this._accessToken
            );

            using (var res = await this._httpClient.SendAsync(req).ConfigureAwait(false))
            {
                switch (res.StatusCode)
                {
                    case HttpStatusCode.BadRequest:
                        throw new ImageNotFoundException();
                    case HttpStatusCode.NotFound:
                        return null; // アクセス不可能
                }

                res.EnsureSuccessStatusCode();
                json = await res.Content.ReadAsStringAsync().ConfigureAwait(false);
            }

            return JSON.Deserialize<Response>(json).data;
        }

        #region Tests

        /*
        [TestMethod(TestCategory.Network)]
        private async Task FetchImageTest()
        {
            // https://www.instagram.com/p/UIetfMAHz2/
            var result = await this.Fetch("UIetfMAHz2").ConfigureAwait(false);
            result.images.thumbnail.url.NotNullOrEmpty();
            result.images.standard_resolution.url.NotNullOrEmpty();
            Assert.True(() => !result.videos.HasValue);
        }

        [TestMethod(TestCategory.Network)]
        private async Task FetchVideoTest()
        {
            // https://www.instagram.com/p/bfnv-AAH4y/
            var result = await this.Fetch("bfnv-AAH4y").ConfigureAwait(false);
            result.images.thumbnail.url.NotNullOrEmpty();
            result.images.standard_resolution.url.NotNullOrEmpty();
            result.videos.Value.low_resolution.url.NotNullOrEmpty();
            result.videos.Value.standard_resolution.url.NotNullOrEmpty();
        }
        */

        [TestMethod(TestCategory.Network)]
        private void HelpWanted()
        {
            throw new System.Exception("API キーが死んでいます。本登録しなきゃダメかな？助けて");
        }

        #endregion
    }
}
