using System;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ImgAzyobuziNet.Core.Test;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace ImgAzyobuziNet.Core.Resolvers
{
    public class _500pxProvider : IPatternProvider
    {
        public string ServiceId => "500px";

        public string ServiceName => "500px";

        // https://500px.com/photo/{id}/{title}
        public string Pattern => @"^https?://(?:www\.)?500px\.com/photo/(\d+)(?:/.*)?$";

        private static readonly ResolverFactory f = PPUtils.CreateFactory<_500pxResolver>();
        public IResolver GetResolver(IServiceProvider serviceProvider) => f(serviceProvider);

        [TestMethod(TestType.Static)]
        private void RegexIdTitle()
        {
            var match = this.GetRegex().Match(
                "https://500px.com/photo/128754325/t-v-winter-by-ray-green?ctx_page=1&from=popular");
            Assert.True(() => match.Success);
            match.Groups[1].Value.Is("128754325");
        }

        [TestMethod(TestType.Static)]
        private void RegexId()
        {
            var match = this.GetRegex().Match("https://500px.com/photo/128742743");
            Assert.True(() => match.Success);
            match.Groups[1].Value.Is("128742743");
        }
    }

    public class _500pxResolver : IResolver
    {
        public _500pxResolver(IMemoryCache memoryCache, ILogger<_500pxResolver> logger)
        {
            this.memoryCache = memoryCache;
            this.logger = logger;
        }

        private readonly IMemoryCache memoryCache;
        private readonly ILogger logger;

        public async Task<ImageInfo[]> GetImages(Match match)
        {
            var id = match.Groups[1].Value;
            var key = "500px-" + id;

            var result = await this.memoryCache.GetOrSet(
                "500px-" + id,
                () => this.GetImageUrl(id)
            ).ConfigureAwait(false);

            return new[] { new ImageInfo(result, result, result) };
        }

        private async Task<string> GetImageUrl(string id)
        {
            using (var hc = new HttpClient())
            {
                var requestUri = "https://api.500px.com/v1/photos/" + id + "?image_size=5&consumer_key=" + Constants._500pxConsumerKey;
                ResolverUtils.RequestingMessage(this.logger, requestUri, null);

                using (var res = await hc.GetAsync(requestUri).ConfigureAwait(false))
                {
                    if (res.StatusCode == HttpStatusCode.NotFound)
                        throw new ImageNotFoundException();

                    res.EnsureSuccessStatusCode();

                    var s = await res.Content.ReadAsStringAsync().ConfigureAwait(false);
                    ResolverUtils.HttpResponseMessage(this.logger, s, null);
                    var j = JObject.Parse(s);
                    return (string)j["photo"]["image_url"];
                }
            }
        }

        [TestMethod(TestType.Network)]
        private async Task GetImageUrlTest()
        {
            var imageUrl = await this.GetImageUrl("128836907").ConfigureAwait(false);
            Assert.True(() => !string.IsNullOrEmpty(imageUrl));
        }
    }
}
