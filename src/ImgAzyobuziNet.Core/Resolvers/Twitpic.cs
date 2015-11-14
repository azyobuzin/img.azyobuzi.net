using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ImgAzyobuziNet.Core.Resolvers
{
    public class Twitpic : IResolver
    {
        public string ServiceId => "twitpic";

        public string ServiceName => "Twitpic";

        public string Pattern => @"^https?://(?:www\.)?twitpic\.com/(?:show/\w+/)?(\w+)/?(?:\?.*)?(?:#.*)?$";

        public Task<ImageInfo[]> GetImages(IResolveContext context, Match match)
        {
            var id = match.Groups[1].Value;
            return Task.FromResult(new[] {
                new ImageInfo(
                    "https://twitpic.com/show/large/" + id,
                    "https://twitpic.com/show/large/" + id,
                    "https://twitpic.com/show/thumb/" + id
                )
            });
        }
    }
}
