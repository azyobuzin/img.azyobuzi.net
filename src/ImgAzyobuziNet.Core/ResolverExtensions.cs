using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace ImgAzyobuziNet.Core
{
    public static class ResolverExtensions
    {
        private static readonly ConditionalWeakTable<IResolver, Regex> regexCache = new ConditionalWeakTable<IResolver, Regex>();

        public static Regex GetRegex(this IResolver resolver)
        {
            return regexCache.GetValue(resolver, r => new Regex(r.Pattern, RegexOptions.IgnoreCase));
        }
    }
}
