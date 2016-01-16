using Microsoft.CodeAnalysis;
using Microsoft.Dnx.Compilation.CSharp;

namespace ImgAzyobuziNet.Core.Compiler.Preprocess
{
    public class SuppressWarningsCompileModule : ICompileModule
    {
        public void BeforeCompile(BeforeCompileContext context)
        {
            var options = context.Compilation.Options;
            var builder = options.SpecificDiagnosticOptions.ToBuilder();

            // CS0649 Field is never assigned to, and will always have its default value null
            builder["CS0649"] = ReportDiagnostic.Suppress;

            context.Compilation = context.Compilation
                .WithOptions(options.WithSpecificDiagnosticOptions(builder.ToImmutable()));
        }

        public void AfterCompile(AfterCompileContext context) { }
    }
}
