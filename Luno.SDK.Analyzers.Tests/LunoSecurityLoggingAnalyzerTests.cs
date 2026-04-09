using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using System.Threading.Tasks;
using Xunit;
using Luno.SDK.Analyzers.Infrastructure;

namespace Luno.SDK.Analyzers.Tests
{
    public class LunoSecurityLoggingAnalyzerTests : GovernanceTestBase
    {
        [Fact(DisplayName = "Given a standard string, When passed to ILogger.LogInformation, Then no diagnostics should be reported")]
        public async Task Log_StandardString_ShouldPass() => await $$"""
            using Microsoft.Extensions.Logging;
            {{DummyInfrastructure}}

            public class TestClass {
                public void Test(ILogger logger) => logger.LogInformation("Hello World");
            }
            """.ShouldPass();

        [Fact(DisplayName = "Given a RequestInformation object, When passed to ILogger.LogInformation, Then a security violation should be reported")]
        public async Task Log_RequestInformation_ShouldFail() => await $$"""
            using Microsoft.Extensions.Logging;
            using Microsoft.Kiota.Abstractions;
            {{DummyInfrastructure}}

            public class TestClass {
                public void Test(ILogger logger, RequestInformation request) {
                    logger.LogInformation("Request: {0}", {|#0:request|});
                }
            }
            """.ShouldFailWith("RequestInformation");

        [Fact(DisplayName = "Given a type derived from LunoCredentials, When passed to ILogger.LogInformation, Then a security violation should be reported")]
        public async Task Log_DerivedCredentials_ShouldFail() => await $$"""
            using Microsoft.Extensions.Logging;
            using Luno.SDK;
            {{DummyInfrastructure}}

            public class MyCustomCredentials : LunoCredentials { }

            public class TestClass {
                public void Test(ILogger logger, MyCustomCredentials creds) {
                    logger.LogInformation("Secret: {0}", {|#0:creds|});
                }
            }
            """.ShouldFailWith("LunoCredentials");

        [Fact(DisplayName = "Given a type implementing ILunoCredentialProvider, When passed to ILogger.LogInformation, Then a security violation should be reported")]
        public async Task Log_ProviderImplementation_ShouldFail() => await $$"""
            using Microsoft.Extensions.Logging;
            using Luno.SDK;
            {{DummyInfrastructure}}

            public class MyProvider : ILunoCredentialProvider { }

            public class TestClass {
                public void Test(ILogger logger, MyProvider provider) {
                    logger.LogInformation("Provider: {0}", {|#0:provider|});
                }
            }
            """.ShouldFailWith("ILunoCredentialProvider");

        [Fact(DisplayName = "Given a LunoCredentials object, When passed to ILogger<T>.Log, Then a security violation should be reported")]
        public async Task Log_GenericLogger_ShouldFail() => await $$"""
            using Microsoft.Extensions.Logging;
            using Luno.SDK;
            {{DummyInfrastructure}}

            public class TestClass {
                public void Test(ILogger<TestClass> logger, LunoCredentials creds) {
                    logger.Log({|#0:creds|});
                }
            }
            """.ShouldFailWith("LunoCredentials");
    }

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
