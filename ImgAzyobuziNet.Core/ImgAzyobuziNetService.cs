using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ImgAzyobuziNet.Core
{
    public static class ImgAzyobuziNetService
    {
        private static readonly Lazy<IPatternProvider[]> providers = new Lazy<IPatternProvider[]>(
            () => typeof(ImgAzyobuziNetService).GetTypeInfo().Assembly.GetTypes()
                .Where(x => x.GetTypeInfo().IsClass && typeof(IPatternProvider).IsAssignableFrom(x))
                .Select(x => Activator.CreateInstance(x) as IPatternProvider)
                .ToArray());

        public static IReadOnlyList<IPatternProvider> GetResolvers()
        {
            return providers.Value;
        }

        public static async Task<ResolveResult> Resolve(IServiceProvider serviceProvider, string uri)
        {
            foreach (var p in providers.Value)
            {
                var m = p.GetRegex().Match(uri);
                if (m.Success)
                {
                    try
                    {
                        return new ResolveResult(p, await p.GetResolver(serviceProvider).GetImages(m).ConfigureAwait(false));
                    }
                    catch (Exception ex)
                    {
                        return new ResolveResult(p, ex);
                    }
                }
            }

            return null;
        }
    }
}
