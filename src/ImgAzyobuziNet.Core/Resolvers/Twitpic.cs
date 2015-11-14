using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ImgAzyobuziNet.Core.Test;

namespace ImgAzyobuziNet.Core.Resolvers
{
    public class Twitpic : IResolver
    {
        public string ServiceId => "twitpic";

        public string ServiceName => "Twitpic";

        public string Pattern => @"^https?://(?:www\.)?twitpic\.com/(?:show/\w+/)?(\w+)/?(?:\?.*)?(?:#.*)?$";

        public Task<ImageInfo[]> GetImages(IResolveContext context, Match match)
        {
            var id = match.Groups[1].Value;
            return Task.FromResult(new[] {
                new ImageInfo(
                    "https://twitpic.com/show/large/" + id,
                    "https://twitpic.com/show/large/" + id,
                    "https://twitpic.com/show/thumb/" + id
                )
            });
        }

        [TestMethod(TestType.Static)]
        private static void RegexTest()
        {
            var match = new Twitpic().GetRegex().Match("http://twitpic.com/bh7827");
            Assert.True(() => match.Success);
            match.Groups[1].Value.Is("bh7827");
        }

        [TestMethod(TestType.Static)]
        private static void RegexShowTest()
        {
            var match = new Twitpic().GetRegex().Match("http://twitpic.com/show/large/bfbwoc");
            Assert.True(() => match.Success);
            match.Groups[1].Value.Is("bfbwoc");
        }

        [TestMethod(TestType.Network)]
        private static async Task TestAvailability()
        {
            var t = new Twitpic();
            var i = await t.GetImages(
                new DefaultResolveContext(),
                t.GetRegex().Match("http://twitpic.com/bh7827")
            ).ConfigureAwait(false);

            using (var hc = new HttpClient())
            {
                var res = await hc.GetAsync(i[0].Thumb).ConfigureAwait(false);
                res.EnsureSuccessStatusCode();
                res.Content.Headers.ContentType.MediaType.Is("image/jpeg");
            }
        }
    }
}
