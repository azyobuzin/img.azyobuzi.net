using System;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ImgAzyobuziNet.Core.SupportServices;
using ImgAzyobuziNet.TestFramework;
using Newtonsoft.Json;
using Shouldly;

namespace ImgAzyobuziNet.Core.Resolvers
{
    public class _500pxProvider : PatternProviderBase<_500pxResolver>
    {
        public override string ServiceId => "500px";

        public override string ServiceName => "500px";

        // https://500px.com/photo/{id}/{title}
        public override string Pattern => @"^https?://(?:www\.)?500px\.com/photo/(\d+)(?:[/\?#]|$)";

        #region Tests

        [TestMethod(TestCategory.Static)]
        private void RegexIdTitle()
        {
            var match = this.GetRegex().Match(
                "https://500px.com/photo/128754325/t-v-winter-by-ray-green?ctx_page=1&from=popular");
            match.Success.ShouldBeTrue();
            match.Groups[1].Value.ShouldBe("128754325");
        }

        [TestMethod(TestCategory.Static)]
        private void RegexId()
        {
            var match = this.GetRegex().Match("https://500px.com/photo/128742743");
            match.Success.ShouldBeTrue();
            match.Groups[1].Value.ShouldBe("128742743");
        }

        #endregion
    }

    public class _500pxResolver : IResolver
    {
        private readonly IImgAzyobuziNetHttpClient _httpClient;
        private readonly IResolverCache _resolverCache;

        public _500pxResolver(IImgAzyobuziNetHttpClient httpClient, IResolverCache resolverCache)
        {
            this._httpClient = httpClient;
            this._resolverCache = resolverCache;
        }

        public async ValueTask<ImageInfo[]> GetImages(Match match)
        {
            var id = match.Groups[1].Value;
            var result = await this._resolverCache.GetOrSet(
                "500px-" + id,
                () => this.Fetch(id)
            ).ConfigureAwait(false);

            return new[] { new ImageInfo(result.url, result.url, result.thumbnail_url) };
        }

        private class CacheItem
        {
            public string url;
            public string thumbnail_url;
        }

        private async Task<CacheItem> Fetch(string id)
        {
            string s;
            var req = new HttpRequestMessage(
                HttpMethod.Get,
                "https://500px.com/oembed?url=https%3A%2F%2F500px.com%2Fphoto%2F" + Uri.EscapeDataString(id) + "&format=json"
            );

            using (var res = await this._httpClient.SendAsync(req).ConfigureAwait(false))
            {
                if (res.StatusCode == HttpStatusCode.NotFound)
                    throw new ImageNotFoundException();

                res.EnsureSuccessStatusCode();

                s = await res.Content.ReadAsStringAsync().ConfigureAwait(false);
            }

            return JsonConvert.DeserializeObject<CacheItem>(s);
        }

        #region Tests

        [TestMethod(TestCategory.Network)]
        private async Task FetchTest()
        {
            var result = await this.Fetch("128836907").ConfigureAwait(false);
            result.url.ShouldNotBeNullOrEmpty();
            result.thumbnail_url.ShouldNotBeNullOrEmpty();
        }

        #endregion
    }
}
