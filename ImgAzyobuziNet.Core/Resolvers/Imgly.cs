using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ImgAzyobuziNet.TestFramework;

namespace ImgAzyobuziNet.Core.Resolvers
{
    public class ImglyProvider : PatternProviderBase<ImglyResolver>
    {
        public override string ServiceId => "imgly";

        public override string ServiceName => "img.ly";

        public override string Pattern => @"^https?://(?:www\.)?img\.ly/(?:show/\w+/)?(\w+)/?(?:[\?#]|$)";

        #region Tests

        [TestMethod(TestCategory.Static)]
        private void RegexTest()
        {
            var match = this.GetRegex().Match("http://img.ly/2eCe");
            Assert.True(() => match.Success);
            match.Groups[1].Value.Is("2eCe");
        }

        [TestMethod(TestCategory.Static)]
        private void RegexShowTest()
        {
            var match = this.GetRegex().Match("http://img.ly/show/large/D6sU");
            Assert.True(() => match.Success);
            match.Groups[1].Value.Is("D6sU");
        }

        #endregion
    }

    public class ImglyResolver : IResolver
    {
        public Task<ImageInfo[]> GetImages(Match match)
        {
            var id = match.Groups[1].Value;
            return Task.FromResult(new[] {
                new ImageInfo(
                    "http://img.ly/show/full/" + id,
                    "http://img.ly/show/large/" + id,
                    "http://img.ly/show/thumb/" + id
                )
            });
        }
    }
}
