// TODO: URL がそもそも変わってる
/*
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;
using ImgAzyobuziNet.TestFramework;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace ImgAzyobuziNet.Core.Resolvers
{
    public class CanonImageGatewayProvider : PatternProviderBase<CanonImageGatewayResolver>
    {
        public override string ServiceId => "CanonImageGateway";

        public override string ServiceName => "CANON iMAGE GATEWAY";

        public override string Pattern => @"^https?://opa\.cig2\.imagegateway\.net/s/([tm]/)?(?:album/)?(\w+(?:/\w+)?)/?(?:[\?#].*)?$";

        #region Tests

        [TestMethod(TestCategory.Static)]
        private void RegexTest()
        {
            var regex = this.GetRegex();

            {
                var match = regex.Match("http://opa.cig2.imagegateway.net/s/t/HRMnimvRE5w/J0sCcbx3To0L1iXx");
                Assert.True(() => match.Success);
                match.Groups[1].Value.Is("t/");
                match.Groups[2].Value.Is("HRMnimvRE5w/J0sCcbx3To0L1iXx");
            }

            {
                var match = regex.Match("http://opa.cig2.imagegateway.net/s/HRMnimvRE5w/J0sCcbx3To0L1iXx");
                Assert.True(() => match.Success);
                match.Groups[1].Value.Is("");
                match.Groups[2].Value.Is("HRMnimvRE5w/J0sCcbx3To0L1iXx");
            }
        }

        [TestMethod(TestCategory.Static)]
        private void RegexAlbumTest()
        {
            var regex = this.GetRegex();

            {
                var match = regex.Match("http://opa.cig2.imagegateway.net/s/t/album/CuNSrxxj4VV");
                Assert.True(() => match.Success);
                match.Groups[1].Value.Is("t/");
                match.Groups[2].Value.Is("CuNSrxxj4VV");
            }

            {
                var match = regex.Match("http://opa.cig2.imagegateway.net/s/m/HPJmdf4wwci");
                Assert.True(() => match.Success);
                match.Groups[1].Value.Is("m/");
                match.Groups[2].Value.Is("HPJmdf4wwci");
            }
        }

        #endregion
    }

    public class CanonImageGatewayResolver : IResolver
    {
        private readonly IMemoryCache _resolverCache;
        private readonly ILogger _logger;

        public CanonImageGatewayResolver(IMemoryCache resolverCache, ILogger<CanonImageGatewayResolver> logger)
        {
            this._resolverCache = resolverCache;
            this._logger = logger;
        }

        public async ValueTask<ImageInfo[]> GetImages(Match match)
        {
            var t = match.Groups[1].Value;
            var id = match.Groups[2].Value;
            var key = "cig-" + id;

            // スラッシュが含まれていれば単一画像、そうでなければアルバムとして処理する

            if (id.Contains("/"))
            {
                var result = await this._resolverCache.GetOrSet(key,
                    () => this.GetImage(t, id)).ConfigureAwait(false);
                return new[] { new ImageInfo(result, result, result) };
            }

            var thumbs = await this._resolverCache.GetOrSet(key,
                () => this.GetAlbumThumbnails(t, id)).ConfigureAwait(false);
            return thumbs.ConvertAll(x => new ImageInfo(x, x, x));
        }

        private static void CheckResponseError(HttpResponseMessage res, string content)
        {
            if (res.StatusCode == HttpStatusCode.NotFound || content.Contains("該当するデータはありません。"))
                throw new ImageNotFoundException();
        }

        private async Task<string> GetImage(string t, string id)
        {
            // フルサイズ画像は毎回 URI が変わるのでキャッシュしたら死ぬ可能性
            // よってアルバムのサムネイル一覧から取得

            string content;
            using (var hc = new HttpClient())
            {
                var requestUri = "http://opa.cig2.imagegateway.net/s/" + t + id;
                ResolverUtils.RequestingMessage(this._logger, requestUri, null);
                var res = await hc.GetAsync(requestUri).ConfigureAwait(false);
                content = await res.Content.ReadAsStringAsync().ConfigureAwait(false);
                CheckResponseError(res, content);
            }

            var style = new HtmlParser().Parse(content)
                .GetElementById("itemList")
                .ChildNodes.OfType<IHtmlListItemElement>()
                .First(x => x.ClassList.Contains("itemOver"))
                .ChildNodes.OfType<IHtmlSpanElement>().First()
                .ChildNodes.OfType<IHtmlSpanElement>().First()
                .GetAttribute("style");

            // background-image:url(here);
            return style.Substring(21, style.Length - 23);
        }

        private async Task<string[]> GetAlbumThumbnails(string t, string id)
        {
            string content;
            using (var hc = new HttpClient())
            {
                var requestUri = "http://opa.cig2.imagegateway.net/s/" + t + "album/" + id;
                ResolverUtils.RequestingMessage(this._logger, requestUri, null);
                var res = await hc.GetAsync(requestUri).ConfigureAwait(false);
                content = await res.Content.ReadAsStringAsync().ConfigureAwait(false);
                CheckResponseError(res, content);
            }

            return new HtmlParser().Parse(content)
                .GetElementById("albumAdd1")
                .ChildNodes.OfType<IHtmlListItemElement>()
                .Select(li =>
                {
                    var style = li.ChildNodes.OfType<IHtmlAnchorElement>().First()
                        .ChildNodes.OfType<IHtmlDivElement>().First()
                        .GetAttribute("style");
                    // background-image: url(here);
                    return style.Substring(22, style.Length - 24);
                })
                .ToArray();
        }

        #region Tests

        [TestMethod(TestCategory.Network)]
        private async Task GetImageTest()
        {
            // http://opa.cig2.imagegateway.net/s/t/CuNSrxxj4VV/3DKkfXRnqrCq0cac
            var image = await this.GetImage("t/", "CuNSrxxj4VV/3DKkfXRnqrCq0cac").ConfigureAwait(false);
            Assert.True(() => !string.IsNullOrEmpty(image));
        }

        [TestMethod(TestCategory.Network)]
        private async Task GetAlbumThumbnailsTest()
        {
            // http://opa.cig2.imagegateway.net/s/m/album/DTe7EWihYUt
            var thumbs = await this.GetAlbumThumbnails("m/", "DTe7EWihYUt").ConfigureAwait(false);
            thumbs.Length.Is(52);
            foreach (var x in thumbs) Assert.True(() => !string.IsNullOrEmpty(x));
        }

        #endregion
    }
}
*/
