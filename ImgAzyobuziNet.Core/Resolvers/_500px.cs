using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ImgAzyobuziNet.TestFramework;
using Jil;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

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
            Assert.True(() => match.Success);
            match.Groups[1].Value.Is("128754325");
        }

        [TestMethod(TestCategory.Static)]
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
        private readonly IHttpClient _httpClient;
        private readonly IMemoryCache _memoryCache;

        public _500pxResolver(IOptions<ImgAzyobuziNetOptions> options, IHttpClient httpClient, IMemoryCache memoryCache)
        {
            this._options = options.Value;
            this._httpClient = httpClient;
            this._memoryCache = memoryCache;
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
            var req = new HttpRequestMessage(
                HttpMethod.Get,
                "https://api.500px.com/v1/photos/" + id
                    + "?image_size=5&consumer_key=" + this._options._500pxConsumerKey
            );

            using (var res = await this._httpClient.SendAsync(req).ConfigureAwait(false))
            {
                if (res.StatusCode == HttpStatusCode.NotFound)
                    throw new ImageNotFoundException();

                res.EnsureSuccessStatusCode();

                s = await res.Content.ReadAsStringAsync().ConfigureAwait(false);
            }

            return JSON.Deserialize<ApiResponse>(s).photo.image_url;
        }

        #region Tests

        [TestMethod(TestCategory.Network)]
        private async Task FetchTest()
        {
            var imageUrl = await this.Fetch("128836907").ConfigureAwait(false);
            Assert.True(() => !string.IsNullOrEmpty(imageUrl));
        }

        #endregion
    }
}
