using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ImgAzyobuziNet.Core.Test;

namespace ImgAzyobuziNet.Core.Resolvers
{
    public class DroplrProvider : PatternProviderBase<DroplrResolver>
    {
        public override string ServiceId => "Droplr";

        public override string ServiceName => "Droplr";

        public override string Pattern => @"^https?://d\.pr/(?:([iv])/)?(\w+)\+?(?:/\w+/?)?(?:[\?#].*)?$";

        #region Tests

        [TestMethod(TestType.Static)]
        private void RegexTest()
        {
            var match = this.GetRegex().Match("http://d.pr/i/180AL");
            Assert.True(() => match.Success);
            match.Groups[1].Value.Is("i");
            match.Groups[2].Value.Is("180AL");
        }

        [TestMethod(TestType.Static)]
        private void RegexUnusualTest()
        {
            var match = this.GetRegex().Match("http://d.pr/180AL/download");
            Assert.True(() => match.Success);
            Assert.True(() => !match.Groups[1].Success);
            match.Groups[2].Value.Is("180AL");
        }

        #endregion
    }

    public class DroplrResolver : IResolver
    {
        public Task<ImageInfo[]> GetImages(Match match)
        {
            var typeGroup = match.Groups[1];
            var id = match.Groups[2].Value;
            var result = new ImageInfo();
            if (!typeGroup.Success || typeGroup.Value == "i")
            {
                result.Full = "http://d.pr/i/" + id + "+";
                result.Large = result.Thumb = "http://d.pr/i/" + id + "/thumbnail";
            }
            else
            {
                // サムネイルなんてものはなかった
                // （OEmbed 対応しろよ）
                result.VideoFull = result.VideoLarge = result.VideoMobile = "http://d.pr/v/" + id + "+";
            }
            return Task.FromResult(new[] { result });
        }

        // 無料プランだと一週間で消されるのでテストが書けない
    }
}
