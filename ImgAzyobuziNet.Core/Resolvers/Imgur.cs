using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ImgAzyobuziNet.TestFramework;
using Shouldly;

namespace ImgAzyobuziNet.Core.Resolvers
{
    public class ImgurProvider : PatternProviderBase<ImgurResolver>
    {
        public override string ServiceId => "Imgur";

        public override string ServiceName => "Imgur";

        public override string Pattern => @"^https?://(?:www\.)?imgur\.com/(?:gallery/|t/memes/)?(\w+)/?(?:[\?#]|$)";

        #region Tests

        [TestMethod(TestCategory.Static)]
        private void RegexTest()
        {
            var match = this.GetRegex().Match("http://imgur.com/M5TPafQ");
            match.Success.ShouldBeTrue();
            match.Groups[1].Value.ShouldBe("M5TPafQ");
        }

        [TestMethod(TestCategory.Static)]
        private void RegexGalleryTest()
        {
            var match = this.GetRegex().Match("http://imgur.com/gallery/NBEIFBz");
            match.Success.ShouldBeTrue();
            match.Groups[1].Value.ShouldBe("NBEIFBz");
        }

        #endregion
    }

    public class ImgurResolver : IResolver
    {
        public ValueTask<ImageInfo[]> GetImages(Match match)
        {
            // 拡張子は何にしても画像本体は返ってくる
            // アニメーション GIF は mp4 で取得可能だけど、チェックしにいく価値がない気がする

            var id = match.Groups[1].Value;
            return new ValueTask<ImageInfo[]>(new[] {
                new ImageInfo(
                    "https://i.imgur.com/" + id + ".jpg",
                    "https://i.imgur.com/" + id + "l.jpg",
                    "https://i.imgur.com/" + id + "s.jpg"
                )
            });
        }
    }
}
