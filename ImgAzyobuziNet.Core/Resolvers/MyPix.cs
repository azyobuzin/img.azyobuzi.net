using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ImgAzyobuziNet.TestFramework;

namespace ImgAzyobuziNet.Core.Resolvers
{
    public class MyPixProvider : PatternProviderBase<MyPixResolver>
    {
        public override string ServiceId => "MyPix";

        public override string ServiceName => "MyPix";

        public override string Pattern => @"^http://www\.mypix\.jp/app.php/picture/(\d+)(?:/(?:in/?)?)?(?:[\?#]|$)";

        #region Tests

        [TestMethod(TestCategory.Static)]
        private void RegexTest()
        {
            var match = this.GetRegex().Match("http://www.mypix.jp/app.php/picture/64303/in");
            Assert.True(() => match.Success);
            match.Groups[1].Value.Is("64303");
        }

        #endregion
    }

    public class MyPixResolver : IResolver
    {
        public ValueTask<ImageInfo[]> GetImages(Match match)
        {
            var id = match.Groups[1].Value;
            var fullUri = $"http://www.mypix.jp/app.php/picture/{id}/860x0.jpg";
            return new ValueTask<ImageInfo[]>(new[]
            {
                new ImageInfo(fullUri, fullUri, $"http://www.mypix.jp/app.php/picture/{id}/thumb.jpg")
            });
        }
    }
}
