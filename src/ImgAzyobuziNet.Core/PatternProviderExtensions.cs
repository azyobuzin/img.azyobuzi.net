using System;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;

namespace ImgAzyobuziNet.Core
{
    public static class PatternProviderExtensions
    {
        private static readonly ConcurrentDictionary<string, Regex> regexCache = new ConcurrentDictionary<string, Regex>();

        public static Regex GetRegex(this IPatternProvider provider)
        {
            return regexCache.GetOrAdd(provider.Pattern, x => new Regex(x, RegexOptions.IgnoreCase));
        }
    }
}
