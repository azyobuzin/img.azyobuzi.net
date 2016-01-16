using System;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ImgAzyobuziNet.Core.Test;
using Jil;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.OptionsModel;

namespace ImgAzyobuziNet.Core.Resolvers
{
    public class _500pxProvider : IPatternProvider
    {
        public string ServiceId => "500px";

        public string ServiceName => "500px";

        // https://500px.com/photo/{id}/{title}
        public string Pattern => @"^https?://(?:www\.)?500px\.com/photo/(\d+)(?:[/\?#].*)?$";

        private static readonly ResolverFactory f = PPUtils.CreateFactory<_500pxResolver>();
        public IResolver GetResolver(IServiceProvider serviceProvider) => f(serviceProvider);

        #region Tests

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

        #endregion
    }

    public class _500pxResolver : IResolver
    {
        private readonly ImgAzyobuziNetOptions _options;
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger _logger;

        public _500pxResolver(IOptions<ImgAzyobuziNetOptions> options, IMemoryCache memoryCache, ILogger<_500pxResolver> logger)
        {
            this._options = options.Value;
            this._memoryCache = memoryCache;
            this._logger = logger;
        }

        public async Task<ImageInfo[]> GetImages(Match match)
        {
            var id = match.Groups[1].Value;
            var result = await this._memoryCache.GetOrSet(
                "500px-" + id,
                () => this.Fetch(id)
            ).ConfigureAwait(false);

            return new[] { new ImageInfo(result, result, result) };
        }

        private struct ApiResponse
        {
            public Photo photo;
        }

        private struct Photo
        {
            public string image_url;
        }

        private async Task<string> Fetch(string id)
        {
            string s;
            using (var hc = new HttpClient())
            {
                var requestUri = "https://api.500px.com/v1/photos/" + id
                    + "?image_size=5&consumer_key=" + this._options._500pxConsumerKey;
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
            return JSON.Deserialize<ApiResponse>(s).photo.image_url;
        }

        #region Tests

        [TestMethod(TestType.Network)]
        private async Task FetchTest()
        {
            var imageUrl = await this.Fetch("128836907").ConfigureAwait(false);
            Assert.True(() => !string.IsNullOrEmpty(imageUrl));
        }

        #endregion
    }
}
