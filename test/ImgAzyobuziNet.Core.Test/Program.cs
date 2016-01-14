using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ImgAzyobuziNet.Core.Test
{
    public class Program
    {
        public void Main(string[] args)
        {
            TestType type;
            var methods = new List<string>();

            if (args.Length == 0)
                type = TestType.Static;
            else if (args.Contains("all", StringComparer.OrdinalIgnoreCase))
                type = TestType.Static | TestType.Network;
            else
            {
                type = 0;
                foreach (var x in args)
                {
                    if (x.Contains("."))
                        methods.Add(x);
                    else
                        type |= (TestType)Enum.Parse(typeof(TestType), x, true);
                }
            }

            var testMethods = typeof(ImgAzyobuziNetService).GetTypeInfo().Assembly.DefinedTypes
                .SelectMany(t => t.DeclaredMethods.Where(m =>
                {
                    var attr = m.GetCustomAttribute<TestMethodAttribute>();
                    if (attr == null) return false;
                    if (type.HasFlag(attr.Type)) return true;
                    var s = m.DeclaringType.Name + "." + m.Name;
                    return methods.Any(x => string.Equals(x, s, StringComparison.OrdinalIgnoreCase));
                }))
                .ToArray();

            Console.WriteLine("{0} tests will be run.", testMethods.Length);

            var serviceProvider = BuildServiceProvider();
            var lf = serviceProvider.GetRequiredService<ILoggerFactory>();
            lf.MinimumLevel = LogLevel.Debug;
            lf.AddConsole(LogLevel.Debug);

            var instanceCache = new Dictionary<Type, object>();
            var stopwatch = new Stopwatch();
            var failedCount = 0;

            foreach (var m in testMethods)
            {
                var testName = m.DeclaringType.Name + "." + m.Name;

                if (m.GetParameters().Length > 0)
                {
                    Console.Write(testName);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(" has parameters");
                    Console.ResetColor();
                    continue;
                }

                object instance = null;
                if (!m.IsStatic && !instanceCache.TryGetValue(m.DeclaringType, out instance))
                    instanceCache.Add(
                        m.DeclaringType,
                        instance = ActivatorUtilities.CreateInstance(serviceProvider, m.DeclaringType));

                stopwatch.Restart();

                try
                {
                    (m.Invoke(instance, null) as Task)?.Wait();

                    stopwatch.Stop();
                    Console.Write(testName);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write(" OK");
                    Console.ResetColor();
                    Console.WriteLine(" in {0}ms", stopwatch.ElapsedMilliseconds);
                }
                catch (Exception ex)
                {
                    stopwatch.Stop();
                    failedCount++;
                    Console.Write(testName);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write(" Failed");
                    Console.ResetColor();
                    Console.WriteLine(" in {0}ms", stopwatch.ElapsedMilliseconds);

                    if (ex is TargetInvocationException)
                        ex = ex.InnerException;
                    var aex = ex as AggregateException;
                    if (aex != null && aex.InnerExceptions.Count == 1)
                        ex = aex.InnerException;
                    Console.WriteLine(ex);
                }
            }

            if (failedCount > 0)
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("{0} tests failed.", failedCount);
                Console.ResetColor();
            }
        }

        private IServiceProvider BuildServiceProvider()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("../../src/ImgAzyobuziNet/appsettings.json")
                .Build();
            var services = new ServiceCollection()
                .Configure<ImgAzyobuziNetOptions>(configuration.GetSection("ImgAzyobuziNet"))
                .AddLogging()
                .AddCaching();
            return services
                .AddInstance(typeof(ITestActivator), new TestActivator(services.BuildServiceProvider()))
                .BuildServiceProvider();
        }
    }
}
