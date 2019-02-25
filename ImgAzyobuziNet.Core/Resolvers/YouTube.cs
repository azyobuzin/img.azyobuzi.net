using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ImgAzyobuziNet.TestFramework;
using Shouldly;

namespace ImgAzyobuziNet.Core.Resolvers
{
    public class YouTubeProvider : PatternProviderBase<YouTubeResolver>
    {
        public override string ServiceId => "YouTube";

        public override string ServiceName => "YouTube";

        public override string Pattern => @"^https?://(?:www\.)?(?:youtube\.com/watch/?\?(?:.+&)?v=([\w\-]+)(?:&|$)|youtu\.be/([\w\-]+)/?(?:[\?#]|$))";

        #region Tests

        [TestMethod(TestCategory.Static)]
        private void RegexTest()
        {
            var match = this.GetRegex().Match("https://www.youtube.com/watch?v=tgerBJynxpw");
            match.Success.ShouldBeTrue();
            match.Groups[1].Value.ShouldBe("tgerBJynxpw");
            match.Groups[2].Success.ShouldBeFalse();
        }

        [TestMethod(TestCategory.Static)]
        private void RegexShortTest()
        {
            var match = this.GetRegex().Match("https://youtu.be/P_D_Iu2rZys");
            match.Success.ShouldBeTrue();
            match.Groups[1].Success.ShouldBeFalse();
            match.Groups[2].Value.ShouldBe("P_D_Iu2rZys");
        }

        #endregion
    }

    public class YouTubeResolver : IResolver
    {
        public ValueTask<ImageInfo[]> GetImages(Match match)
        {
            var id = match.Groups[1].Success ? match.Groups[1].Value : match.Groups[2].Value;
            var hq = $"https://i.ytimg.com/vi/{id}/hqdefault.jpg";
            return new ValueTask<ImageInfo[]>(new[]
            {
                new ImageInfo(hq, hq, $"https://i.ytimg.com/vi/{id}/default.jpg")
            });
        }
    }
}
