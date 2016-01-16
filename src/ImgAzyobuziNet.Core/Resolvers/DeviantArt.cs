using System;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ImgAzyobuziNet.Core.Test;
using Jil;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace ImgAzyobuziNet.Core.Resolvers
{
    public class DeviantArtProvider : IPatternProvider
    {
        public string ServiceId => "DeviantArt";

        public string ServiceName => "DeviantArt";

        public string Pattern => @"^https?://(?:[\w\-]+)\.deviantart\.com/art/([\w\-]+)/?(?:[\?#].*)?$";

        private static readonly ResolverFactory f = PPUtils.CreateFactory<DeviantArtResolver>();
        public IResolver GetResolver(IServiceProvider serviceProvider) => f(serviceProvider);

        #region Tests

        [TestMethod(TestType.Static)]
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
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger _logger;

        public DeviantArtResolver(IMemoryCache memoryCache, ILogger<DeviantArtResolver> logger)
        {
            this._memoryCache = memoryCache;
            this._logger = logger;
        }

        public async Task<ImageInfo[]> GetImages(Match match)
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
            using (var hc = new HttpClient())
            {
                var requestUri = "http://backend.deviantart.com/oembed?url=" + Uri.EscapeDataString(uri);
                ResolverUtils.RequestingMessage(this._logger, requestUri, null);

                using (var res = await hc.GetAsync(requestUri).ConfigureAwait(false))
                {
                    if (res.StatusCode == HttpStatusCode.NotFound)
                        throw new ImageNotFoundException();

                    res.EnsureSuccessStatusCode();
                    json = await res.Content.ReadAsStringAsync().ConfigureAwait(false);
                }
            }

            ResolverUtils.HttpResponseMessage(this._logger, json, null);
            return JSON.Deserialize<CacheItem>(json);
        }

        #region Tests

        [TestMethod(TestType.Network)]
        private async Task FetchTest()
        {
            var result = await this.Fetch("http://kirokaze.deviantart.com/art/Mountain-town-578514456").ConfigureAwait(false);
            Assert.True(() => !string.IsNullOrEmpty(result.url));
            Assert.True(() => !string.IsNullOrEmpty(result.thumbnail_url));
        }

        #endregion
    }
}
