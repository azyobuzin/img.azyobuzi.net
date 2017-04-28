using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ImgAzyobuziNet.Core.SupportServices;
using ImgAzyobuziNet.TestFramework;
using Jil;

namespace ImgAzyobuziNet.Core.Resolvers
{
    public class CloudAppProvider : PatternProviderBase<CloudAppResolver>
    {
        public override string ServiceId => "CloudApp";

        public override string ServiceName => "CloudApp";

        public override string Pattern => @"^https?://(?:www\.)?cl\.ly/(?:image/)?(\w+)/?(?:[\?#]|$)";

        #region Tests

        [TestMethod(TestCategory.Static)]
        private void RegexId()
        {
            var match = this.GetRegex().Match("http://cl.ly/2V2a2R1E1v3F");
            Assert.True(() => match.Success);
            match.Groups[1].Value.Is("2V2a2R1E1v3F");
        }

        [TestMethod(TestCategory.Static)]
        private void RegexImageId()
        {
            var match = this.GetRegex().Match("http://cl.ly/image/1u1T2k2N2F1L");
            Assert.True(() => match.Success);
            match.Groups[1].Value.Is("1u1T2k2N2F1L");
        }

        #endregion
    }

    public class CloudAppResolver : IResolver
    {
        private readonly IHttpClient _httpClient;
        private readonly IResolverCache _resolverCache;

        public CloudAppResolver(IHttpClient httpClient, IResolverCache resolverCache)
        {
            this._httpClient = httpClient;
            this._resolverCache = resolverCache;
        }

        private class CacheItem
        {
            public string item_type;
            public string content_url;
            public string thumbnail_url;
        }

        public async ValueTask<ImageInfo[]> GetImages(Match match)
        {
            var id = match.Groups[1].Value;
            var key = "cloudapp-" + id;

            var result = await this._resolverCache.GetOrSet(
                "cloudapp-" + id,
                () => this.Fetch(match.Value)
            ).ConfigureAwait(false);

            ImageInfo i;
            switch (result.item_type)
            {
                case "image":
                    i = new ImageInfo(result.content_url, result.content_url, result.thumbnail_url ?? result.content_url);
                    break;
                case "video":
                    // ThumbnailUrl is probably null.
                    i = new ImageInfo(result.thumbnail_url, result.thumbnail_url, result.thumbnail_url, result.content_url, result.content_url, result.content_url);
                    break;
                default:
                    throw new NotPictureException();
            }

            return new[] { i };
        }

        private async Task<CacheItem> Fetch(string uri)
        {
            string s;
            var req = new HttpRequestMessage(HttpMethod.Get, uri);
            req.Headers.Accept.Set(new MediaTypeWithQualityHeaderValue("application/json"));

            using (var res = await this._httpClient.SendAsync(req).ConfigureAwait(false))
            {
                if (res.StatusCode == HttpStatusCode.NotFound)
                    throw new ImageNotFoundException();

                res.EnsureSuccessStatusCode();
                s = await res.Content.ReadAsStringAsync().ConfigureAwait(false);
            }

            return JSON.Deserialize<CacheItem>(s);
        }

        #region Tests

        [TestMethod(TestCategory.Network)]
        private async Task ImageTest()
        {
            var result = await this.Fetch("http://cl.ly/image/1u1T2k2N2F1L").ConfigureAwait(false);
            result.item_type.Is("image");
            Assert.True(() => !string.IsNullOrEmpty(result.content_url));
            Assert.True(() => !string.IsNullOrEmpty(result.thumbnail_url));
        }

        [TestMethod(TestCategory.Network)]
        private async Task VideoTest()
        {
            var result = await this.Fetch("http://cl.ly/2V2a2R1E1v3F").ConfigureAwait(false);
            result.item_type.Is("video");
            Assert.True(() => !string.IsNullOrEmpty(result.content_url));
        }

        [TestMethod(TestCategory.Network)]
        private async Task NotImageTest()
        {
            try
            {
                await this.GetImages(new CloudAppProvider().GetRegex().Match("http://cl.ly/0D3P1e022K10")).ConfigureAwait(false);
            }
            catch (NotPictureException)
            {
                // OK
                return;
            }

            throw new AssertionException("No exception has been thrown.");
        }

        #endregion
    }
}
