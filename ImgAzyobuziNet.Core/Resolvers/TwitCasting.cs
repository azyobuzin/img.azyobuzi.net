using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ImgAzyobuziNet.TestFramework;
using Shouldly;

namespace ImgAzyobuziNet.Core.Resolvers
{
    public class TwitCastingProvider : PatternProviderBase<TwitCastingResolver>
    {
        public override string ServiceId => "TwitCasting";

        public override string ServiceName => "ツイキャス";

        public override string Pattern => @"^https?://(?:(?:www|ssl)\.)?twitcasting\.tv/(\w+)(?:/movie/(\d+))?(?:[\?#]|$)";

        #region Tests

        [TestMethod(TestCategory.Static)]
        private void RegexUserTest()
        {
            var match = this.GetRegex().Match("https://twitcasting.tv/azyobuzin");
            match.Success.ShouldBeTrue();
            match.Groups[1].Value.ShouldBe("azyobuzin");
            match.Groups[2].Success.ShouldBeFalse();
        }

        [TestMethod(TestCategory.Static)]
        private void RegexMovieTest()
        {
            var match = this.GetRegex().Match("https://ssl.twitcasting.tv/kb10uy/movie/519721682");
            match.Success.ShouldBeTrue();
            match.Groups[1].Value.ShouldBe("kb10uy");
            match.Groups[2].Value.ShouldBe("519721682");
        }

        #endregion
    }

    public class TwitCastingResolver : IResolver
    {
        public ValueTask<ImageInfo[]> GetImages(Match match)
        {
            var user = match.Groups[1].Value;

            if (match.Groups[2].Success)
            {
                var movieId = match.Groups[2].Value;
                var thumbnail = $"https://ssl.twitcasting.tv/{user}/twimage/{movieId}";
                var movie = $"https://dl.twitcasting.tv/{user}/download/{movieId}?dl=1";

                return new ValueTask<ImageInfo[]>(new[]
                {
                    new ImageInfo(thumbnail, thumbnail, thumbnail, movie, movie, movie)
                });
            }
            else
            {
                var thumbnail = $"https://twitcasting.tv/{user}/thumbstream/liveshot";
                var thumbnailSmall = $"http://twitcasting.tv/{user}/thumbstream/liveshot-1";

                return new ValueTask<ImageInfo[]>(new[]
                {
                    new ImageInfo(thumbnail, thumbnail, thumbnailSmall)
                });
            }
        }
    }
}
