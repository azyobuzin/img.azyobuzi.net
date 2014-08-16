using System.Collections.Generic;
using System.Text.RegularExpressions;
using ImgAzyobuziV3.Core.DataModels;

namespace ImgAzyobuziV3.Core
{
    public interface IResolver
    {
        string ServiceId { get; }
        string ServiceName { get; }
        Regex Pattern { get; }
        string GetId(Match match);
        IReadOnlyCollection<ImageInfo> GetImages(ImgAzyobuziContext context, Match match);
    }

    public abstract class ResolverBase : IResolver
    {
        public abstract string ServiceId { get; }
        public abstract string ServiceName { get; }
        public abstract string PatternString { get; }
        public abstract string GetId(Match match);
        public abstract IReadOnlyCollection<ImageInfo> GetImages(ImgAzyobuziContext context, Match match);

        private Regex pattern;
        public Regex Pattern
        {
            get
            {
                if (this.pattern == null)
                    this.pattern = new Regex(this.PatternString, RegexOptions.IgnoreCase);
                return this.pattern;
            }
        }
    }
}
