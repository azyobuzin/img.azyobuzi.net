using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ImgAzyobuziNet.TestFramework;
using Shouldly;

namespace ImgAzyobuziNet.Core.Resolvers
{
    public class PixivProvider : PatternProviderBase<PixivResolver>
    {
        public override string ServiceId => "pixiv";

        public override string ServiceName => "pixiv";

        public override string Pattern => @"^https?://(?:www\.)?pixiv\.net/(?:index|member_illust)\.php\?(?:.*&)?illust_id=(\d+)(?:&|$)";

        #region Tests

        [TestMethod(TestCategory.Static)]
        private void RegexTest()
        {
            var match = this.GetRegex().Match("https://www.pixiv.net/member_illust.php?mode=medium&illust_id=70507077");
            match.Success.ShouldBeTrue();
            match.Groups[1].Value.ShouldBe("70507077");
        }

        #endregion
    }

    public class PixivResolver : IResolver
    {
        public ValueTask<ImageInfo[]> GetImages(Match match)
        {
            var id = match.Groups[1].Value;
            var result = "https://embed.pixiv.net/decorate.php?illust_id=" + id;
            return new ValueTask<ImageInfo[]>(new[]
            {
                new ImageInfo(result, result, result)
            });
        }
    }
}
