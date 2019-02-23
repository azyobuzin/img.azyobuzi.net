using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ImgAzyobuziNet.TestFramework;
using Shouldly;

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
            match.Success.ShouldBeTrue();
            match.Groups[1].Value.ShouldBe("2eCe");
        }

        [TestMethod(TestCategory.Static)]
        private void RegexShowTest()
        {
            var match = this.GetRegex().Match("http://img.ly/show/large/D6sU");
            match.Success.ShouldBeTrue();
            match.Groups[1].Value.ShouldBe("D6sU");
        }

        #endregion
    }

    public class ImglyResolver : IResolver
    {
        public ValueTask<ImageInfo[]> GetImages(Match match)
        {
            var id = match.Groups[1].Value;
            return new ValueTask<ImageInfo[]>(new[] {
                new ImageInfo(
                    "http://img.ly/show/full/" + id,
                    "http://img.ly/show/large/" + id,
                    "http://img.ly/show/thumb/" + id
                )
            });
        }
    }
}
