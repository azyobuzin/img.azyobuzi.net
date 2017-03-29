using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ImgAzyobuziNet.TestFramework;
using Microsoft.Extensions.Caching.Memory;

namespace ImgAzyobuziNet.Core.Resolvers
{
    public class CameranProvider : PatternProviderBase<CameranResolver>
    {
        // http://cameran.in/posts/get/v1/{hex} は現存するものが見つからないのでサポートやめます

        public override string ServiceId => "cameran";

        public override string ServiceName => "cameran";

        public override string Pattern => @"^http://cameran\.in/p/v1/(\w+)/?(?:[\?#]|$)";

        #region Tests

        [TestMethod(TestCategory.Static)]
        private void RegexTest()
        {
            var match = this.GetRegex().Match("http://cameran.in/p/v1/3hbTqO2U4W");
            Assert.True(() => match.Success);
            match.Groups[1].Value.Is("3hbTqO2U4W");
        }

        #endregion
    }

    public class CameranResolver : IResolver
    {
        private readonly IHttpClient _httpClient;
        private readonly IMemoryCache _memoryCache;

        public CameranResolver(IHttpClient httpClient, IMemoryCache memoryCache)
        {
            this._httpClient = httpClient;
            this._memoryCache = memoryCache;
        }

        public async Task<ImageInfo[]> GetImages(Match match)
        {
            var id = match.Groups[1].Value;
            var result = await this._memoryCache.GetOrSet(
                "cameran-" + id,
                () => this.Fetch(id)
            ).ConfigureAwait(false);

            return new[] { new ImageInfo(result, result, result) };
        }

        private async Task<string> Fetch(string id)
        {
            var req = new HttpRequestMessage(HttpMethod.Get, "http://cameran.in/p/v1/" + id);

            using (var res = await this._httpClient.SendAsync(req, false).ConfigureAwait(false))
            {
                switch (res.StatusCode)
                {
                    case HttpStatusCode.NotFound:
                    case HttpStatusCode.Found:
                    case HttpStatusCode.SeeOther:
                        throw new ImageNotFoundException();
                }

                res.EnsureSuccessStatusCode();

                return ResolverUtils.GetOgImage(
                    await res.Content.ReadAsHtmlDocument().ConfigureAwait(false));
            }
        }

        #region Tests

        [TestMethod(TestCategory.Network)]
        private async Task FetchTest()
        {
            var image = await this.Fetch("3hbTqO2U4W").ConfigureAwait(false);
            Assert.True(() => !string.IsNullOrEmpty(image));
        }

        #endregion
    }
}
