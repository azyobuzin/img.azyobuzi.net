using System;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngleSharp.Parser.Html;
using ImgAzyobuziNet.Core.Test;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace ImgAzyobuziNet.Core.Resolvers
{
    public class CameranProvider : IPatternProvider
    {
        // http://cameran.in/posts/get/v1/{hex} は現存するものが見つからないのでサポートやめます

        public string ServiceId => "cameran";

        public string ServiceName => "cameran";

        public string Pattern => @"^http://cameran\.in/p/v1/(\w+)/?(?:\?.*)?(?:#.*)?$";

        private static readonly ResolverFactory f = PPUtils.CreateFactory<CameranResolver>();
        public IResolver GetResolver(IServiceProvider serviceProvider) => f(serviceProvider);

        [TestMethod(TestType.Static)]
        private void RegexTest()
        {
            var match = this.GetRegex().Match("http://cameran.in/p/v1/3hbTqO2U4W");
            Assert.True(() => match.Success);
            match.Groups[1].Value.Is("3hbTqO2U4W");
        }
    }

    public class CameranResolver : IResolver
    {
        public CameranResolver(IMemoryCache memoryCache, ILogger<CameranResolver> logger)
        {
            this.memoryCache = memoryCache;
            this.logger = logger;
        }

        private readonly IMemoryCache memoryCache;
        private readonly ILogger logger;

        public async Task<ImageInfo[]> GetImages(Match match)
        {
            var id = match.Groups[1].Value;
            var key = "cameran-" + id;

            string result;
            if (!this.memoryCache.TryGetValue(key, out result))
            {
                result = await this.GetOgImage(id).ConfigureAwait(false);
                this.memoryCache.SetWithDefaultExpiration(key, result);
            }

            return new[] { new ImageInfo(result, result, result) };
        }

        private async Task<string> GetOgImage(string id)
        {
            using (var hc = new HttpClient(new HttpClientHandler() { AllowAutoRedirect = false }))
            {
                var requestUri = "http://cameran.in/p/v1/" + id;
                ResolverUtils.RequestingMessage(this.logger, requestUri, null);
                var res = await hc.GetAsync(requestUri).ConfigureAwait(false);

                switch (res.StatusCode)
                {
                    case HttpStatusCode.NotFound:
                    case HttpStatusCode.Found:
                    case HttpStatusCode.SeeOther:
                        throw new ImageNotFoundException();
                }

                res.EnsureSuccessStatusCode();

                return ResolverUtils.GetOgImage(
                    await new HtmlParser().ParseAsync(
                        await res.Content.ReadAsStreamAsync().ConfigureAwait(false)
                    ).ConfigureAwait(false));
            }
        }

        [TestMethod(TestType.Network)]
        private async Task GetOgImageTest()
        {
            var image = await this.GetOgImage("3hbTqO2U4W").ConfigureAwait(false);
            Assert.True(() => !string.IsNullOrEmpty(image));
        }
    }
}
