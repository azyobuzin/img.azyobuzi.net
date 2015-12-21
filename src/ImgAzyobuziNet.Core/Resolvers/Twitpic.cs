using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ImgAzyobuziNet.Core.Test;

namespace ImgAzyobuziNet.Core.Resolvers
{
    public class TwitpicProvider : IPatternProvider
    {
        public string ServiceId => "Twitpic";

        public string ServiceName => "Twitpic";

        public string Pattern => @"^https?://(?:www\.)?twitpic\.com/(?:show/\w+/)?(\w+)/?(?:[\?#].*)?$";

        private static readonly ResolverFactory f = PPUtils.CreateFactory<TwitpicResolver>();
        public IResolver GetResolver(IServiceProvider serviceProvider) => f(serviceProvider);

        #region Tests

        [TestMethod(TestType.Static)]
        private void RegexTest()
        {
            var match = this.GetRegex().Match("http://twitpic.com/bh7827");
            Assert.True(() => match.Success);
            match.Groups[1].Value.Is("bh7827");
        }

        [TestMethod(TestType.Static)]
        private void RegexShowTest()
        {
            var match = this.GetRegex().Match("http://twitpic.com/show/large/bfbwoc");
            Assert.True(() => match.Success);
            match.Groups[1].Value.Is("bfbwoc");
        }

        #endregion
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
}
