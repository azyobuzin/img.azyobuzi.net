using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ImgAzyobuziNet.Core.SupportServices;
using ImgAzyobuziNet.TestFramework;

namespace ImgAzyobuziNet.Core.Resolvers
{
    public class MovapicProvider : PatternProviderBase<MovapicResolver>
    {
        public override string ServiceId => "Movapic";

        public override string ServiceName => "携帯百景";

        public override string Pattern => @"^http://(?:www\.)?movapic\.com/(?:(\w+)/pic/(\d+)|pic/(\w+))/?(?:[\?#]|$)";

        #region Tests

        [TestMethod(TestCategory.Static)]
        private void RegexUserAndSequentialIdTest()
        {
            var match = this.GetRegex().Match("http://movapic.com/boubun/pic/5399239/");
            Assert.True(() => match.Success);
            match.Groups[1].Value.Is("boubun");
            match.Groups[2].Value.Is("5399239");
            Assert.True(() => !match.Groups[3].Success);
        }

        [TestMethod(TestCategory.Static)]
        private void RegexTimestampAndIdTest()
        {
            var match = this.GetRegex().Match("http://movapic.com/pic/201902072352025c5cc4a2958d6");
            Assert.True(() => match.Success);
            match.Groups[3].Value.Is("201902072352025c5cc4a2958d6");
        }

        #endregion
    }

    public class MovapicResolver : IResolver
    {
        private readonly IImgAzyobuziNetHttpClient _httpClient;
        private readonly IResolverCache _resolverCache;

        public MovapicResolver(IImgAzyobuziNetHttpClient httpClient, IResolverCache resolverCache)
        {
            this._httpClient = httpClient;
            this._resolverCache = resolverCache;
        }

        public async ValueTask<ImageInfo[]> GetImages(Match match)
        {
            string id;

            if (match.Groups[3].Success)
            {
                id = match.Groups[3].Value;
            }
            else
            {
                var userName = match.Groups[1].Value;
                var sequentialId = match.Groups[2].Value;

                id = await this._resolverCache.GetOrSet(
                    "movapic-" + sequentialId,
                    () => this.Fetch(userName, sequentialId)
                ).ConfigureAwait(false);
            }

            var fullUri = $"http://image.movapic.com/pic/m_{id}.jpeg";
            return new[]
            {
                new ImageInfo(fullUri, fullUri, $"http://image.movapic.com/pic/t_{id}.jpeg")
            };
        }

        private async Task<string> Fetch(string userName, string sequentialId)
        {
            string s;
            var req = new HttpRequestMessage(
                HttpMethod.Get,
                $"http://movapic.com/{userName}/pic/{sequentialId}"
            );

            using (var res = await this._httpClient.SendAsync(req).ConfigureAwait(false))
            {
                res.EnsureSuccessStatusCode();
                s = await res.Content.ReadAsStringAsync().ConfigureAwait(false);
            }

            var match = Regex.Match(s, @"<img class=""image"" src=""http://image\.movapic\.com/pic/m_(\w+)\.jpeg""/>");

            // エラーページは 200
            if (!match.Success)
                throw new ImageNotFoundException();

            return match.Groups[1].Value;
        }

        #region Tests

        [TestMethod(TestCategory.Network)]
        private async Task FetchTest()
        {
            // http://movapic.com/boubun/pic/5399239
            var result = await this.Fetch("boubun", "5399239").ConfigureAwait(false);
            result.NotNullOrEmpty();
        }

        #endregion
    }
}
