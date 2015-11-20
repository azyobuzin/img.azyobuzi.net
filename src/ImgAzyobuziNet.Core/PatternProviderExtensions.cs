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

        //private static readonly ConcurrentDictionary<Type, ObjectFactory> factoryCache = new ConcurrentDictionary<Type, ObjectFactory>();

        //public static IPatternProvider GetResolver(this IPatternProvider provider, IServiceProvider serviceProvider)
        //{
        //    return (IPatternProvider)factoryCache
        //        .GetOrAdd(provider.ResolverType, x => ActivatorUtilities.CreateFactory(x, Type.EmptyTypes))
        //        .Invoke(serviceProvider, new object[0]);
        //}
    }
}
