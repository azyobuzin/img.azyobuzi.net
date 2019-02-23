using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ImgAzyobuziNet.Core.SupportServices;
using ImgAzyobuziNet.TestFramework;
using Newtonsoft.Json.Linq;

namespace ImgAzyobuziNet.Core.Resolvers
{
    public class PhotozouProvider : PatternProviderBase<PhotozouResolver>
    {
        public override string ServiceId => "Photozou";

        public override string ServiceName => "フォト蔵";

        public override string Pattern => @"^https?://(?:www\.)?photozou\.jp/photo/(?:show|photo_only)/\d+/(\d+)/?(?:[\?#]|$)";

        #region Tests

        [TestMethod(TestCategory.Static)]
        private void RegexShowTest()
        {
            var match = this.GetRegex().Match("http://photozou.jp/photo/show/3175940/260308826");
            Assert.True(() => match.Success);
            match.Groups[1].Value.Is("260308826");
        }

        [TestMethod(TestCategory.Static)]
        private void RegexPhotoOnlyTest()
        {
            var match = this.GetRegex().Match("http://photozou.jp/photo/photo_only/2886762/158535562?size=1024#content");
            Assert.True(() => match.Success);
            match.Groups[1].Value.Is("158535562");
        }

        #endregion
    }

    public class PhotozouResolver : IResolver
    {
        private readonly IImgAzyobuziNetHttpClient _httpClient;
        private readonly IResolverCache _resolverCache;

        public PhotozouResolver(IImgAzyobuziNetHttpClient httpClient, IResolverCache resolverCache)
        {
            this._httpClient = httpClient;
            this._resolverCache = resolverCache;
        }

        public async ValueTask<ImageInfo[]> GetImages(Match match)
        {
            var id = match.Groups[1].Value;
            var result = await this._resolverCache.GetOrSet(
                "photozou-" + id,
                () => this.Fetch(id)
            ).ConfigureAwait(false);

            return new[]
            {
                new ImageInfo(
                    string.IsNullOrEmpty(result.original_image_url) ? result.image_url : result.original_image_url,
                    result.image_url,
                    result.thumbnail_image_url
                )
            };
        }

        private class CacheItem
        {
            public string image_url;
            public string original_image_url;
            public string thumbnail_image_url;
        }

        private async Task<CacheItem> Fetch(string id)
        {
            string json;
            var req = new HttpRequestMessage(
                HttpMethod.Get,
                "https://api.photozou.jp/rest/photo_info.json?photo_id=" + id
            );

            using (var res = await this._httpClient.SendAsync(req).ConfigureAwait(false))
            {
                if (res.StatusCode == HttpStatusCode.BadRequest)
                    throw new ImageNotFoundException();

                res.EnsureSuccessStatusCode();
                json = await res.Content.ReadAsStringAsync().ConfigureAwait(false);
            }

            return JObject.Parse(json)["info"]["photo"].ToObject<CacheItem>();
        }

        #region Tests

        [TestMethod(TestCategory.Network)]
        private async Task FetchTest()
        {
            // http://photozou.jp/photo/show/3230845/257350186
            var result = await this.Fetch("257350186").ConfigureAwait(false);
            result.image_url.NotNullOrEmpty();
            result.original_image_url.NotNullOrEmpty();
            result.thumbnail_image_url.NotNullOrEmpty();
        }

        #endregion
    }
}
