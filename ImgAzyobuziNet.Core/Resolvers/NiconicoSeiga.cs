using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngleSharp.Html.Dom;
using ImgAzyobuziNet.Core.SupportServices;
using ImgAzyobuziNet.TestFramework;

namespace ImgAzyobuziNet.Core.Resolvers
{
    public class NiconicoSeigaProvider : PatternProviderBase<NiconicoSeigaResolver>
    {
        public override string ServiceId => "NiconicoSeiga";

        public override string ServiceName => "ニコニコ静画";

        public override string Pattern => @"^https?://(?:seiga\.nicovideo\.jp/(?:seiga|watch)|nico\.(?:ms|sc))/(im|mg)(\d+)/?(?:[\?#]|$)";

        #region Tests

        [TestMethod(TestCategory.Static)]
        private void RegexSeigaTest()
        {
            var match = this.GetRegex().Match("http://seiga.nicovideo.jp/watch/im6025110/");
            Assert.True(() => match.Success);
            match.Groups[1].Value.Is("im");
            match.Groups[2].Value.Is("6025110");
        }

        [TestMethod(TestCategory.Static)]
        private void RegexMangaTest()
        {
            var match = this.GetRegex().Match("http://seiga.nicovideo.jp/watch/mg175730");
            Assert.True(() => match.Success);
            match.Groups[1].Value.Is("mg");
            match.Groups[2].Value.Is("175730");
        }

        #endregion
    }

    public class NiconicoSeigaResolver : IResolver
    {
        private readonly IImgAzyobuziNetHttpClient _httpClient;
        private readonly IResolverCache _resolverCache;

        public NiconicoSeigaResolver(IImgAzyobuziNetHttpClient httpClient, IResolverCache resolverCache)
        {
            this._httpClient = httpClient;
            this._resolverCache = resolverCache;
        }

        public async ValueTask<ImageInfo[]> GetImages(Match match)
        {
            var type = match.Groups[1].Value;
            var id = match.Groups[2].Value;

            if (type == "im")
            {
                return new[]
                {
                    new ImageInfo(
                        $"https://lohas.nicoseiga.jp/thumb/{id}i",
                        $"https://lohas.nicoseiga.jp/thumb/{id}i",
                        $"https://lohas.nicoseiga.jp/thumb/{id}cz"
                    )
                };
            }

            if (type == "mg")
            {
                var mangaThumbnail = await this._resolverCache.GetOrSet(
                    "niconicoseiga-mg" + id,
                    () => this.FetchManga(id)
                ).ConfigureAwait(false);

                return new[] { new ImageInfo(mangaThumbnail, mangaThumbnail, mangaThumbnail) };
            }

            throw new ImageNotFoundException();
        }

        private async Task<string> FetchManga(string id)
        {
            // canonical なアドレスはまだ HTTP っぽい。 HTTPS でも普通に見れるけど
            var req = new HttpRequestMessage(HttpMethod.Get, "http://seiga.nicovideo.jp/watch/mg" + id);
            IHtmlDocument document;

            using (var res = await this._httpClient.SendAsync(req).ConfigureAwait(false))
            {
                if (res.StatusCode == HttpStatusCode.NotFound)
                    throw new ImageNotFoundException();

                res.EnsureSuccessStatusCode();
                document = await res.Content.ReadAsHtmlDocument().ConfigureAwait(false);
            }

            // 未ログイン状態で取得できるのはサムネイルだけ
            var twitterCardImage = document.GetTwitterCardImage();

            // 存在しなくても 200 が返ってくるので、 twitter:image の有無で判断
            if (string.IsNullOrEmpty(twitterCardImage))
                throw new ImageNotFoundException();

            return twitterCardImage;
        }

        #region Tests

        [TestMethod(TestCategory.Network)]
        private async Task FetchMangaTest()
        {
            // http://seiga.nicovideo.jp/watch/mg332323
            var result = await this.FetchManga("332323").ConfigureAwait(false);
            result.NotNullOrEmpty();
            Assert.True(() => Regex.Match(result, "^https?://lohas.nicoseiga.jp/thumb/").Success);
        }

        #endregion
    }
}
