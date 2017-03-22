using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

namespace ImgAzyobuziNet.TestFramework.TestAdapter
{
    [ExtensionUri(Constants.ExecutorUriString)]
    public class TestExecutor : ITestExecutor
    {
        private static readonly string[] s_knownTraits = new[] { "TestCategory" };
        private static readonly TestProperty[] s_knownProperties = new[]
        {
            TestCaseProperties.DisplayName,
            TestCaseProperties.FullyQualifiedName
        };
        private static readonly string[] s_supportedProperties = s_knownTraits.Concat(s_knownProperties.Select(x => x.Label)).ToArray();

        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        public void Cancel()
        {
            this._cts.Cancel();
        }

        public void RunTests(IEnumerable<TestCase> tests, IRunContext runContext, IFrameworkHandle frameworkHandle)
        {
            // TODO: ILogger support
            Task runTest(TestCase testCase)
            {
                var result = new TestResult(testCase);

                void SetException(Exception ex)
                {
                    result.ErrorMessage = ex.ToString();
                    result.ErrorStackTrace = ex.StackTrace;
                }

                var providerAssemblyName = testCase.GetPropertyValue(Constants.ProviderAssemblyNameProperty, (string)null);
                var providerTypeFullName = testCase.GetPropertyValue(Constants.ProviderTypeFullNameProperty, (string)null);
                var testMethodAssemblyName = testCase.GetPropertyValue(Constants.TestMethodAssemblyNameProperty, (string)null);
                var testMethodDeclaringType = testCase.GetPropertyValue(Constants.TestMethodDeclaringTypeProperty, (string)null);
                var testMethodName = testCase.GetPropertyValue(Constants.TestMethodNameProperty, (string)null);

                Type providerType;
                Type testMethodType;
                MethodInfo testMethod;

                try
                {
                    providerType = Assembly.Load(new AssemblyName(providerAssemblyName))
                        .GetType(providerTypeFullName, true);

                    testMethodType = Assembly.Load(new AssemblyName(testMethodAssemblyName))
                        .GetType(testMethodDeclaringType, true);

                    testMethod = testMethodType.GetTypeInfo().DeclaredMethods
                        .First(x => x.Name == testMethodName
                            && x.IsDefined(typeof(TestMethodAttribute))
                            && x.GetParameters().Length == 0
                            && (x.ReturnType == typeof(void) || x.ReturnType == typeof(Task))
                        );
                }
                catch (Exception ex)
                {
                    SetException(ex);
                    result.Outcome = TestOutcome.NotFound;
                    frameworkHandle.RecordResult(result);
                    return Task.CompletedTask;
                }

                frameworkHandle.RecordStart(testCase);
                result.StartTime = DateTimeOffset.Now;
                var stopwatch = Stopwatch.StartNew();

                try
                {
                    object instance = null;

                    if (!testMethod.IsStatic)
                    {
                        instance = ((ITestTargetProvider)Activator.CreateInstance(providerType))
                            .GetActivator().CreateInstance(testMethodType);
                    }

                    var returnValue = testMethod.Invoke(instance, null);

                    if (returnValue is Task returnTask)
                    {
                        return returnTask.ContinueWith(t =>
                        {
                            stopwatch.Stop();
                            result.EndTime = DateTimeOffset.Now;
                            result.Duration = stopwatch.Elapsed;

                            if (t.IsFaulted)
                            {
                                SetException(t.Exception.InnerException);
                                result.Outcome = TestOutcome.Failed;
                            }
                            else if (t.IsCanceled)
                            {
                                result.ErrorMessage = "The task was canceled";
                                result.Outcome = TestOutcome.Failed;
                            }
                            else
                            {
                                result.Outcome = TestOutcome.Passed;
                            }

                            frameworkHandle.RecordEnd(testCase, result.Outcome);
                            frameworkHandle.RecordResult(result);
                        }, TaskContinuationOptions.ExecuteSynchronously);
                    }

                    result.Outcome = TestOutcome.Passed;
                }
                catch (Exception ex)
                {
                    if (ex is TargetInvocationException)
                        ex = ex.InnerException;
                    SetException(ex);
                    result.Outcome = TestOutcome.Failed;
                }

                stopwatch.Stop();
                result.EndTime = DateTimeOffset.Now;
                result.Duration = stopwatch.Elapsed;

                frameworkHandle.RecordEnd(testCase, result.Outcome);
                frameworkHandle.RecordResult(result);

                return Task.CompletedTask;
            }

            var filterExpr = runContext.GetTestCaseFilter(s_supportedProperties, null);

            Task.WaitAll(
                tests.Where(x => filterExpr == null || filterExpr.MatchTestCase(x, p => PropertyValueProvider(x, p)))
                    .Select(x => Task.Run(() => runTest(x), this._cts.Token))
                    .ToArray(),
                this._cts.Token
            );
        }

        public void RunTests(IEnumerable<string> sources, IRunContext runContext, IFrameworkHandle frameworkHandle)
        {
            this.RunTests(TestDiscoverer.DiscoverTests(sources, frameworkHandle), runContext, frameworkHandle);
        }

        private static object PropertyValueProvider(TestCase testCase, string propertyName)
        {
            foreach (var trait in testCase.Traits)
            {
                if (string.Equals(trait.Name, propertyName, StringComparison.CurrentCultureIgnoreCase))
                    return trait.Value;
            }

            foreach (var prop in s_knownProperties)
            {
                if (string.Equals(prop.Label, propertyName, StringComparison.CurrentCultureIgnoreCase))
                    return testCase.GetPropertyValue(prop);
            }

            return null;
        }
    }
}
