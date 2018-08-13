using System;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ImgAzyobuziNet.Core.SupportServices;
using ImgAzyobuziNet.TestFramework;
using Jil;

namespace ImgAzyobuziNet.Core.Resolvers
{
    public class GyazoProvider : PatternProviderBase<GyazoResolver>
    {
        public override string ServiceId => "Gyazo";

        public override string ServiceName => "Gyazo";

        public override string Pattern => @"^https?://(?:(?:www|i|bot)\.)?gyazo\.com/(\w+)(\.\w+)?(?:[\?#]|$)";

        #region Tests

        [TestMethod(TestCategory.Static)]
        private void RegexTest()
        {
            var match = this.GetRegex().Match("https://gyazo.com/5a924a0a4c83e23436754de2293b646e");
            Assert.True(() => match.Success);
            match.Groups[1].Value.Is("5a924a0a4c83e23436754de2293b646e");
            Assert.True(() => !match.Groups[2].Success);
        }

        [TestMethod(TestCategory.Static)]
        private void WithExtensionTest()
        {
            var match = this.GetRegex().Match("https://bot.gyazo.com/694986837d4524f13a264db302dd2122.gif");
            Assert.True(() => match.Success);
            match.Groups[1].Value.Is("694986837d4524f13a264db302dd2122");
            match.Groups[2].Value.Is(".gif");
        }

        #endregion
    }

    public class GyazoResolver : IResolver
    {
        private readonly IImgAzyobuziNetHttpClient _httpClient;
        private readonly IResolverCache _resolverCache;

        public GyazoResolver(IImgAzyobuziNetHttpClient httpClient, IResolverCache resolverCache)
        {
            this._httpClient = httpClient;
            this._resolverCache = resolverCache;
        }

        public async ValueTask<ImageInfo[]> GetImages(Match match)
        {
            var id = match.Groups[1].Value;
            var uri = new Uri(
                match.Groups[2].Success
                ? match.Value
                : await this._resolverCache.GetOrSet("gyazo-" + id, () => this.Fetch(id)).ConfigureAwait(false)
            );
            var full = uri.GetComponents(UriComponents.SchemeAndServer | UriComponents.Path, UriFormat.UriEscaped);
            var thumb = uri.GetComponents(UriComponents.SchemeAndServer, UriFormat.UriEscaped)
                + "/thumb/180" + uri.AbsolutePath;
            return new[] { new ImageInfo(full, full, thumb) };
        }

        private struct ApiResponse
        {
            public string url;
        }

        private async Task<string> Fetch(string id)
        {
            string s;
            var req = new HttpRequestMessage(
                HttpMethod.Get,
                "https://api.gyazo.com/api/oembed?url="
                    + Uri.EscapeDataString("http://gyazo.com/" + id)
            );

            using (var res = await this._httpClient.SendAsync(req).ConfigureAwait(false))
            {
                if (res.StatusCode == HttpStatusCode.NotFound)
                    throw new ImageNotFoundException();

                res.EnsureSuccessStatusCode();
                s = await res.Content.ReadAsStringAsync().ConfigureAwait(false);
            }

            return JSON.Deserialize<ApiResponse>(s).url;
        }

        #region Tests

        [TestMethod(TestCategory.Network)]
        private async Task FetchTest()
        {
            var result = await this.Fetch("5a924a0a4c83e23436754de2293b646e");
            Assert.True(() => result.EndsWith("/5a924a0a4c83e23436754de2293b646e.png", StringComparison.Ordinal));
        }

        #endregion
    }
}
