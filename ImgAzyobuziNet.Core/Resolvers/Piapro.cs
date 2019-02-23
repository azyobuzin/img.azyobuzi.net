using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngleSharp.Html.Dom;
using ImgAzyobuziNet.Core.SupportServices;
using ImgAzyobuziNet.TestFramework;
using Shouldly;

namespace ImgAzyobuziNet.Core.Resolvers
{
    public class PiaproProvider : PatternProviderBase<PiaproResolver>
    {
        public override string ServiceId => "piapro";

        public override string ServiceName => "piapro";

        public override string Pattern => @"^https?://(?:www\.)?piapro\.jp/t/([\w+\-]+)(?:[\?#]|$)";

        #region Tests

        [TestMethod(TestCategory.Static)]
        private void RegexTest()
        {
            var match = this.GetRegex().Match("https://piapro.jp/t/hak-#downloadBox");
            match.Success.ShouldBeTrue();
            match.Groups[1].Value.ShouldBe("hak-");
        }

        #endregion
    }

    public class PiaproResolver : IResolver
    {
        private static readonly Regex s_imageUriPattern = new Regex(@"(?<=_)\d{4}_\d{4}(?=\.\w{3}$)");

        private readonly IImgAzyobuziNetHttpClient _httpClient;
        private readonly IResolverCache _resolverCache;

        public PiaproResolver(IImgAzyobuziNetHttpClient httpClient, IResolverCache resolverCache)
        {
            this._httpClient = httpClient;
            this._resolverCache = resolverCache;
        }

        public async ValueTask<ImageInfo[]> GetImages(Match match)
        {
            var id = match.Groups[1].Value;
            var twitterCardImage = await this._resolverCache.GetOrSet(
                "piapro-" + id,
                () => this.Fetch(id)
            ).ConfigureAwait(false);

            return new[]
            {
                new ImageInfo(
                    s_imageUriPattern.Replace(twitterCardImage, "0740_0500", 1),
                    s_imageUriPattern.Replace(twitterCardImage, "0500_0500", 1),
                    s_imageUriPattern.Replace(twitterCardImage, "0150_0150", 1)
                )
            };
        }

        private async Task<string> Fetch(string id)
        {
            IHtmlDocument document;
            var req = new HttpRequestMessage(HttpMethod.Get, "https://piapro.jp/t/" + id);

            using (var res = await this._httpClient.SendAsync(req).ConfigureAwait(false))
            {
                if (res.StatusCode == HttpStatusCode.NotFound)
                    throw new ImageNotFoundException();

                res.EnsureSuccessStatusCode();
                document = await res.Content.ReadAsHtmlDocument().ConfigureAwait(false);
            }

            var twitterCardImage = document.GetTwitterCardImage();

            // イラストが設定されていないオンガク、テキスト
            if (string.IsNullOrEmpty(twitterCardImage) || twitterCardImage.Contains("//res.piapro.jp/"))
                throw new NotPictureException();

            return twitterCardImage;
        }

        #region Tests

        [TestMethod(TestCategory.Static)]
        private static void ImageUriPatternTest()
        {
            var match = s_imageUriPattern.Match("https://cdn.piapro.jp/thumb_i/r7/r7qk5w0t29yn412b_20190223174953_0500_0500.png");
            match.Success.ShouldBeTrue();
            match.Value.ShouldBe("0500_0500");
        }

        [TestMethod(TestCategory.Network)]
        private async Task FetchIllustTest()
        {
            // https://piapro.jp/t/VkiE
            var result = await this.Fetch("VkiE").ConfigureAwait(false);
            s_imageUriPattern.IsMatch(result).ShouldBeTrue();
        }

        [TestMethod(TestCategory.Network)]
        private async Task FetchMusicWithCoverIllust()
        {
            // https://piapro.jp/t/GECJ
            var result = await this.Fetch("GECJ").ConfigureAwait(false);
            s_imageUriPattern.IsMatch(result).ShouldBeTrue();
        }

        [TestMethod(TestCategory.Network)]
        private Task FetchMusicWithoutCoverIllust()
        {
            // https://piapro.jp/t/aji7
            return Should.ThrowAsync<NotPictureException>(() => this.Fetch("aji7"));
        }

        #endregion
    }
}
