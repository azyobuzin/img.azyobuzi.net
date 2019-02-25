using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ImgAzyobuziNet.Core.SupportServices;
using ImgAzyobuziNet.TestFramework;
using Newtonsoft.Json;
using Shouldly;

namespace ImgAzyobuziNet.Core.Resolvers
{
    public class VineProvider : PatternProviderBase<VineResolver>
    {
        public override string ServiceId => "Vine";

        public override string ServiceName => "Vine";

        public override string Pattern => @"^https?://(?:www\.)?vine\.co/v/(\w+)(?:[\?#]|$)";

        #region Tests

        [TestMethod(TestCategory.Static)]
        private void RegexTest()
        {
            var match = this.GetRegex().Match("https://vine.co/v/OZ6UgtjQveY");
            match.Success.ShouldBeTrue();
            match.Groups[1].Value.ShouldBe("OZ6UgtjQveY");
        }

        #endregion
    }

    public class VineResolver : IResolver
    {
        private readonly IImgAzyobuziNetHttpClient _httpClient;
        private readonly IResolverCache _resolverCache;

        public VineResolver(IImgAzyobuziNetHttpClient httpClient, IResolverCache resolverCache)
        {
            this._httpClient = httpClient;
            this._resolverCache = resolverCache;
        }

        public async ValueTask<ImageInfo[]> GetImages(Match match)
        {
            var id = match.Groups[1].Value;
            var result = await this._resolverCache.GetOrSet(
                "vine-" + id,
                () => this.Fetch(id)
            ).ConfigureAwait(false);

            if (result == null) throw new ImageNotFoundException();

            return new[]
            {
                new ImageInfo(
                    result.thumbnailUrl,
                    result.thumbnailUrl,
                    result.thumbnailUrl,
                    result.videoUrl,
                    result.videoUrl,
                    result.videoLowURL
                )
            };
        }

        private class CacheItem
        {
            public string thumbnailUrl;
            public string videoUrl;
            public string videoLowURL;
        }

        private async Task<CacheItem> Fetch(string id)
        {
            string json;
            var req = new HttpRequestMessage(
                HttpMethod.Get,
                $"https://archive.vine.co/posts/{id}.json"
            );

            using (var res = await this._httpClient.SendAsync(req).ConfigureAwait(false))
            {
                switch (res.StatusCode)
                {
                    case HttpStatusCode.Forbidden:
                    case HttpStatusCode.NotFound:
                        // Vine は新規投稿できないのでネガティブキャッシュを保存できる
                        return null;
                }

                res.EnsureSuccessStatusCode();
                json = await res.Content.ReadAsStringAsync().ConfigureAwait(false);
            }

            var result = JsonConvert.DeserializeObject<CacheItem>(json);

            result.thumbnailUrl.ShouldNotBeEmpty();
            result.videoUrl.ShouldNotBeEmpty();
            result.videoLowURL.ShouldNotBeEmpty();

            return result;
        }

        #region Tests

        [TestMethod(TestCategory.Network)]
        private async Task FetchTest()
        {
            // https://vine.co/v/OL6wwTOKjY1
            var result = await this.Fetch("OL6wwTOKjY1").ConfigureAwait(false);
            result.ShouldNotBeNull();
            // 各フィールドに値が入っていることは、 Fetch メソッドで確認済み
        }

        [TestMethod(TestCategory.Network)]
        private async Task FetchNotFoundTest()
        {
            // Vine は新規投稿されることがないので、今存在しない ID は今後存在しない
            var result = await this.Fetch("9DhtW59Zu3M").ConfigureAwait(false);
            result.ShouldBeNull();
        }

        #endregion
    }
}
