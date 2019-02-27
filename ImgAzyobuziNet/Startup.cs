using ImgAzyobuziNet.Middlewares;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.ApplicationInsights;

namespace ImgAzyobuziNet
{
    public class Startup
    {
        internal const string ApiCorsPolicyName = "Api";

        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddMemoryCache()
                .AddResolverByOption()
                .AddImgAzyobuziNetHttpClient()
                .AddTwitterResolver()
                .AddImgAzyobuziNetService(this.Configuration)
                .AddDefaultPatternProviders()
                .AddCors(options => options.AddPolicy(
                    ApiCorsPolicyName,
                    builder => builder.AllowAnyOrigin().AllowAnyMethod()
                ))
                .AddMvc();
        }

        // Configure is called after ConfigureServices is called.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseForwardedHeaders();

            app.UseDeveloperExceptionPage();

            // Configure the HTTP request pipeline.
            app.UseMiddleware(typeof(ApiV2Middleware));
            app.UseDefaultFiles().UseStaticFiles();

            // Add MVC to the request pipeline.
            app.UseMvc();
        }

        public static void Main(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseApplicationInsights()
                .UseStartup<Startup>()
                .ConfigureLogging(builder =>
                {
                    builder.AddApplicationInsights();
                    // 外部への通信は Dependency として記録しているはずなので、 ILogger 経由では記録しない
                    builder.AddFilter<ApplicationInsightsLoggerProvider>("ImgAzyobuziNet.Core.SupportServices.DefaultHttpClient", LogLevel.Warning);
                    builder.AddFilter<ApplicationInsightsLoggerProvider>("ImgAzyobuziNet.Core.SupportServices.Twitter.DefaultTwitterResolver", LogLevel.Warning);
                })
                .Build()
                .Run();
    }
}
