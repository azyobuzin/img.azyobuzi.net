using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace ImgAzyobuziNet.Core
{
    public static class PatternProviderExtensions
    {
        private static readonly ConcurrentDictionary<string, Regex> s_regexCache = new ConcurrentDictionary<string, Regex>();

        public static Regex GetRegex(this IPatternProvider provider)
        {
            return s_regexCache.GetOrAdd(provider.Pattern, x => new Regex(x, RegexOptions.IgnoreCase));
        }
    }
}
