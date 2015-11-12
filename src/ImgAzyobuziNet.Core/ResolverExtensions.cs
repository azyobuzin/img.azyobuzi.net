using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace ImgAzyobuziNet.Core
{
    public static class ResolverExtensions
    {
        private static readonly ConcurrentDictionary<IResolver, Regex> regexCache = new ConcurrentDictionary<IResolver, Regex>();

        public static Regex GetRegex(this IResolver resolver)
        {
            return regexCache.GetOrAdd(resolver, r => new Regex(r.Pattern, RegexOptions.IgnoreCase));
        }
    }
}
