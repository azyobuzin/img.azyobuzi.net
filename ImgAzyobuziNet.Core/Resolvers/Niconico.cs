using System;
using System.Globalization;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using ImgAzyobuziNet.Core.SupportServices;
using ImgAzyobuziNet.TestFramework;

namespace ImgAzyobuziNet.Core.Resolvers
{
    public class NiconicoProvider : PatternProviderBase<NiconicoResolver>
    {
        public override string ServiceId => "niconico";

        public override string ServiceName => "ニコニコ動画";

        public override string Pattern => @"^https?://(?:(?:www\.)?nicovideo\.jp/watch|nico\.(?:ms|sc))/([sn]m\d+)/?(?:[\?#]|$)";

        #region Tests

        [TestMethod(TestCategory.Static)]
        private void RegexLongTest()
        {
            var match = this.GetRegex().Match("https://www.nicovideo.jp/watch/sm34610104/");
            Assert.True(() => match.Success);
            match.Groups[1].Value.Is("sm34610104");
        }

        [TestMethod(TestCategory.Static)]
        private void RegexShortTest()
        {
            var match = this.GetRegex().Match("https://nico.ms/sm28073785?ref=twitter");
            Assert.True(() => match.Success);
            match.Groups[1].Value.Is("sm28073785");
        }

        [TestMethod(TestCategory.Static)]
        private void RegexNiconicoMovieMakerTest()
        {
            var match = this.GetRegex().Match("https://www.nicovideo.jp/watch/nm2829323");
            Assert.True(() => match.Success);
            match.Groups[1].Value.Is("nm2829323");
        }

        #endregion
    }

    public class NiconicoResolver : IResolver
    {
        private readonly IImgAzyobuziNetHttpClient _httpClient;
        private readonly IResolverCache _resolverCache;

        public NiconicoResolver(IImgAzyobuziNetHttpClient httpClient, IResolverCache resolverCache)
        {
            this._httpClient = httpClient;
            this._resolverCache = resolverCache;
        }

        public async ValueTask<ImageInfo[]> GetImages(Match match)
        {
            var id = match.Groups[1].Value;
            var thumbnail = await this._resolverCache.GetOrSet(
                "niconico-" + id,
                () => this.Fetch(id)
            ).ConfigureAwait(false);

            var largeThumbnail = int.Parse(id.Substring(2), CultureInfo.InvariantCulture) >= 16371850
                ? thumbnail + ".L"
                : thumbnail;

            return new[]
            {
                new ImageInfo(largeThumbnail, largeThumbnail, thumbnail)
            };
        }

        private async Task<string> Fetch(string id)
        {
            string s;
            var req = new HttpRequestMessage(
                HttpMethod.Get,
                "https://ext.nicovideo.jp/api/getthumbinfo/" + id
            );

            using (var res = await this._httpClient.SendAsync(req).ConfigureAwait(false))
            {
                res.EnsureSuccessStatusCode();
                s = await res.Content.ReadAsStringAsync().ConfigureAwait(false);
            }

            var root = XElement.Parse(s);

            if (root.Attribute("status")?.Value != "ok")
            {
                var errorCode = root.Element("error")?.Element("code")?.Value;

                if (errorCode == "NOT_FOUND")
                    throw new ImageNotFoundException();

                if (!string.IsNullOrEmpty(errorCode))
                {
                    var description = root.Element("error").Element("description")?.Value;
                    throw new NiconicoErrorResponseException(errorCode, description, s);
                }
            }

            var thumbnailUrl = root.Element("thumb")?.Element("thumbnail_url")?.Value;

            if (string.IsNullOrEmpty(thumbnailUrl))
                throw new NiconicoErrorResponseException(null, null, s);

            return thumbnailUrl;
        }

        public class NiconicoErrorResponseException : Exception
        {
            public NiconicoErrorResponseException(string code, string description, string body)
                : base(CreateMessage(code, description, body))
            { }

            private static string CreateMessage(string code, string description, string body)
            {
                return string.IsNullOrEmpty(code)
                    ? body
                    : $"Error response {code} {description}";
            }
        }

        #region Tests

        [TestMethod(TestCategory.Network)]
        private async Task FetchTest()
        {
            // https://www.nicovideo.jp/watch/sm30202776
            var result = await this.Fetch("sm30202776").ConfigureAwait(false);
            result.NotNullOrEmpty();
        }

        #endregion
    }
}
