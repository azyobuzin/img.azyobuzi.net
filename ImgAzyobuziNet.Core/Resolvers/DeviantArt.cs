using System;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ImgAzyobuziNet.Core.SupportServices;
using ImgAzyobuziNet.TestFramework;
using Jil;
using Microsoft.Extensions.Caching.Memory;

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
            Assert.True(() => match.Success);
            match.Groups[1].Value.Is("Stillness-578505886");
        }

        #endregion
    }

    public class DeviantArtResolver : IResolver
    {
        private readonly IHttpClient _httpClient;
        private readonly IResolverCache _memoryCache;

        public DeviantArtResolver(IHttpClient httpClient, IResolverCache memoryCache)
        {
            this._httpClient = httpClient;
            this._memoryCache = memoryCache;
        }

        public async ValueTask<ImageInfo[]> GetImages(Match match)
        {
            var id = match.Groups[1].Value;
            var result = await this._memoryCache.GetOrSet(
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
            Assert.True(() => !string.IsNullOrEmpty(result.url));
            Assert.True(() => !string.IsNullOrEmpty(result.thumbnail_url));
        }

        #endregion
    }
}
