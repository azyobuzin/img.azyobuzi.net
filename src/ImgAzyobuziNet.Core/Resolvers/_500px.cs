using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ImgAzyobuziNet.Core.Test;
using Microsoft.Framework.Caching.Memory;
using Newtonsoft.Json.Linq;

namespace ImgAzyobuziNet.Core.Resolvers
{
    public class _500px : IResolver
    {
        public string ServiceId => "500px";

        public string ServiceName => "500px";

        // https://500px.com/photo/{id}/{title}
        public string Pattern => @"^https?://(?:www\.)?500px\.com/photo/(\d+)(?:/.*)?$";

        public async Task<ImageInfo[]> GetImages(IResolveContext context, Match match)
        {
            var id = match.Groups[1].Value;
            var key = "500px-" + id;

            string result;
            if (!context.MemoryCache.TryGetValue(key, out result))
            {
                result = await GetImageUrl(id).ConfigureAwait(false);
                context.MemoryCache.SetWithDefaultExpiration(key, result);
            }

            return new[] { new ImageInfo(result, result, result) };
        }

        private static async Task<string> GetImageUrl(string id)
        {
            using (var hc = new HttpClient())
            {
                var res = await hc.GetAsync(
                    "https://api.500px.com/v1/photos/" + id + "?image_size=5&consumer_key=" + Constants._500pxConsumerKey
                ).ConfigureAwait(false);

                if (res.StatusCode == HttpStatusCode.NotFound)
                    throw new ImageNotFoundException();

                res.EnsureSuccessStatusCode();

                var j = JObject.Parse(await res.Content.ReadAsStringAsync().ConfigureAwait(false));
                return (string)j["photo"]["image_url"];
            }
        }

        [TestMethod(TestType.Static)]
        private static void RegexIdTitle()
        {
            var match = Regex.Match(
                "https://500px.com/photo/128754325/t-v-winter-by-ray-green?ctx_page=1&from=popular",
                new _500px().Pattern);
            match.Success.Is(true);
            match.Groups[1].Value.Is("128754325");
        }

        [TestMethod(TestType.Static)]
        private static void RegexId()
        {
            var match = Regex.Match("https://500px.com/photo/128742743", new _500px().Pattern);
            match.Success.Is(true);
            match.Groups[1].Value.Is("128742743");
        }

        [TestMethod(TestType.Network)]
        private static async Task GetImageUrlTest()
        {
            var imageUrl = await GetImageUrl("128836907").ConfigureAwait(false);
            Assert.True(() => !string.IsNullOrEmpty(imageUrl));
        }
    }
}
