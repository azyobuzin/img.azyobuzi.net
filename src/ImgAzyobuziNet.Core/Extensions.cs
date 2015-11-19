using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace ImgAzyobuziNet.Core
{
    internal static class Extensions
    {
        private static readonly MemoryCacheEntryOptions defaultOptions = new MemoryCacheEntryOptions()
        {
            SlidingExpiration = new TimeSpan(TimeSpan.TicksPerDay)
        };

        internal static object SetWithDefaultExpiration(this IMemoryCache m, object key, object value)
        {
            return m.Set(key, value, defaultOptions);
        }

        internal static TResult[] ConvertAll<TSource, TResult>(this TSource[] source, Func<TSource, TResult> selector)
        {
            var len = source.Length;
            var result = new TResult[len];
            for (var i = 0; i < len; i++)
            {
                result[i] = selector(source[i]);
            }
            return result;
        }
    }
}
