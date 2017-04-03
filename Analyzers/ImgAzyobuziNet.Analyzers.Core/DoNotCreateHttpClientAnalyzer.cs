using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

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

                compilationContext.RegisterSyntaxNodeAction(
                    nodeContext =>
                    {
                        var node = (ObjectCreationExpressionSyntax)nodeContext.Node;
                        if (nodeContext.SemanticModel.GetSymbolInfo(node.Type).Symbol == httpClientType)
                        {
                            nodeContext.ReportDiagnostic(Diagnostic.Create(
                                Rule,
                                node.GetLocation()
                            ));
                        }
                    },
                    SyntaxKind.ObjectCreationExpression
                );
            });
        }
    }
}
