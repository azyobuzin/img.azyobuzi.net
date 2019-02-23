using System;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ImgAzyobuziNet.Core.SupportServices;
using ImgAzyobuziNet.TestFramework;
using Jil;
using Shouldly;

namespace ImgAzyobuziNet.Core.Resolvers
{
    public class DeviantArtProvider : PatternProviderBase<DeviantArtResolver>
    {
        public override string ServiceId => "DeviantArt";

        public override string ServiceName => "DeviantArt";

        public override string Pattern => @"^https?://(?:[\w\-]+)\.deviantart\.com/art/([\w\-]+)/?(?:[\?#]|$)";

        #region Tests

        [TestMethod(TestCategory.Static)]
        private void RegexTest()
        {
            var match = this.GetRegex().Match("http://aenea-jones.deviantart.com/art/Stillness-578505886");
            match.Success.ShouldBeTrue();
            match.Groups[1].Value.ShouldBe("Stillness-578505886");
        }

        #endregion
    }

    public class DeviantArtResolver : IResolver
    {
        private readonly IImgAzyobuziNetHttpClient _httpClient;
        private readonly IResolverCache _resolverCache;

        public DeviantArtResolver(IImgAzyobuziNetHttpClient httpClient, IResolverCache resolverCache)
        {
            this._httpClient = httpClient;
            this._resolverCache = resolverCache;
        }

        public async ValueTask<ImageInfo[]> GetImages(Match match)
        {
            var id = match.Groups[1].Value;
            var result = await this._resolverCache.GetOrSet(
                "deviantart-" + id,
                () => this.Fetch(match.Value)
            ).ConfigureAwait(false);
            return new[] { new ImageInfo(result.url, result.url, result.thumbnail_url) };
        }

        private class CacheItem
        {
            public string url;
            public string thumbnail_url;
        }

        private async Task<CacheItem> Fetch(string uri)
        {
            string json;
            var req = new HttpRequestMessage(
                HttpMethod.Get,
                "http://backend.deviantart.com/oembed?url=" + Uri.EscapeDataString(uri)
            );

            using (var res = await this._httpClient.SendAsync(req).ConfigureAwait(false))
            {
                if (res.StatusCode == HttpStatusCode.NotFound)
                    throw new ImageNotFoundException();

                res.EnsureSuccessStatusCode();
                json = await res.Content.ReadAsStringAsync().ConfigureAwait(false);
            }

            return JSON.Deserialize<CacheItem>(json);
        }

        #region Tests

        [TestMethod(TestCategory.Network)]
        private async Task FetchTest()
        {
            var result = await this.Fetch("http://kirokaze.deviantart.com/art/Mountain-town-578514456").ConfigureAwait(false);
            result.url.ShouldNotBeNullOrEmpty();
            result.thumbnail_url.ShouldNotBeNullOrEmpty();
        }

        #endregion
    }
}
