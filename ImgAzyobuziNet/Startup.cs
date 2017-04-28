using System.IO;
using ImgAzyobuziNet.Core;
using ImgAzyobuziNet.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ImgAzyobuziNet
{
    public class Startup
    {
        public IConfigurationRoot Configuration { get; }

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables();

            this.Configuration = builder.Build();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<ImgAzyobuziNetOptions>(this.Configuration.GetSection("ImgAzyobuziNet"))
                .AddMemoryCache()
                .AddMemoryResolverCache()
                .AddHttpClient()
                .AddImgAzyobuziNetService()
                .AddDefaultPatternProviders()
                .AddMvc();
        }

        // Configure is called after ConfigureServices is called.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(LogLevel.Debug);

            app.UseForwardedHeaders();

            app.UseDeveloperExceptionPage();

            // Configure the HTTP request pipeline.
            app.UseMiddleware(typeof(ApiV2Middleware));
            app.UseDefaultFiles().UseStaticFiles();

            // Add MVC to the request pipeline.
            app.UseMvc();
        }

        public static void Main(string[] args) =>
            new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>()
                .Build()
                .Run();
    }
}
