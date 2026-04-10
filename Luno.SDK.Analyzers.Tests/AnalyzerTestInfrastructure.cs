using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using Luno.SDK.Analyzers.Infrastructure;
using Luno.SDK.Analyzers.Rules;

namespace Luno.SDK.Analyzers.Tests
{
    /// <summary>
    /// Infrastructure engine for orchestrating Microsoft.CodeAnalysis.Testing.
    /// </summary>
    public static class VerifyCS
    {
        public static Task VerifyAnalyzerAsync(string source, string? expectedArgument = null)
        {
            var test = new CSharpAnalyzerTest<LunoGovernanceAnalyzer, XUnitVerifier> { TestCode = source };

            if (expectedArgument != null)
            {
                test.ExpectedDiagnostics.Add(Diagnostic().WithLocation(0).WithArguments(expectedArgument));
            }

            return test.RunAsync();
        }

        public static DiagnosticResult Diagnostic()
            => Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<LunoGovernanceAnalyzer>.Diagnostic();
    }
}
