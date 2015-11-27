using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ImgAzyobuziNet.Core.Test;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ImgAzyobuziNet.Core.Resolvers
{
    public class DailymotionProvider : IPatternProvider
    {
        public string ServiceId => "Dailymotion";

        public string ServiceName => "Dailymotion";

        public string Pattern => @"^https?://(?:www\.)?dailymotion\.com/video/([^/\?]+)/?(?:\?.*)?(?:#.*)?$";

        private static readonly ResolverFactory f = PPUtils.CreateFactory<DailymotionResolver>();
        public IResolver GetResolver(IServiceProvider serviceProvider) => f(serviceProvider);

        [TestMethod(TestType.Static)]
        private void RegexTest()
        {
            var match = this.GetRegex().Match("http://www.dailymotion.com/video/x26m1j4_wildlife_animals");
            Assert.True(() => match.Success);
            match.Groups[1].Value.Is("x26m1j4_wildlife_animals");
        }
    }

    public class DailymotionResolver : IResolver
    {
        // 動画の取得には投稿者のアクセストークンが必要

        public DailymotionResolver(IMemoryCache memoryCache, ILogger<DailymotionResolver> logger)
        {
            this.memoryCache = memoryCache;
            this.logger = logger;
        }

        private readonly IMemoryCache memoryCache;
        private readonly ILogger logger;

        public class CacheItem
        {
            public string thumbnail_url { get; set; }
            public string thumbnail_480_url { get; set; }
            public string thumbnail_180_url { get; set; }
        }

        private static readonly string fields =
            string.Join(",", typeof(CacheItem).GetTypeInfo().DeclaredProperties.Select(x => x.Name));

        public async Task<ImageInfo[]> GetImages(Match match)
        {
            var id = match.Groups[1].Value;
            var result = await this.memoryCache.GetOrSet(
                "dailymotion-" + id,
                () => this.GetThumbnailUrl(id)
            ).ConfigureAwait(false);
            return new[] { new ImageInfo(result.thumbnail_url, result.thumbnail_480_url, result.thumbnail_180_url) };
        }

        private async Task<CacheItem> GetThumbnailUrl(string id)
        {
            using (var hc = new HttpClient())
            {
                var requestUri = "https://api.dailymotion.com/video/" + id + "?fields=" + fields;
                ResolverUtils.RequestingMessage(this.logger, requestUri, null);
                var res = await hc.GetAsync(requestUri).ConfigureAwait(false);

                switch (res.StatusCode)
                {
                    case HttpStatusCode.BadRequest:
                    case HttpStatusCode.NotFound:
                        throw new ImageNotFoundException();
                }

                res.EnsureSuccessStatusCode();

                var s = await res.Content.ReadAsStringAsync().ConfigureAwait(false);
                ResolverUtils.HttpResponseMessage(this.logger, s, null);

                return JsonConvert.DeserializeObject<CacheItem>(s);
            }
        }

        [TestMethod(TestType.Network)]
        private async Task GetThumbnailUrlTest()
        {
            var result = await this.GetThumbnailUrl("x26m1j4_wildlife_animals").ConfigureAwait(false);
            Assert.True(() => !string.IsNullOrEmpty(result.thumbnail_url));
            Assert.True(() => !string.IsNullOrEmpty(result.thumbnail_480_url));
            Assert.True(() => !string.IsNullOrEmpty(result.thumbnail_180_url));
        }
    }
}
