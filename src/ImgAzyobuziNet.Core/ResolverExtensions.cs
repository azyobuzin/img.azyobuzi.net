using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace ImgAzyobuziNet.Core
{
    public static class ResolverExtensions
    {
        private static readonly ConcurrentDictionary<string, Regex> regexCache = new ConcurrentDictionary<string, Regex>();

        public static Regex GetRegex(this IResolver resolver)
        {
            return regexCache.GetOrAdd(resolver.Pattern, x => new Regex(x, RegexOptions.IgnoreCase));
        }
    }
}
