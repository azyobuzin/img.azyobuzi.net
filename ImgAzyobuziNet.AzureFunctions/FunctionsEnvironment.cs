using System;
using System.Threading;
using System.Threading.Tasks;
using ImgAzyobuziNet.Core;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace ImgAzyobuziNet.AzureFunctions
{
    internal static class FunctionsEnvironment
    {
        static FunctionsEnvironment()
        {
            // Application Insights
            var telemetryClientConfiguration = TelemetryConfiguration.CreateDefault();
            if (IsDevelopment)
                telemetryClientConfiguration.TelemetryChannel.DeveloperMode = true;
            TelemetryClient = new TelemetryClient(telemetryClientConfiguration);

            // Services
            var configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .Build();

            ServiceProvider = new ServiceCollection()
                .Configure<ImgAzyobuziNetOptions>(configuration)
                .AddMemoryCache()
                .AddMemoryResolverCache() // TODO: キャッシュ方法を設定でプラガブルに
                .AddImgAzyobuziNetHttpClient()
                .AddImgAzyobuziNetService()
                .AddDefaultPatternProviders()
                .AddSingleton(TelemetryClient)
                .AddLogging()
                .ReplaceWithRequestService<ILoggerFactory>()
                .BuildServiceProvider();
        }

        public static bool IsDevelopment
        {
            get
            {
                return string.Equals(
                    Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
                    "Development",
                    StringComparison.OrdinalIgnoreCase);
            }
        }

        public static TelemetryClient TelemetryClient { get; }

        public static IServiceProvider ServiceProvider { get; }

        public static IServiceScope CreateScope() => ServiceProvider.CreateScope();

        private static readonly AsyncLocal<IServiceProvider> s_requestServices = new AsyncLocal<IServiceProvider>();

        public static void SetContext(HttpContext httpContext)
        {
            s_requestServices.Value = httpContext.RequestServices;
        }

        public static T DoInContext<T>(HttpContext httpContext, Func<ImgAzyobuziNetService, T> action)
        {
            SetContext(httpContext);

            using (var serviceScope = CreateScope())
            {
                return action(serviceScope.ServiceProvider.GetRequiredService<ImgAzyobuziNetService>());
            }
        }

        public static async Task<T> DoInContextAsync<T>(HttpContext httpContext, Func<ImgAzyobuziNetService, Task<T>> action)
        {
            SetContext(httpContext);

            using (var serviceScope = CreateScope())
            {
                // ConfigureAwait(false) しないほうが適切？ わからん
                return await action(serviceScope.ServiceProvider.GetRequiredService<ImgAzyobuziNetService>());
            }
        }

        private static IServiceCollection ReplaceWithRequestService<TService>(this IServiceCollection serviceCollection)
            where TService : class
        {
            return serviceCollection.Replace(ServiceDescriptor.Transient(_ => s_requestServices.Value?.GetService<TService>()));
        }
    }
}
