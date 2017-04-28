using System.Linq;
using System.Reflection;
using ImgAzyobuziNet.Core;
using ImgAzyobuziNet.Core.SupportServices;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ImgAzyobuziNetServiceCollectionExtensions
    {
        public static IServiceCollection AddImgAzyobuziNetService(this IServiceCollection serviceCollection)
        {
            return serviceCollection.AddTransient(typeof(ImgAzyobuziNetService));
        }

        public static IServiceCollection AddDefaultPatternProviders(this IServiceCollection serviceCollection)
        {
            var patternProviderTypes = typeof(ImgAzyobuziNetService)
                .GetTypeInfo().Assembly.GetTypes()
                .Where(x =>
                {
                    var ti = x.GetTypeInfo();
                    return ti.IsClass && !ti.IsAbstract
                        && typeof(IPatternProvider).IsAssignableFrom(x);
                });

            foreach (var x in patternProviderTypes)
                serviceCollection.AddSingleton(typeof(IPatternProvider), x);

            return serviceCollection;
        }

        public static IServiceCollection AddHttpClient(this IServiceCollection serviceCollection)
        {
            return serviceCollection.AddSingleton(typeof(IHttpClient), typeof(DefaultHttpClient));
        }

        public static IServiceCollection AddMemoryResolverCache(this IServiceCollection serviceCollection)
        {
            return serviceCollection.AddTransient(typeof(IResolverCache), typeof(MemoryResolverCache));
        }
    }
}
