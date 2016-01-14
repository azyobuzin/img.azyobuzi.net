using ImgAzyobuziNet.Core;
using ImgAzyobuziNet.Middlewares;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.HttpOverrides;
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
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables();

            this.Configuration = builder.Build().ReloadOnChanged("appsettings.json");
        }

        // This method gets called by a runtime.
        // Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<ImgAzyobuziNetOptions>(this.Configuration.GetSection("ImgAzyobuziNet"))
                .AddCaching()
                .AddMvc();
        }

        // Configure is called after ConfigureServices is called.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.MinimumLevel = env.IsDevelopment() ? LogLevel.Debug : LogLevel.Warning;
            loggerFactory.AddConsole(LogLevel.Debug);
            loggerFactory.AddDebug(LogLevel.Debug);

            app.UseOverrideHeaders(new OverrideHeaderMiddlewareOptions { ForwardedOptions = ForwardedHeaders.All });

            app.UseDeveloperExceptionPage();

            // Configure the HTTP request pipeline.
            app.UseMiddleware(typeof(ApiV2Middleware));
            app.UseDefaultFiles().UseStaticFiles();

            // Add MVC to the request pipeline.
            app.UseMvc();
        }

        public static void Main(string[] args) => WebApplication.Run<Startup>(args);
    }
}
