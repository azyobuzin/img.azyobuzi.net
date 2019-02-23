using System.Collections.Generic;
using System.IO;
using System.Reflection;
using ImgAzyobuziNet.TestFramework;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ImgAzyobuziNet.Core.Test
{
    public class TestTargetProvider : ITestTargetProvider
    {
        private static readonly Assembly[] s_targetAssemblies =
        {
            typeof(ImgAzyobuziNetService).GetTypeInfo().Assembly
        };

        private static readonly TestActivator s_activator = new TestActivator(
            new ServiceCollection()
                .Configure<ImgAzyobuziNetOptions>(options =>
                {
                    var config = new ConfigurationBuilder()
                        .SetBasePath(Path.GetDirectoryName(typeof(TestTargetProvider).GetTypeInfo().Assembly.Location))
                        .AddJsonFile("appsettings.json", true)
                        .AddJsonFile("appsettings.Development.json", true)
                        .AddUserSecrets("ImgAzyobuziNet")
                        .AddEnvironmentVariables()
                        .Build();
                    config.Bind(options);
                })
                .AddLogging()
                .AddNoResolverCache()
                .AddImgAzyobuziNetHttpClient()
                .BuildServiceProvider()
        );

        public IEnumerable<Assembly> GetTargetAssemblies()
        {
            return s_targetAssemblies;
        }

        public ITestActivator GetActivator()
        {
            return s_activator;
        }
    }
}
