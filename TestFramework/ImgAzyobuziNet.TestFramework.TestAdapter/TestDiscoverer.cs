using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Logging;

namespace ImgAzyobuziNet.TestFramework.TestAdapter
{
    [FileExtension(".dll")]
    [DefaultExecutorUri(Constants.ExecutorUriString)]
    public class TestDiscoverer : ITestDiscoverer
    {
        public void DiscoverTests(IEnumerable<string> sources, IDiscoveryContext discoveryContext, IMessageLogger logger, ITestCaseDiscoverySink discoverySink)
        {
            foreach (var testCase in DiscoverTests(sources, logger))
                discoverySink.SendTestCase(testCase);
        }

        public static IEnumerable<TestCase> DiscoverTests(IEnumerable<string> sources, IMessageLogger logger)
        {
            foreach (var source in sources)
            {
                var assemblyName = Path.GetFileNameWithoutExtension(source);
                Assembly sourceAssembly;

                try
                {
                    sourceAssembly = Assembly.Load(new AssemblyName(assemblyName));
                }
                catch (Exception ex)
                {
                    logger.SendMessage(TestMessageLevel.Informational, $"Failed load assembly: {assemblyName}\n{ex}");
                    continue;
                }

                logger.SendMessage(TestMessageLevel.Informational, "Loaded assembly: " + sourceAssembly.FullName);

                var providerTypes = sourceAssembly.GetTypes()
                    .Where(x => x.GetInterfaces().Contains(typeof(ITestTargetProvider)));

                var targetAssemblies = new List<Assembly>();

                foreach (var providerType in providerTypes)
                {
                    targetAssemblies.Clear();

                    try
                    {
                        var provider = (ITestTargetProvider)Activator.CreateInstance(providerType);
                        targetAssemblies.AddRange(provider.GetTargetAssemblies());
                    }
                    catch (Exception ex)
                    {
                        logger.SendMessage(TestMessageLevel.Warning, $"{providerType.FullName} threw an exception: {ex}");
                        continue;
                    }

                    foreach (var targetAssembly in targetAssemblies)
                    {
                        if (targetAssembly != null)
                        {
                            foreach (var testCase in FindTestMethods(targetAssembly, source, sourceAssembly.FullName, providerType.FullName))
                                yield return testCase;
                        }
                    }
                }
            }
        }

        private static IEnumerable<TestCase> FindTestMethods(Assembly targetAssembly, string source, string sourceAssemblyName, string providerFullName)
        {
            // DefinedTypes includes nested types
            foreach (var method in targetAssembly.DefinedTypes.SelectMany(x => x.DeclaredMethods))
            {
                if (method.GetParameters().Length > 0) continue;
                if (method.ReturnType != typeof(void) && method.ReturnType != typeof(Task)) continue;

                var attr = method.GetCustomAttribute<TestMethodAttribute>();
                if (attr == null) continue;

                var testCase = new TestCase(
                    method.DeclaringType.FullName + "." + method.Name,
                    Constants.ExecutorUri,
                    source
                );

                testCase.DisplayName = method.DeclaringType.Name + "." + method.Name;
                testCase.CodeFilePath = attr.FilePath;
                testCase.LineNumber = attr.LineNumber;
                testCase.Traits.Add("TestCategory", attr.Category.ToString());
                testCase.SetPropertyValue(Constants.ProviderAssemblyNameProperty, sourceAssemblyName);
                testCase.SetPropertyValue(Constants.ProviderTypeFullNameProperty, providerFullName);
                testCase.SetPropertyValue(Constants.TestMethodAssemblyNameProperty, targetAssembly.FullName);
                testCase.SetPropertyValue(Constants.TestMethodDeclaringTypeProperty, method.DeclaringType.FullName);
                testCase.SetPropertyValue(Constants.TestMethodNameProperty, method.Name);

                yield return testCase;
            }
        }
    }
}
