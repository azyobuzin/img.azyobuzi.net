using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ImgAzyobuziNet.Core.Test;
using Microsoft.Framework.Caching.Memory;

namespace ImgAzyobuziNet.Core.Resolvers
{
    public class Cameran : IResolver
    {
        // http://cameran.in/posts/get/v1/{hex} は現存するものが見つからないのでサポートやめます

        public string Pattern => @"^http://cameran\.in/p/v1/(\w+)/?(?:\?.*)?(?:#.*)?$";

        public string ServiceId => "cameran";

        public string ServiceName => "cameran";

        public async Task<ImageInfo[]> GetImages(IResolveContext context, Match match)
        {
            var id = match.Groups[1].Value;
            var key = "cameran-" + id;

            string result;
            if (!context.MemoryCache.TryGetValue(key, out result))
            {
                result = await GetOgImage(id).ConfigureAwait(false);
                context.MemoryCache.SetWithDefaultExpiration(key, result);
            }

            return new[] { new ImageInfo(result, result, result) };
        }

        private static async Task<string> GetOgImage(string id)
        {
            using (var hc = new HttpClient())
            {
                var res = await hc.GetAsync("http://cameran.in/p/v1/" + id).ConfigureAwait(false);

                switch (res.StatusCode)
                {
                    case HttpStatusCode.NotFound:
                    case HttpStatusCode.Found:
                    case HttpStatusCode.SeeOther:
                        throw new ImageNotFoundException();
                }

                res.EnsureSuccessStatusCode();

                return await ParseUtils.GetOgImage(await res.Content.ReadAsStreamAsync().ConfigureAwait(false)).ConfigureAwait(false);
            }
        }

        [TestMethod(TestType.Static)]
        private static void RegexTest()
        {
            var match = new Cameran().GetRegex().Match("http://cameran.in/p/v1/3hbTqO2U4W");
            Assert.True(() => match.Success);
            match.Groups[1].Value.Is("3hbTqO2U4W");
        }

        [TestMethod(TestType.Network)]
        private static async Task GetOgImageTest()
        {
            var image = await GetOgImage("3hbTqO2U4W").ConfigureAwait(false);
            Assert.True(() => !string.IsNullOrEmpty(image));
        }
    }
}
