using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace ImgAzyobuziNet.Core
{
    public static class Extensions
    {
        private static readonly MemoryCacheEntryOptions defaultOptions = new MemoryCacheEntryOptions
        {
            SlidingExpiration = new TimeSpan(TimeSpan.TicksPerDay)
        };

        internal static object SetWithDefaultExpiration(this IMemoryCache m, object key, object value)
        {
            return m.Set(key, value, defaultOptions);
        }

        internal static async Task<T> GetOrSet<T>(this IMemoryCache m, object key, Func<Task<T>> valueFactory)
        {
            T result;
            if (!m.TryGetValue(key, out result))
            {
                result = await valueFactory().ConfigureAwait(false);
                m.SetWithDefaultExpiration(key, result);
            }
            return result;
        }

        public static TResult[] ConvertAll<TSource, TResult>(this TSource[] source, Func<TSource, TResult> selector)
        {
            var len = source.Length;
            var result = new TResult[len];
            for (var i = 0; i < len; i++)
            {
                result[i] = selector(source[i]);
            }
            return result;
        }

        public static TResult[] ConvertAll<TSource, TResult>(this IReadOnlyCollection<TSource> source, Func<TSource, TResult> selector)
        {
            var array = source as TSource[];
            if (array != null) return ConvertAll(array, selector);

            var count = source.Count;
            var result = new TResult[count];
            if (count == 0) return result;
            using (var enumerator = source.GetEnumerator())
            {
                for (var i = 0; i < count; i++)
                {
                    if (!enumerator.MoveNext())
                        throw new InvalidOperationException();
                    result[i] = selector(enumerator.Current);
                }
            }
            return result;
        }

        public static TValue GetOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> source, TKey key)
        {
            TValue value;
            return source.TryGetValue(key, out value) ? value : default(TValue);
        }
    }
}
