using System;
using System.Threading;
using System.Threading.Tasks;
using ImgAzyobuziNet.Core;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Extensibility.PerfCounterCollector.QuickPulse;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ImgAzyobuziNet.AzureFunctions
{
    internal static class FunctionsEnvironment
    {
        static FunctionsEnvironment()
        {
            // Services
            var configuration = new ConfigurationBuilder()
                .AddUserSecrets("ImgAzyobuziNet")
                .AddEnvironmentVariables()
                .Build();

            ServiceProvider = new ServiceCollection()
                .Configure<ImgAzyobuziNetOptions>(configuration)
                .AddMemoryCache()
                .AddMemoryResolverCache() // TODO: キャッシュ方法を設定でプラガブルに
                .AddImgAzyobuziNetHttpClient()
                .AddTwitterResolver()
                .AddImgAzyobuziNetService()
                .AddDefaultPatternProviders()
                .Configure<TelemetryConfiguration>(tc =>
                {
                    tc.TelemetryChannel.DeveloperMode |= IsDevelopment;
                    tc.TelemetryProcessorChainBuilder
                        .UseAdaptiveSampling()
                        .Use(next => new QuickPulseTelemetryProcessor(next))
                        .Build();
                })
                .AddSingleton(typeof(IOptions<TelemetryConfiguration>), typeof(TelemetryConfigurationOptions))
                .AddSingleton(provider => provider.GetService<IOptions<TelemetryConfiguration>>().Value)
                .AddSingleton<TelemetryClient>()
                .AddLogging(builder =>
                {
                    builder.SetMinimumLevel(LogLevel.Information);
                    builder.AddApplicationInsights();
                })
                .BuildServiceProvider();

            TelemetryClient = ServiceProvider.GetRequiredService<TelemetryClient>();
        }

        public static string EnvironmentName
        {
            get
            {
                var s = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                return string.IsNullOrEmpty(s) ? Microsoft.AspNetCore.Hosting.EnvironmentName.Production : s;
            }
        }

        public static bool IsDevelopment
        {
            get
            {
                return string.Equals(
                    EnvironmentName,
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
    }
}
