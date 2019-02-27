using System;
using System.Linq;
using System.Reflection;
using ImgAzyobuziNet.Core;
using ImgAzyobuziNet.Core.SupportServices;
using ImgAzyobuziNet.Core.SupportServices.Twitter;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ImgAzyobuziNetServiceCollectionExtensions
    {
        public static IServiceCollection AddImgAzyobuziNetService(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
            return serviceCollection
                .AddTransient(typeof(ImgAzyobuziNetService))
                .Configure<ImgAzyobuziNetOptions>(configuration)
                .Configure<ApiKeyOptions>(configuration.GetSection(nameof(ImgAzyobuziNetOptions.ApiKeys)))
                .Configure<ResolverCacheOptions>(configuration.GetSection(nameof(ImgAzyobuziNetOptions.ResolverCache)));
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

        public static IServiceCollection AddImgAzyobuziNetHttpClient(this IServiceCollection serviceCollection)
        {
            return serviceCollection.AddSingleton(typeof(IImgAzyobuziNetHttpClient), typeof(DefaultHttpClient));
        }

        public static IServiceCollection AddMemoryResolverCache(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient(typeof(IResolverCache), typeof(MemoryResolverCache));
            serviceCollection.TryAddSingleton(typeof(IResolverCacheLogger<>), typeof(DefaultResolverCacheLogger<>));
            return serviceCollection;
        }

        public static IServiceCollection AddNoResolverCache(this IServiceCollection serviceCollection)
        {
            return serviceCollection.AddSingleton(typeof(IResolverCache), typeof(NoResolverCache));
        }

        public static IServiceCollection AddAzureTableStorageResolverCache(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient(typeof(IResolverCache), typeof(AzureTableStorageResolverCache));
            serviceCollection.TryAddSingleton(typeof(IResolverCacheLogger<>), typeof(DefaultResolverCacheLogger<>));
            return serviceCollection;
        }

        private static readonly Lazy<ObjectFactory> s_memoryResolverCacheFactory = new Lazy<ObjectFactory>(
            () => ActivatorUtilities.CreateFactory(typeof(MemoryResolverCache), Type.EmptyTypes));

        private static readonly Lazy<ObjectFactory> s_noResolverCacheFactory = new Lazy<ObjectFactory>(
            () => ActivatorUtilities.CreateFactory(typeof(NoResolverCache), Type.EmptyTypes));

        private static readonly Lazy<ObjectFactory> s_azureTableStorageResolverCacheFactory = new Lazy<ObjectFactory>(
            () => ActivatorUtilities.CreateFactory(typeof(AzureTableStorageResolverCache), Type.EmptyTypes));

        public static IServiceCollection AddResolverByOption(this IServiceCollection serviceCollection)
        {
            serviceCollection.TryAddSingleton(typeof(IResolverCacheLogger<>), typeof(DefaultResolverCacheLogger<>));
            serviceCollection.AddTransient<IResolverCache>(serviceProvider =>
            {
                switch (serviceProvider.GetService<IOptionsMonitor<ResolverCacheOptions>>()?.CurrentValue?.Type)
                {
                    case ResolverCacheType.None:
                        return serviceProvider.GetService<NoResolverCache>()
                            ?? (NoResolverCache)s_noResolverCacheFactory.Value(serviceProvider, Array.Empty<object>());
                    case ResolverCacheType.AzureTableStorage:
                        return serviceProvider.GetService<AzureTableStorageResolverCache>()
                            ?? (AzureTableStorageResolverCache)s_azureTableStorageResolverCacheFactory.Value(serviceProvider, Array.Empty<object>());
                    default:
                        return serviceProvider.GetService<MemoryResolverCache>()
                            ?? (MemoryResolverCache)s_memoryResolverCacheFactory.Value(serviceProvider, Array.Empty<object>());
                }
            });
            return serviceCollection;
        }

        public static IServiceCollection AddTwitterResolver(this IServiceCollection serviceCollection)
        {
            return serviceCollection
                .AddTransient(typeof(ITwitterResolver), typeof(DefaultTwitterResolver))
                .AddSingleton(typeof(TwitterCredentialsManager));
        }
    }
}
