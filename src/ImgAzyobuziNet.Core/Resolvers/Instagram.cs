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
    public class InstagramProvider : IPatternProvider
    {
        public string ServiceId => "Instagram";

        public string ServiceName => "Instagram";

        public string Pattern => @"^https?://(?:www\.)?instagr(?:\.am|am\.com)/p/([\w\-]+)(?:/(?:media/?)?)?(?:[\?#].*)?$";

        private static readonly ResolverFactory f = PPUtils.CreateFactory<InstagramResolver>();
        public IResolver GetResolver(IServiceProvider serviceProvider) => f(serviceProvider);

        #region Tests

        [TestMethod(TestType.Static)]
        private void RegexTest()
        {
            var regex = this.GetRegex();

            {
                var match = regex.Match("https://www.instagram.com/p/UIetfMAHz2/?taken-by=azyobuzin");
                Assert.True(() => match.Success);
                match.Groups[1].Value.Is("UIetfMAHz2");
            }

            {
                var match = regex.Match("https://www.instagram.com/p/UIau5sgHwW/media/?size=l");
                Assert.True(() => match.Success);
                match.Groups[1].Value.Is("UIau5sgHwW");
            }
        }

        #endregion
    }

    public class InstagramResolver : IResolver
    {
        private readonly ImgAzyobuziNetOptions _options;
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger _logger;

        public InstagramResolver(IOptions<ImgAzyobuziNetOptions> options, IMemoryCache memoryCache, ILogger<InstagramResolver> logger)
        {
            this._options = options.Value;
            this._memoryCache = memoryCache;
            this._logger = logger;
        }

        public async Task<ImageInfo[]> GetImages(Match match)
        {
            var id = match.Groups[1].Value;
            var result = await this._memoryCache.GetOrSet(
                "instagram-" + id,
                () => this.Fetch(id)
            ).ConfigureAwait(false);

            return new[]
            {
                result != null
                    ? new ImageInfo(
                        result.images.standard_resolution.url,
                        result.images.standard_resolution.url,
                        result.images.thumbnail.url,
                        result.videos?.standard_resolution.url,
                        result.videos?.standard_resolution.url,
                        result.videos?.low_resolution.url
                    )
                    : new ImageInfo(
                        "https://www.instagram.com/p/" + id + "/media/?size=l",
                        "https://www.instagram.com/p/" + id + "/media/?size=l",
                        "https://www.instagram.com/p/" + id + "/media/?size=t"
                    )
            };
        }

        private struct Response
        {
            public CacheItem data;
        }

        private class CacheItem
        {
            public Videos? videos;
            public Images images;
        }

        private struct Image
        {
            public string url;
        }

        private struct Videos
        {
            public Image low_resolution;
            public Image standard_resolution;
        }

        private struct Images
        {
            public Image thumbnail;
            public Image standard_resolution;
        }

        private async Task<CacheItem> Fetch(string id)
        {
            string json;
            using (var hc = new HttpClient())
            {
                var requestUri = "https://api.instagram.com/v1/media/shortcode/" + id
                    + "?access_token=" + this._options.InstagramAccessToken;
                ResolverUtils.RequestingMessage(this._logger, requestUri, null);

                using (var res = await hc.GetAsync(requestUri).ConfigureAwait(false))
                {
                    switch (res.StatusCode)
                    {
                        case HttpStatusCode.BadRequest:
                            throw new ImageNotFoundException();
                        case HttpStatusCode.NotFound:
                            return null; // アクセス不可能
                    }

                    res.EnsureSuccessStatusCode();
                    json = await res.Content.ReadAsStringAsync().ConfigureAwait(false);
                }
            }

            ResolverUtils.HttpResponseMessage(this._logger, json, null);
            return JSON.Deserialize<Response>(json).data;
        }

        #region Tests

        [TestMethod(TestType.Network)]
        private async Task FetchImageTest()
        {
            // https://www.instagram.com/p/UIetfMAHz2/
            var result = await this.Fetch("UIetfMAHz2").ConfigureAwait(false);
            result.images.thumbnail.url.NotNullOrEmpty();
            result.images.standard_resolution.url.NotNullOrEmpty();
            Assert.True(() => !result.videos.HasValue);
        }

        [TestMethod(TestType.Network)]
        private async Task FetchVideoTest()
        {
            // https://www.instagram.com/p/bfnv-AAH4y/
            var result = await this.Fetch("bfnv-AAH4y").ConfigureAwait(false);
            result.images.thumbnail.url.NotNullOrEmpty();
            result.images.standard_resolution.url.NotNullOrEmpty();
            result.videos.Value.low_resolution.url.NotNullOrEmpty();
            result.videos.Value.standard_resolution.url.NotNullOrEmpty();
        }

        #endregion
    }
}
