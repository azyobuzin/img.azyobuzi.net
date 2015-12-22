using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ImgAzyobuziNet.Core.Test;

namespace ImgAzyobuziNet.Core.Resolvers
{
    public class ImgurProvider : IPatternProvider
    {
        public string ServiceId => "Imgur";

        public string ServiceName => "Imgur";

        public string Pattern => @"^https?://(?:www\.)?imgur\.com/(?:gallery/|t/memes/)?(\w+)/?(?:[\?#].*)?$";

        private static readonly ResolverFactory f = PPUtils.CreateFactory<ImgurResolver>();
        public IResolver GetResolver(IServiceProvider serviceProvider) => f(serviceProvider);

        #region Tests

        [TestMethod(TestType.Static)]
        private void RegexTest()
        {
            var match = this.GetRegex().Match("http://imgur.com/M5TPafQ");
            Assert.True(() => match.Success);
            match.Groups[1].Value.Is("M5TPafQ");
        }

        [TestMethod(TestType.Static)]
        private void RegexGalleryTest()
        {
            var match = this.GetRegex().Match("http://imgur.com/gallery/NBEIFBz");
            Assert.True(() => match.Success);
            match.Groups[1].Value.Is("NBEIFBz");
        }

        #endregion
    }

    public class ImgurResolver : IResolver
    {
        public Task<ImageInfo[]> GetImages(Match match)
        {
            // 拡張子は何にしても画像本体は返ってくる
            // アニメーション GIF は mp4 で取得可能だけど、チェックしにいく価値がない気がする

            var id = match.Groups[1].Value;
            return Task.FromResult(new[] {
                new ImageInfo(
                    "https://i.imgur.com/" + id + ".jpg",
                    "https://i.imgur.com/" + id + "l.jpg",
                    "https://i.imgur.com/" + id + "s.jpg"
                )
            });
        }
    }
}
