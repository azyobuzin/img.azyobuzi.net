using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngleSharp.Dom.Html;
using AngleSharp.Extensions;
using ImgAzyobuziNet.Core.Test;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace ImgAzyobuziNet.Core.Resolvers
{
    public class MeshimazuProvider : IPatternProvider
    {
        public string ServiceId => "Meshimazu";

        public string ServiceName => "メシマズ.net";

        public string Pattern => @"^http://(?:www\.)?meshimazu\.net/posts/(\d+)/?(?:[\?#].*)?$";

        private static readonly ResolverFactory f = PPUtils.CreateFactory<MeshimazuResolver>();
        public IResolver GetResolver(IServiceProvider serviceProvider) => f(serviceProvider);

        #region Tests

        [TestMethod(TestType.Static)]
        private void RegexTest()
        {
            var match = this.GetRegex().Match("http://www.meshimazu.net/posts/1100");
            Assert.True(() => match.Success);
            match.Groups[1].Value.Is("1100");
        }

        #endregion
    }

    public class MeshimazuResolver : IResolver
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger _logger;

        public MeshimazuResolver(IMemoryCache memoryCache, ILogger<MeshimazuResolver> logger)
        {
            this._memoryCache = memoryCache;
            this._logger = logger;
        }

        public async Task<ImageInfo[]> GetImages(Match match)
        {
            var id = match.Groups[1].Value;
            var result = await this._memoryCache.GetOrSet(
                "meshimazu-" + id,
                () => this.Fetch(id)
            ).ConfigureAwait(false);

            var full = "http://www.meshimazu.net" + result;
            return new[] { new ImageInfo(full, full, full.Replace("/medium/", "/thumb/")) };
        }

        private async Task<string> Fetch(string id)
        {
            IHtmlDocument document;
            using (var hc = new HttpClient())
            {
                var requestUri = "http://www.meshimazu.net/posts/" + id;
                ResolverUtils.RequestingMessage(this._logger, requestUri, null);

                using (var res = await hc.GetAsync(requestUri).ConfigureAwait(false))
                {
                    if (res.StatusCode == HttpStatusCode.NotFound)
                        throw new ImageNotFoundException();

                    res.EnsureSuccessStatusCode();
                    document = await res.Content.ReadAsHtmlDocument().ConfigureAwait(false);
                }
            }

            return document.Body.Descendents<IHtmlImageElement>()
                .First(img => img.ClassName == "testes")
                .GetAttribute("src");
        }

        #region Tests

        [TestMethod(TestType.Network)]
        private async Task FetchTest()
        {
            // http://www.meshimazu.net/posts/1100
            var result = await this.Fetch("1100").ConfigureAwait(false);
            result.NotNullOrEmpty();
        }

        #endregion
    }
}
