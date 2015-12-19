using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ImgAzyobuziNet.Core.Test;
using Jil;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace ImgAzyobuziNet.Core.Resolvers
{
    public class DailymotionProvider : IPatternProvider
    {
        public string ServiceId => "Dailymotion";

        public string ServiceName => "Dailymotion";

        public string Pattern => @"^https?://(?:www\.)?dailymotion\.com/video/([^/\?]+)/?(?:\?.*)?(?:#.*)?$";

        private static readonly ResolverFactory f = PPUtils.CreateFactory<DailymotionResolver>();
        public IResolver GetResolver(IServiceProvider serviceProvider) => f(serviceProvider);

        #region Tests

        [TestMethod(TestType.Static)]
        private void RegexTest()
        {
            var match = this.GetRegex().Match("http://www.dailymotion.com/video/x26m1j4_wildlife_animals");
            Assert.True(() => match.Success);
            match.Groups[1].Value.Is("x26m1j4_wildlife_animals");
        }

        #endregion
    }

    public class DailymotionResolver : IResolver
    {
        // 動画の取得には投稿者のアクセストークンが必要

        private readonly IMemoryCache _memoryCache;
        private readonly ILogger _logger;

        public DailymotionResolver(IMemoryCache memoryCache, ILogger<DailymotionResolver> logger)
        {
            this._memoryCache = memoryCache;
            this._logger = logger;
        }

        public class CacheItem
        {
            public string thumbnail_url;
            public string thumbnail_480_url;
            public string thumbnail_180_url;
        }

        private static readonly string fields =
            string.Join(",", typeof(CacheItem).GetTypeInfo().DeclaredFields.Select(x => x.Name));

        public async Task<ImageInfo[]> GetImages(Match match)
        {
            var id = match.Groups[1].Value;
            var result = await this._memoryCache.GetOrSet(
                "dailymotion-" + id,
                () => this.Fetch(id)
            ).ConfigureAwait(false);
            return new[] { new ImageInfo(result.thumbnail_url, result.thumbnail_480_url, result.thumbnail_180_url) };
        }

        private async Task<CacheItem> Fetch(string id)
        {
            using (var hc = new HttpClient())
            {
                var requestUri = "https://api.dailymotion.com/video/" + id + "?fields=" + fields;
                ResolverUtils.RequestingMessage(this._logger, requestUri, null);

                string json;
                using (var res = await hc.GetAsync(requestUri).ConfigureAwait(false))
                {
                    switch (res.StatusCode)
                    {
                        case HttpStatusCode.BadRequest:
                        case HttpStatusCode.NotFound:
                            throw new ImageNotFoundException();
                    }

                    res.EnsureSuccessStatusCode();
                    json = await res.Content.ReadAsStringAsync().ConfigureAwait(false);
                }

                ResolverUtils.HttpResponseMessage(this._logger, json, null);
                return JSON.Deserialize<CacheItem>(json);
            }
        }

        #region Tests

        [TestMethod(TestType.Network)]
        private async Task FetchTest()
        {
            var result = await this.Fetch("x26m1j4_wildlife_animals").ConfigureAwait(false);
            Assert.True(() => !string.IsNullOrEmpty(result.thumbnail_url));
            Assert.True(() => !string.IsNullOrEmpty(result.thumbnail_480_url));
            Assert.True(() => !string.IsNullOrEmpty(result.thumbnail_180_url));
        }

        #endregion
    }
}
