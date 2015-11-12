using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ImgAzyobuziNet.Core
{
    public static class ImgAzyobuziNetService
    {
        private static readonly Lazy<IResolver[]> resolvers = new Lazy<IResolver[]>(
            () => typeof(ImgAzyobuziNetService).GetTypeInfo().Assembly.GetTypes()
                .Where(x => x.GetTypeInfo().IsClass && typeof(IResolver).IsAssignableFrom(x))
                .Select(x => Activator.CreateInstance(x) as IResolver)
                .ToArray());

        public static IReadOnlyList<IResolver> GetResolvers()
        {
            return resolvers.Value;
        }

        public static async Task<ResolveResult> Resolve(IResolveContext context, string uri)
        {
            foreach (var r in resolvers.Value)
            {
                var m = r.GetRegex().Match(uri);
                if (m.Success)
                {
                    try
                    {
                        return new ResolveResult(r, await r.GetImages(context, m).ConfigureAwait(false));
                    }
                    catch (Exception ex)
                    {
                        return new ResolveResult(r, ex);
                    }
                }
            }

            return null;
        }
    }
}
