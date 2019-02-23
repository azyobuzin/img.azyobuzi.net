using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using ImgAzyobuziNet.Core.SupportServices;

namespace ImgAzyobuziNet.Core
{
    public static class Extensions
    {
        public static async ValueTask<T> GetOrSet<T>(this IResolverCache resolverCache, string key, Func<Task<T>> valueFactory)
        {
            var (exists, value) = await resolverCache.TryGetValue<T>(key).ConfigureAwait(false);
            if (!exists)
            {
                value = await valueFactory().ConfigureAwait(false);
                await resolverCache.Set(key, value).ConfigureAwait(false);
            }
            return value;
        }

        public static TValue GetOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> source, TKey key)
        {
            return source.TryGetValue(key, out TValue value) ? value : default(TValue);
        }

        public static T GetElementById<T>(this INode node, string id) where T : IElement
        {
            return node.Descendents<T>().FirstOrDefault(x => x.Id == id);
        }

        public static string GetTwitterCardImage(this IDocument document)
        {
            return document.Head.ChildNodes
                .OfType<IHtmlMetaElement>()
                .Where(x => x.Name == "twitter:image")
                .Select(x => x.Content)
                .FirstOrDefault(x => !string.IsNullOrEmpty(x));
        }

        public static string GetOpenGraphImage(this IDocument document)
        {
            return document.Head.ChildNodes
                .OfType<IHtmlMetaElement>()
                .Where(x => x.GetAttribute("property") == "og:image")
                .Select(x => x.Content)
                .FirstOrDefault(x => !string.IsNullOrEmpty(x));
        }

        public static async Task<IHtmlDocument> ReadAsHtmlDocument(this HttpContent httpContent)
        {
            // ReadAsStreamAsync returns a MemoryStream.
            using (var stream = await httpContent.ReadAsStreamAsync().ConfigureAwait(false))
                return new HtmlParser().ParseDocument(stream);
        }

        public static void Set<T>(this HttpHeaderValueCollection<T> headers, T value)
            where T : class
        {
            headers.Clear();
            headers.Add(value);
        }
    }
}
