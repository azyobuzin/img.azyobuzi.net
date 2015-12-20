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
    public class GyazoProvider : IPatternProvider
    {
        public string ServiceId => "Gyazo";

        public string ServiceName => "Gyazo";

        public string Pattern => @"^https?://(?:(?:www|i|bot)\.)?gyazo\.com/(\w+)(\.\w+)?(?:\?.*)?(?:#.*)?$";

        private static readonly ResolverFactory f = PPUtils.CreateFactory<GyazoResolver>();
        public IResolver GetResolver(IServiceProvider serviceProvider) => f(serviceProvider);

        #region Tests

        [TestMethod(TestType.Static)]
        private void RegexTest()
        {
            var match = this.GetRegex().Match("https://gyazo.com/5a924a0a4c83e23436754de2293b646e");
            Assert.True(() => match.Success);
            match.Groups[1].Value.Is("5a924a0a4c83e23436754de2293b646e");
            Assert.True(() => !match.Groups[2].Success);
        }

        [TestMethod(TestType.Static)]
        private void WithExtensionTest()
        {
            var match = this.GetRegex().Match("https://bot.gyazo.com/694986837d4524f13a264db302dd2122.gif");
            Assert.True(() => match.Success);
            match.Groups[1].Value.Is("694986837d4524f13a264db302dd2122");
            match.Groups[2].Value.Is(".gif");
        }

        #endregion
    }

    public class GyazoResolver : IResolver
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger _logger;

        public GyazoResolver(IMemoryCache memoryCache, ILogger<_500pxResolver> logger)
        {
            this._memoryCache = memoryCache;
            this._logger = logger;
        }

        public async Task<ImageInfo[]> GetImages(Match match)
        {
            var id = match.Groups[1].Value;
            var uri = new Uri(
                match.Groups[2].Success
                ? match.Value
                : await this._memoryCache.GetOrSet("gyazo-" + id, () => this.Fetch(id)).ConfigureAwait(false)
            );
            var full = uri.GetComponents(UriComponents.SchemeAndServer | UriComponents.Path, UriFormat.UriEscaped);
            var thumb = uri.GetComponents(UriComponents.SchemeAndServer, UriFormat.UriEscaped)
                + "/thumb/180" + uri.AbsolutePath;
            return new[] { new ImageInfo(full, full, thumb) };
        }

        private struct ApiResponse
        {
#pragma warning disable 649
            public string url;
#pragma warning restore 649
        }

        private async Task<string> Fetch(string id)
        {
            string s;
            using (var hc = new HttpClient())
            {
                var requestUri = "https://api.gyazo.com/api/oembed/?url="
                    + Uri.EscapeDataString("http://gyazo.com/" + id);
                ResolverUtils.RequestingMessage(this._logger, requestUri, null);

                using (var res = await hc.GetAsync(requestUri).ConfigureAwait(false))
                {
                    if (res.StatusCode == HttpStatusCode.NotFound)
                        throw new ImageNotFoundException();

                    res.EnsureSuccessStatusCode();
                    s = await res.Content.ReadAsStringAsync().ConfigureAwait(false);
                }
            }

            ResolverUtils.HttpResponseMessage(this._logger, s, null);
            return JSON.Deserialize<ApiResponse>(s).url;
        }

        #region Tests

        [TestMethod(TestType.Network)]
        private async Task FetchTest()
        {
            var result = await this.Fetch("5a924a0a4c83e23436754de2293b646e");
            Assert.True(() => result.EndsWith("/5a924a0a4c83e23436754de2293b646e.png", StringComparison.Ordinal));
        }

        #endregion
    }
}
