using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngleSharp.Html.Dom;
using ImgAzyobuziNet.Core.SupportServices;
using ImgAzyobuziNet.TestFramework;

namespace ImgAzyobuziNet.Core.Resolvers
{
    public class OwlyProvider : PatternProviderBase<OwlyResolver>
    {
        public override string ServiceId => "Owly";

        public override string ServiceName => "Ow.ly";

        public override string Pattern => @"^http://(?:www\.)?ow\.ly/i/(\w+)(?:/(?:original/?)?)?(?:[\?#]|$)";

        #region Tests

        [TestMethod(TestCategory.Static)]
        private void RegexTest()
        {
            var match = this.GetRegex().Match("http://ow.ly/i/12D4C");
            Assert.True(() => match.Success);
            match.Groups[1].Value.Is("12D4C");
        }

        [TestMethod(TestCategory.Static)]
        private void RegexOriginalTest()
        {
            var match = this.GetRegex().Match("http://ow.ly/i/12D4C/original");
            Assert.True(() => match.Success);
            match.Groups[1].Value.Is("12D4C");
        }

        #endregion
    }

    public class OwlyResolver : IResolver
    {
        private readonly IImgAzyobuziNetHttpClient _httpClient;
        private readonly IResolverCache _resolverCache;

        public OwlyResolver(IImgAzyobuziNetHttpClient httpClient, IResolverCache resolverCache)
        {
            this._httpClient = httpClient;
            this._resolverCache = resolverCache;
        }

        public async ValueTask<ImageInfo[]> GetImages(Match match)
        {
            var id = match.Groups[1].Value;
            var originalUri = await this._resolverCache.GetOrSet(
                "owly-" + id,
                () => this.Fetch(id)
            ).ConfigureAwait(false);

            return new[]
            {
                new ImageInfo(
                    originalUri,
                    $"http://static.ow.ly/photos/normal/{id}.jpg",
                    $"http://static.ow.ly/photos/thumb/{id}.jpg"
                )
            };
        }

        private async Task<string> Fetch(string id)
        {
            // original は拡張子がわからないのでスクレイピングする

            IHtmlDocument document;
            var req = new HttpRequestMessage(HttpMethod.Get, $"http://ow.ly/i/{id}/original");

            using (var res = await this._httpClient.SendAsync(req).ConfigureAwait(false))
            {
                if (res.StatusCode == HttpStatusCode.NotFound)
                    throw new ImageNotFoundException();

                res.EnsureSuccessStatusCode();
                document = await res.Content.ReadAsHtmlDocument().ConfigureAwait(false);
            }

            var originalUri = document.GetElementsByClassName("imageWrapper")
                .FirstOrDefault()
                ?.ChildNodes
                .OfType<IHtmlImageElement>()
                .FirstOrDefault()
                ?.Source;

            if (string.IsNullOrEmpty(originalUri))
                throw new Exception("ow.ly 仕様変更感");

            return originalUri;
        }

        #region Tests

        [TestMethod(TestCategory.Network)]
        private async Task FetchTest()
        {
            // http://ow.ly/i/5argt
            var result = await this.Fetch("5argt").ConfigureAwait(false);
            result.Is("http://static.ow.ly/photos/original/5argt.jpg");
        }

        #endregion
    }
}
