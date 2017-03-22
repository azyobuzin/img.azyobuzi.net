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
        private static Assembly[] s_targetAssemblies =
        {
            typeof(ImgAzyobuziNetService).GetTypeInfo().Assembly
        };

        private static TestActivator s_activator = new TestActivator(
            new ServiceCollection()
                .Configure<ImgAzyobuziNetOptions>(
                    new ConfigurationBuilder()
                        .SetBasePath(Path.GetDirectoryName(typeof(TestTargetProvider).GetTypeInfo().Assembly.Location))
                        .AddJsonFile("appsettings.json")
                        .Build()
                        .GetSection("ImgAzyobuziNet")
                )
                .AddLogging()
                .AddMemoryCache()
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
