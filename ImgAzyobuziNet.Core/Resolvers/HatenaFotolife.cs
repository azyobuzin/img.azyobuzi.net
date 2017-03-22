using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngleSharp.Dom.Html;
using AngleSharp.Extensions;
using ImgAzyobuziNet.TestFramework;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace ImgAzyobuziNet.Core.Resolvers
{
    public class HatenaFotolifeProvider : PatternProviderBase<HatenaFotolifeResolver>
    {
        public override string ServiceId => "HatenaFotolife";

        public override string ServiceName => "はてなフォトライフ";

        public override string Pattern => @"^http://f\.hatena\.ne\.jp/(\w+)/(\d{14})(?:[\?#]|$)";

        #region Tests

        [TestMethod(TestCategory.Static)]
        private void RegexTest()
        {
            var match = this.GetRegex().Match("http://f.hatena.ne.jp/azyobuzin_test/20090502111522");
            Assert.True(() => match.Success);
            match.Groups[1].Value.Is("azyobuzin_test");
            match.Groups[2].Value.Is("20090502111522");
        }

        #endregion
    }

    public class HatenaFotolifeResolver : IResolver
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger _logger;

        public HatenaFotolifeResolver(IMemoryCache memoryCache, ILogger<HatenaFotolifeResolver> logger)
        {
            this._memoryCache = memoryCache;
            this._logger = logger;
        }

        public async Task<ImageInfo[]> GetImages(Match match)
        {
            var username = match.Groups[1].Value;
            var id = match.Groups[2].Value;
            var info = await this._memoryCache.GetOrSet(
                "hatenafotolife-" + username + "/" + id,
                () => this.Fetch(username, id)
            ).ConfigureAwait(false);

            var result = new ImageInfo();
            var baseUri = "http://cdn-ak.f.st-hatena.com/images/fotolife/" + username.Substring(0, 1) + "/" + username + "/" + id.Substring(0, 8) + "/" + id;

            if (info.Extension == "flv")
            {
                result.VideoFull = result.VideoLarge = result.VideoMobile = baseUri + ".flv";
                result.Full = result.Large = baseUri + ".jpg";
            }
            else
            {
                result.Large = baseUri + "." + info.Extension;
                result.Full = info.IsOriginalAvailable
                    ? baseUri + "_original." + info.Extension
                    : result.Large;
            }

            result.Thumb = baseUri + "_120.jpg";

            return new[] { result };
        }

        private class CacheItem
        {
            public string Extension;
            public bool IsOriginalAvailable;
        }

        private async Task<CacheItem> Fetch(string username, string id)
        {
            IHtmlDocument document;
            using (var hc = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false }))
            {
                var requestUri = "http://f.hatena.ne.jp/" + username + "/" + id;
                ResolverUtils.RequestingMessage(this._logger, requestUri, null);

                using (var res = await hc.GetAsync(requestUri).ConfigureAwait(false))
                {
                    switch (res.StatusCode)
                    {
                        case HttpStatusCode.NotFound:
                        case HttpStatusCode.Found:
                        case HttpStatusCode.SeeOther:
                            throw new ImageNotFoundException();
                    }

                    res.EnsureSuccessStatusCode();
                    document = await res.Content.ReadAsHtmlDocument().ConfigureAwait(false);
                }
            }

            var fotoBody = document.GetElementById("foto-body");
            var img = fotoBody.GetElementById<IHtmlImageElement>("foto-for-html-tag-" + id);

            // <img id="foto-for-html-tag-" src="" style="display:none;" class="" alt="" title="" />
            if (img == null) throw new ImageNotFoundException();

            return new CacheItem
            {
                Extension = img.GetAttribute("class"),
                IsOriginalAvailable = fotoBody.Descendents<IHtmlImageElement>()
                    .Any(x => x.GetAttribute("src") == "/images/original.gif")
            };
        }

        #region Tests

        [TestMethod(TestCategory.Network)]
        private async Task FetchTest()
        {
            // http://f.hatena.ne.jp/azyobuzin/20150412015830
            var result = await this.Fetch("azyobuzin", "20150412015830").ConfigureAwait(false);
            result.Extension.Is("png");
            result.IsOriginalAvailable.Is(true);
        }

        [TestMethod(TestCategory.Network)]
        private async Task FetchVideoTest()
        {
            // http://f.hatena.ne.jp/azyobuzin_test/20070423171636
            var result = await this.Fetch("azyobuzin_test", "20070423171636").ConfigureAwait(false);
            result.Extension.Is("flv");
            result.IsOriginalAvailable.Is(false);
        }

        #endregion
    }
}
