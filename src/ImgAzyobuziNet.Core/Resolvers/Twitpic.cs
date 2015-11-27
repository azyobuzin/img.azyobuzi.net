using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ImgAzyobuziNet.Core.Test;

namespace ImgAzyobuziNet.Core.Resolvers
{
    public class TwitpicProvider : IPatternProvider
    {
        public string ServiceId => "Twitpic";

        public string ServiceName => "Twitpic";

        public string Pattern => @"^https?://(?:www\.)?twitpic\.com/(?:show/\w+/)?(\w+)/?(?:\?.*)?(?:#.*)?$";

        private static readonly ResolverFactory f = PPUtils.CreateFactory<TwitpicResolver>();
        public IResolver GetResolver(IServiceProvider serviceProvider) => f(serviceProvider);
    }

    public class TwitpicResolver : IResolver
    {
        public Task<ImageInfo[]> GetImages(Match match)
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
    }

    class TwitpicTest
    {
        public TwitpicTest(ITestActivator activator)
        {
            this.provider = activator.CreateInstance<TwitpicProvider>();
            this.resolver = activator.CreateInstance<TwitpicResolver>();
        }

        private readonly TwitpicProvider provider;
        private readonly TwitpicResolver resolver;

        [TestMethod(TestType.Static)]
        private void RegexTest()
        {
            var match = this.provider.GetRegex().Match("http://twitpic.com/bh7827");
            Assert.True(() => match.Success);
            match.Groups[1].Value.Is("bh7827");
        }

        [TestMethod(TestType.Static)]
        private void RegexShowTest()
        {
            var match = this.provider.GetRegex().Match("http://twitpic.com/show/large/bfbwoc");
            Assert.True(() => match.Success);
            match.Groups[1].Value.Is("bfbwoc");
        }

        [TestMethod(TestType.Network)]
        private async Task TestAvailability()
        {
            var i = await this.resolver.GetImages(
                this.provider.GetRegex().Match("http://twitpic.com/bh7827")
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
