using System;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using ImgAzyobuziNet.Core.SupportServices;
using ImgAzyobuziNet.TestFramework;
using Microsoft.Extensions.Options;
using Shouldly;

namespace ImgAzyobuziNet.Core.Resolvers
{
    public class TinamiProvider : PatternProviderBase<TinamiResolver>
    {
        public override string ServiceId => "Tinami";

        public override string ServiceName => "TINAMI";

        public override string Pattern => @"^https?://(?:www\.)?tinami\.(?:com|jp)/(?:view/(\d+)/?|([a-z0-9]+))(?:[\?#]|$)";

        #region Tests

        [TestMethod(TestCategory.Static)]
        private void RegexShortTest()
        {
            var match = this.GetRegex().Match("http://tinami.jp/l40c");
            match.Success.ShouldBeTrue();
            match.Groups[1].Success.ShouldBeFalse();
            match.Groups[2].Value.ShouldBe("l40c");
        }

        [TestMethod(TestCategory.Static)]
        private void RegexViewTest()
        {
            var match = this.GetRegex().Match("https://www.tinami.com/view/985001");
            match.Success.ShouldBeTrue();
            match.Groups[1].Value.ShouldBe("985001");
            match.Groups[2].Success.ShouldBeFalse();
        }

        #endregion
    }

    public class TinamiResolver : IResolver
    {
        private readonly string _apiKey;
        private readonly IImgAzyobuziNetHttpClient _httpClient;
        private readonly IResolverCache _resolverCache;

        public TinamiResolver(IOptionsSnapshot<ApiKeyOptions> options, IImgAzyobuziNetHttpClient httpClient, IResolverCache resolverCache)
        {
            this._apiKey = options?.Value?.TinamiApiKey;
            this._httpClient = httpClient;
            this._resolverCache = resolverCache;
        }

        public async ValueTask<ImageInfo[]> GetImages(Match match)
        {
            if (string.IsNullOrEmpty(this._apiKey))
                throw new NotConfiguredException(nameof(ImgAzyobuziNetOptions.ApiKeys) + ":" + nameof(ApiKeyOptions.TinamiApiKey));

            var id = match.Groups[1].Value;
            var result = await this._resolverCache.GetOrSet(
                "tinami-" + id,
                () => this.Fetch(id)
            ).ConfigureAwait(false);

            if ((result.ImageUrls?.Length ?? 0) == 0)
            {
                if (string.IsNullOrEmpty(result.ThumbnailUrl))
                    throw new NotPictureException();

                var thumb = WithScheme(result.ThumbnailUrl);
                return new[] { new ImageInfo(thumb, thumb, thumb) };
            }

            var imageInfoArray = Array.ConvertAll(result.ImageUrls, x =>
            {
                var s = WithScheme(x);
                return new ImageInfo(s, s, s);
            });

            if (!string.IsNullOrEmpty(result.ThumbnailUrl))
                imageInfoArray[0].Thumb = WithScheme(result.ThumbnailUrl);

            return imageInfoArray;
        }

        private static string ToLongId(string shortId)
        {
            uint longId = 0;

            foreach (var c in shortId)
            {
                checked
                {
                    longId *= 36;

                    if (c >= '0' && c <= '9')
                        longId += (uint)c - '0';
                    else if (c >= 'a' && c <= 'z')
                        longId += 10 + (uint)c - 'a';
                    else
                        throw new FormatException();
                }
            }

            return longId.ToString(CultureInfo.InvariantCulture);
        }

        private static string WithScheme(string url)
        {
            return url.StartsWith("//", StringComparison.Ordinal)
                ? "https:" + url
                : url;
        }

        private class CacheItem
        {
            public string ThumbnailUrl;
            public string[] ImageUrls;
        }

        private async Task<CacheItem> Fetch(string id)
        {
            XElement rootElement;
            var req = new HttpRequestMessage(
                HttpMethod.Get,
                string.Format(
                    "https://www.tinami.com/api/content/info?api_key={0}&cont_id={1}",
                    Uri.EscapeDataString(this._apiKey),
                    id
                )
            );

            using (var res = await this._httpClient.SendAsync(req).ConfigureAwait(false))
            {
                res.EnsureSuccessStatusCode();
                rootElement = XElement.Load(await res.Content.ReadAsStreamAsync().ConfigureAwait(false));
            }

            var stat = rootElement.Attribute("stat")?.Value;
            if (stat == "fail") throw new ImageNotFoundException();
            if (stat != "ok") throw new Exception(rootElement.Element("err")?.Attribute("msg")?.Value ?? stat ?? "Invalid response");

            var contentElement = rootElement.Element("content");
            var thumbnail = contentElement.Element("thumbnails")?.Elements().FirstOrDefault()?.Attribute("url").Value;

            string[] images = null;
            if (contentElement.Element("images") is XElement imagesElement)
            {
                // マンガ、モデル
                images = imagesElement.Elements("image")
                    .Select(x => x.Element("url").Value)
                    .ToArray();
            }
            else if (contentElement.Element("image") is XElement imageElement)
            {
                // イラスト
                images = new[] { imageElement.Element("url").Value };
            }

            return new CacheItem()
            {
                ThumbnailUrl = thumbnail,
                ImageUrls = images,
            };
        }

        #region Tests

        [TestMethod(TestCategory.Static)]
        private static void ToLongIdTest()
        {
            ToLongId("l40c").ShouldBe("984972");
        }

        [TestMethod(TestCategory.Network)]
        private async Task FetchIllustTest()
        {
            // https://www.tinami.com/view/985001
            var result = await this.Fetch("985001").ConfigureAwait(false);
            result.ImageUrls.Length.ShouldBe(1);
            result.ThumbnailUrl.ShouldNotBeNullOrEmpty();
        }

        [TestMethod(TestCategory.Network)]
        private async Task FetchMangaTest()
        {
            // https://www.tinami.com/view/912648
            var result = await this.Fetch("912648").ConfigureAwait(false);
            result.ImageUrls.Length.ShouldBe(11);
            result.ThumbnailUrl.ShouldNotBeEmpty();
        }

        [TestMethod(TestCategory.Network)]
        private async Task FetchNovelTest()
        {
            // https://www.tinami.com/view/473326
            var result = await this.Fetch("473326").ConfigureAwait(false);
            result.ImageUrls.ShouldBeNull();
            result.ThumbnailUrl.ShouldNotBeNullOrEmpty();
        }

        #endregion
    }
}
