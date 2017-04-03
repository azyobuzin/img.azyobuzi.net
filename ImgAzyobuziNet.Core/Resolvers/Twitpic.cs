using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ImgAzyobuziNet.TestFramework;

namespace ImgAzyobuziNet.Core.Resolvers
{
    public class TwitpicProvider : PatternProviderBase<TwitpicResolver>
    {
        public override string ServiceId => "Twitpic";

        public override string ServiceName => "Twitpic";

        public override string Pattern => @"^https?://(?:www\.)?twitpic\.com/(?:show/\w+/)?(\w+)/?(?:[\?#]|$)";

        #region Tests

        [TestMethod(TestCategory.Static)]
        private void RegexTest()
        {
            var match = this.GetRegex().Match("http://twitpic.com/bh7827");
            Assert.True(() => match.Success);
            match.Groups[1].Value.Is("bh7827");
        }

        [TestMethod(TestCategory.Static)]
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
        public ValueTask<ImageInfo[]> GetImages(Match match)
        {
            var id = match.Groups[1].Value;
            return new ValueTask<ImageInfo[]>(new[] {
                new ImageInfo(
                    "https://twitpic.com/show/large/" + id,
                    "https://twitpic.com/show/large/" + id,
                    "https://twitpic.com/show/thumb/" + id
                )
            });
        }
    }
}
