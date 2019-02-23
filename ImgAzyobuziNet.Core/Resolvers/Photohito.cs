using System;
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
    public class PhotohitoProvider : PatternProviderBase<PhotohitoResolver>
    {
        public override string ServiceId => "Photohito";

        public override string ServiceName => "PHOTOHITO";

        public override string Pattern => @"^https?://(?:www\.)?photohito\.com/photo/(\d+)/?(?:[\?#]|$)";

        #region Tests

        [TestMethod(TestCategory.Static)]
        private void RegexTest()
        {
            var match = this.GetRegex().Match("https://photohito.com/photo/8395855/");
            match.Success.ShouldBeTrue();
            match.Groups[1].Value.ShouldBe("8395855");
        }

        #endregion
    }

    public class PhotohitoResolver : IResolver
    {
        private readonly IImgAzyobuziNetHttpClient _httpClient;
        private readonly IResolverCache _resolverCache;

        public PhotohitoResolver(IImgAzyobuziNetHttpClient httpClient, IResolverCache resolverCache)
        {
            this._httpClient = httpClient;
            this._resolverCache = resolverCache;
        }

        public async ValueTask<ImageInfo[]> GetImages(Match match)
        {
            var id = match.Groups[1].Value;
            var ogImage = await this._resolverCache.GetOrSet(
                "photohito-" + id,
                () => this.Fetch(id)
            ).ConfigureAwait(false);

            var baseUri = ogImage.Substring(0, ogImage.Length - 6);
            return new[]
            {
                new ImageInfo(baseUri + "_o.jpg", baseUri + "_m.jpg", baseUri + "_s.jpg")
            };
        }

        private async Task<string> Fetch(string id)
        {
            IHtmlDocument document;
            var req = new HttpRequestMessage(HttpMethod.Get, $"https://photohito.com/photo/{id}/");

            using (var res = await this._httpClient.SendAsync(req).ConfigureAwait(false))
            {
                if (res.StatusCode == HttpStatusCode.NotFound)
                    throw new ImageNotFoundException();

                res.EnsureSuccessStatusCode();
                document = await res.Content.ReadAsHtmlDocument().ConfigureAwait(false);
            }

            var ogImage = document.GetOpenGraphImage();

            if (string.IsNullOrEmpty(ogImage))
                throw new ImageNotFoundException();

            if (!Regex.IsMatch(ogImage, @"_[smo]\.jpg$"))
                throw new Exception(ogImage);

            return ogImage;
        }

        #region Tests

        [TestMethod(TestCategory.Network)]
        private async Task FetchTest()
        {
            // https://photohito.com/photo/8433605/
            var result = await this.Fetch("8433605").ConfigureAwait(false);
            result.ShouldNotBeNullOrEmpty();
        }

        #endregion
    }
}
