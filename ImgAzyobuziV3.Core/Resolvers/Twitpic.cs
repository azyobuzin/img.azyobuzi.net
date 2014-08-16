using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Text.RegularExpressions;
using ImgAzyobuziV3.Core.DataModels;

namespace ImgAzyobuziV3.Core.Resolvers
{
    [Export(typeof(IResolver))]
    public class Twitpic : ResolverBase
    {
        public override string ServiceId
        {
            get { return "twitpic"; }
        }

        public override string ServiceName
        {
            get { return "Twitpic"; }
        }

        public override string PatternString
        {
            get { return @"^https?://(?:www\.)?twitpic\.com/(?:show/\w+/)?(\w+)/?(?:\?.*)?(?:#.*)?$"; }
        }

        public override string GetId(Match match)
        {
            return match.Groups[1].Value;
        }

        public override IReadOnlyCollection<ImageInfo> GetImages(ImgAzyobuziContext context, Match match)
        {
            var id = this.GetId(match);
            return new[]
            {
                new ImageInfo(
                    "https://twitpic.com/show/large/" + id,
                    "https://twitpic.com/show/large/" + id,
                    "https://twitpic.com/show/thumb/" + id
                )
            };
        }
    }
}
