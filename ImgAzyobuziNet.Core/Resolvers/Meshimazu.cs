using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngleSharp.Dom.Html;
using AngleSharp.Extensions;
using ImgAzyobuziNet.TestFramework;
using Microsoft.Extensions.Caching.Memory;

namespace ImgAzyobuziNet.Core.Resolvers
{
    public class MeshimazuProvider : PatternProviderBase<MeshimazuResolver>
    {
        public override string ServiceId => "Meshimazu";

        public override string ServiceName => "メシマズ.net";

        public override string Pattern => @"^http://(?:www\.)?meshimazu\.net/posts/(\d+)/?(?:[\?#]|$)";

        #region Tests

        [TestMethod(TestCategory.Static)]
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
        private readonly IHttpClient _httpClient;
        private readonly IMemoryCache _memoryCache;

        public MeshimazuResolver(IHttpClient httpClient, IMemoryCache memoryCache)
        {
            this._httpClient = httpClient;
            this._memoryCache = memoryCache;
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
            var req = new HttpRequestMessage(
                HttpMethod.Get,
                "http://www.meshimazu.net/posts/" + id
            );

            using (var res = await this._httpClient.SendAsync(req).ConfigureAwait(false))
            {
                if (res.StatusCode == HttpStatusCode.NotFound)
                    throw new ImageNotFoundException();

                res.EnsureSuccessStatusCode();
                document = await res.Content.ReadAsHtmlDocument().ConfigureAwait(false);
            }

            return document.Body.Descendents<IHtmlImageElement>()
                .First(img => img.ClassName == "testes")
                .GetAttribute("src");
        }

        #region Tests

        [TestMethod(TestCategory.Network)]
        private async Task FetchTest()
        {
            // http://www.meshimazu.net/posts/1100
            var result = await this.Fetch("1100").ConfigureAwait(false);
            result.NotNullOrEmpty();
        }

        #endregion
    }
}
