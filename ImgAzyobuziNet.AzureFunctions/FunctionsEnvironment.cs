using System;
using System.Threading;
using System.Threading.Tasks;
using ImgAzyobuziNet.Core;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.ApplicationInsights;

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

            var hostingEnvironment = new HostingEnvironment()
            {
                EnvironmentName = EnvironmentName,
                ApplicationName = "ImgAzyobuziNet",
            };

            ServiceProvider = new ServiceCollection()
                .Configure<ImgAzyobuziNetOptions>(configuration)
                .AddMemoryCache()
                .AddMemoryResolverCache() // TODO: キャッシュ方法を設定でプラガブルに
                .AddImgAzyobuziNetHttpClient()
                .AddTwitterResolver()
                .AddImgAzyobuziNetService()
                .AddDefaultPatternProviders()
                .AddSingleton((Microsoft.AspNetCore.Hosting.IHostingEnvironment)hostingEnvironment)
                .AddSingleton((Microsoft.Extensions.Hosting.IHostingEnvironment)hostingEnvironment)
                .AddApplicationInsightsTelemetry(options => options.DeveloperMode |= IsDevelopment)
                .AddLogging(builder =>
                {
                    builder.AddApplicationInsights();
                    // 外部への通信は Dependency として記録しているはずなので、 ILogger 経由では記録しない
                    builder.AddFilter<ApplicationInsightsLoggerProvider>("ImgAzyobuziNet.Core.SupportServices.DefaultHttpClient", LogLevel.Warning);
                    builder.AddFilter<ApplicationInsightsLoggerProvider>("ImgAzyobuziNet.Core.SupportServices.Twitter.DefaultTwitterResolver", LogLevel.Warning);
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
