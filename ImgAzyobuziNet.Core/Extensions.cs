using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AngleSharp.Dom.Html;
using AngleSharp.Extensions;
using AngleSharp.Parser.Html;
using Microsoft.Extensions.Caching.Memory;

namespace ImgAzyobuziNet.Core
{
    public static class Extensions
    {
        private static readonly MemoryCacheEntryOptions defaultOptions = new MemoryCacheEntryOptions
        {
            SlidingExpiration = new TimeSpan(TimeSpan.TicksPerDay)
        };

        [Obsolete("あとでサービス作る")]
        internal static object SetWithDefaultExpiration(this IMemoryCache m, object key, object value)
        {
            return m.Set(key, value, defaultOptions);
        }

        [Obsolete("あとでサービス作る")]
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

        public static TValue GetOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> source, TKey key)
        {
            return source.TryGetValue(key, out TValue value) ? value : default(TValue);
        }

        public static T GetElementById<T>(this INode node, string id) where T : IElement
        {
            return node.Descendents<T>().FirstOrDefault(x => x.Id == id);
        }

        public static async Task<IHtmlDocument> ReadAsHtmlDocument(this HttpContent httpContent)
        {
            // ReadAsStreamAsync returns a MemoryStream.
            using (var stream = await httpContent.ReadAsStreamAsync().ConfigureAwait(false))
                return new HtmlParser().Parse(stream);
        }

        public static void Set<T>(this HttpHeaderValueCollection<T> headers, T value)
            where T : class
        {
            headers.Clear();
            headers.Add(value);
        }
    }
}
