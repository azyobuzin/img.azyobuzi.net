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
    }

    public class CloudAppResolver : IResolver
    {
        public CloudAppResolver(IMemoryCache memoryCache, ILogger<CloudAppResolver> logger)
        {
            this.memoryCache = memoryCache;
            this.logger = logger;
        }

        private readonly IMemoryCache memoryCache;
        private readonly ILogger logger;

        public class CacheItem
        {
            [DataMember(Name = "item_type")]
            public string ItemType { get; set; }
            [DataMember(Name = "content_url")]
            public string ContentUrl { get; set; }
            [DataMember(Name = "thumbnail_url")]
            public string ThumbnailUrl { get; set; }
        }

        public async Task<ImageInfo[]> GetImages(Match match)
        {
            var id = match.Groups[1].Value;
            var key = "cloudapp-" + id;

            var result = await this.memoryCache.GetOrSet(
                "cloudapp-" + id,
                () => this.GetContentUrl(match.Value)
            ).ConfigureAwait(false);

            ImageInfo i;
            switch (result.ItemType)
            {
                case "image":
                    i = new ImageInfo(result.ContentUrl, result.ContentUrl, result.ThumbnailUrl ?? result.ContentUrl);
                    break;
                case "video":
                    // ThumbnailUrl is probably null.
                    i = new ImageInfo(result.ThumbnailUrl, result.ThumbnailUrl, result.ThumbnailUrl, result.ContentUrl);
                    break;
                default:
                    throw new NotPictureException();
            }

            return new[] { i };
        }

        private async Task<CacheItem> GetContentUrl(string uri)
        {
            using (var hc = new HttpClient())
            {
                hc.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                ResolverUtils.RequestingMessage(this.logger, uri, null);

                using (var res = await hc.GetAsync(uri).ConfigureAwait(false))
                {
                    if (res.StatusCode == HttpStatusCode.NotFound)
                        throw new ImageNotFoundException();

                    res.EnsureSuccessStatusCode();

                    var s = await res.Content.ReadAsStringAsync().ConfigureAwait(false);
                    ResolverUtils.HttpResponseMessage(this.logger, s, null);

                    return JSON.Deserialize<CacheItem>(s);
                }
            }
        }

        [TestMethod(TestType.Network)]
        private async Task ImageTest()
        {
            var result = await this.GetContentUrl("http://cl.ly/image/1u1T2k2N2F1L").ConfigureAwait(false);
            result.ItemType.Is("image");
            Assert.True(() => !string.IsNullOrEmpty(result.ContentUrl));
            Assert.True(() => !string.IsNullOrEmpty(result.ThumbnailUrl));
        }

        [TestMethod(TestType.Network)]
        private async Task VideoTest()
        {
            var result = await this.GetContentUrl("http://cl.ly/2V2a2R1E1v3F").ConfigureAwait(false);
            result.ItemType.Is("video");
            Assert.True(() => !string.IsNullOrEmpty(result.ContentUrl));
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
    }
}
