using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ImgAzyobuziNet.Core.Test;
using Jil;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace ImgAzyobuziNet.Core.Resolvers
{
    public class CloudAppProvider : IPatternProvider
    {
        public string ServiceId => "CloudApp";

        public string ServiceName => "CloudApp";

        public string Pattern => @"^https?://(?:www\.)?cl\.ly/(?:image/)?(\w+)/?(?:\?.*)?(?:#.*)?$";

        private static readonly ResolverFactory f = PPUtils.CreateFactory<CloudAppResolver>();
        public IResolver GetResolver(IServiceProvider serviceProvider) => f(serviceProvider);

        #region Tests

        [TestMethod(TestType.Static)]
        private void RegexId()
        {
            var match = this.GetRegex().Match("http://cl.ly/2V2a2R1E1v3F");
            Assert.True(() => match.Success);
            match.Groups[1].Value.Is("2V2a2R1E1v3F");
        }

        [TestMethod(TestType.Static)]
        private void RegexImageId()
        {
            var match = this.GetRegex().Match("http://cl.ly/image/1u1T2k2N2F1L");
            Assert.True(() => match.Success);
            match.Groups[1].Value.Is("1u1T2k2N2F1L");
        }

        #endregion
    }

    public class CloudAppResolver : IResolver
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger _logger;

        public CloudAppResolver(IMemoryCache memoryCache, ILogger<CloudAppResolver> logger)
        {
            this._memoryCache = memoryCache;
            this._logger = logger;
        }

        public class CacheItem
        {
            public string item_type;
            public string content_url;
            public string thumbnail_url;
        }

        public async Task<ImageInfo[]> GetImages(Match match)
        {
            var id = match.Groups[1].Value;
            var key = "cloudapp-" + id;

            var result = await this._memoryCache.GetOrSet(
                "cloudapp-" + id,
                () => this.Fetch(match.Value)
            ).ConfigureAwait(false);

            ImageInfo i;
            switch (result.item_type)
            {
                case "image":
                    i = new ImageInfo(result.content_url, result.content_url, result.thumbnail_url ?? result.content_url);
                    break;
                case "video":
                    // ThumbnailUrl is probably null.
                    i = new ImageInfo(result.thumbnail_url, result.thumbnail_url, result.thumbnail_url, result.content_url);
                    break;
                default:
                    throw new NotPictureException();
            }

            return new[] { i };
        }

        private async Task<CacheItem> Fetch(string uri)
        {
            using (var hc = new HttpClient())
            {
                hc.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                ResolverUtils.RequestingMessage(this._logger, uri, null);

                string s;
                using (var res = await hc.GetAsync(uri).ConfigureAwait(false))
                {
                    if (res.StatusCode == HttpStatusCode.NotFound)
                        throw new ImageNotFoundException();

                    res.EnsureSuccessStatusCode();
                    s = await res.Content.ReadAsStringAsync().ConfigureAwait(false);
                }

                ResolverUtils.HttpResponseMessage(this._logger, s, null);
                return JSON.Deserialize<CacheItem>(s);
            }
        }

        #region Tests

        [TestMethod(TestType.Network)]
        private async Task ImageTest()
        {
            var result = await this.Fetch("http://cl.ly/image/1u1T2k2N2F1L").ConfigureAwait(false);
            result.item_type.Is("image");
            Assert.True(() => !string.IsNullOrEmpty(result.content_url));
            Assert.True(() => !string.IsNullOrEmpty(result.thumbnail_url));
        }

        [TestMethod(TestType.Network)]
        private async Task VideoTest()
        {
            var result = await this.Fetch("http://cl.ly/2V2a2R1E1v3F").ConfigureAwait(false);
            result.item_type.Is("video");
            Assert.True(() => !string.IsNullOrEmpty(result.content_url));
        }

        [TestMethod(TestType.Network)]
        private async Task NotImageTest()
        {
            try
            {
                await this.GetImages(new CloudAppProvider().GetRegex().Match("http://cl.ly/0D3P1e022K10")).ConfigureAwait(false);
            }
            catch (NotPictureException)
            {
                // OK
                return;
            }

            throw new AssertionException("No exception has been thrown.");
        }

        #endregion
    }
}
