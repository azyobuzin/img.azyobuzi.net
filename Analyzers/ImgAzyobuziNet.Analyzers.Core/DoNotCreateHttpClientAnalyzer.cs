using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Semantics;

namespace ImgAzyobuziNet.Analyzers.Core
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DoNotCreateHttpClientAnalyzer : DiagnosticAnalyzer
    {
        internal static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor("IAN0001", "Don't Create an HttpClient Instance", "Don't create an HttpClient instance. Use IHttpClient.", "Usage", DiagnosticSeverity.Warning, true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();

            context.RegisterCompilationStartAction(compilationContext =>
            {
                var httpClientType = compilationContext.Compilation
                    .GetTypeByMetadataName("System.Net.Http.HttpClient");

                if (httpClientType == null) return;

                compilationContext.RegisterOperationAction(
                    operationContext =>
                    {
                        var operation = (IObjectCreationExpression)operationContext.Operation;
                        if (operation.Constructor.ContainingType == httpClientType)
                        {
                            operationContext.ReportDiagnostic(Diagnostic.Create(
                                Rule,
                                operation.Syntax.GetLocation()
                            ));
                        }
                    },
                    OperationKind.ObjectCreationExpression
                );
            });
        }
    }
}
