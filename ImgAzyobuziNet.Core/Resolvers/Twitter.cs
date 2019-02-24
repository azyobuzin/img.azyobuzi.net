using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ImgAzyobuziNet.Core.SupportServices;
using ImgAzyobuziNet.Core.SupportServices.Twitter;
using ImgAzyobuziNet.TestFramework;
using Shouldly;

namespace ImgAzyobuziNet.Core.Resolvers
{
    public class TwitterProvider : PatternProviderBase<TwitterResolver>
    {
        public override string ServiceId => "Twitter";

        public override string ServiceName => "Twitter";

        public override string Pattern => @"^(?:(https?://(?:\w+\.twimg\.com/media|p\.twimg\.com)/[\w\-]+\.\w+)(?::\w+)?|https?://(?:www\.)?twitter\.com/(?:#!/)?\w+/status(?:es)?/(\d+)/photo/\d+(?:/(?:\w+/?)?)?)(?:[\?#]|$)";

        #region Tests

        [TestMethod(TestCategory.Static)]
        private void RegexTest()
        {
            var match = this.GetRegex().Match("https://twitter.com/azyobuzin/status/881401298305794048/photo/1?s=09");
            match.Success.ShouldBeTrue();
            match.Groups[1].Success.ShouldBeFalse();
            match.Groups[2].Value.ShouldBe("881401298305794048");
        }

        [TestMethod(TestCategory.Static)]
        private void RegexDirectLinkTest()
        {
            var match = this.GetRegex().Match("https://pbs.twimg.com/media/DDtdzdDUwAAf5dk.jpg:large?foo");
            match.Success.ShouldBeTrue();
            match.Groups[1].Value.ShouldBe("https://pbs.twimg.com/media/DDtdzdDUwAAf5dk.jpg");
            match.Groups[2].Success.ShouldBeFalse();
        }

        #endregion
    }

    public class TwitterResolver : IResolver
    {
        private readonly IResolverCache _resolverCache;
        private readonly ITwitterResolver _twitterResolver;

        public TwitterResolver(IResolverCache resolverCache, ITwitterResolver twitterResolver)
        {
            this._resolverCache = resolverCache;
            this._twitterResolver = twitterResolver;
        }

        public async ValueTask<ImageInfo[]> GetImages(Match match)
        {
            if (match.Groups[1].Success)
            {
                var baseUri = match.Groups[1].Value;
                return new[]
                {
                    new ImageInfo(baseUri + ":orig", baseUri, baseUri + ":thumb")
                };
            }

            var id = match.Groups[2].Value;
            var result = await this._resolverCache.GetOrSet(
                "twitter-" + id,
                () => this._twitterResolver.GetImagesByStatusIdAsync(id)
            ).ConfigureAwait(false);

            if (result.Length == 0)
                throw new NotPictureException();

            return result;
        }
    }
}
