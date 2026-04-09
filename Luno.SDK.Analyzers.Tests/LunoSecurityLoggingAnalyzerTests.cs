using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using System.Threading.Tasks;
using Xunit;
using Luno.SDK.Analyzers.Infrastructure;

namespace Luno.SDK.Analyzers.Tests
{
    public class LunoSecurityLoggingAnalyzerTests
    {
        private const string DummyInfrastructure = @"
namespace Microsoft.Extensions.Logging
{
    public interface ILogger { void Log(object state); }
    public interface ILogger<T> : ILogger { }
    public static class LoggerExtensions
    {
        public static void LogInformation(this ILogger logger, string message, params object[] args) { }
    }
}
namespace Microsoft.Kiota.Abstractions
{
    public class RequestInformation { }
}
namespace Luno.SDK
{
    public class LunoCredentials { }
    public interface ILunoCredentialProvider { }
}
";

        [Fact]
        public async Task Log_StandardString_ShouldPass()
        {
            var test = $@"using Microsoft.Extensions.Logging;
{DummyInfrastructure}

public class TestClass
{{
    public void Test(ILogger logger)
    {{
        logger.LogInformation(""Hello World"");
    }}
}}";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [Fact]
        public async Task Log_RequestInformation_ShouldFail()
        {
            var test = $@"using Microsoft.Extensions.Logging;
using Microsoft.Kiota.Abstractions;
{DummyInfrastructure}

public class TestClass
{{
    public void Test(ILogger logger, RequestInformation request)
    {{
        logger.LogInformation(""Request: {{0}}"", {{|#0:request|}});
    }}
}}";

            await VerifyCS.VerifyAnalyzerAsync(test, "RequestInformation");
        }

        [Fact]
        public async Task Log_DerivedCredentials_ShouldFail()
        {
            var test = $@"using Microsoft.Extensions.Logging;
using Luno.SDK;
{DummyInfrastructure}

public class MyCustomCredentials : LunoCredentials {{ }}

public class TestClass
{{
    public void Test(ILogger logger, MyCustomCredentials creds)
    {{
        logger.LogInformation(""Secret: {{0}}"", {{|#0:creds|}});
    }}
}}";

            await VerifyCS.VerifyAnalyzerAsync(test, "LunoCredentials");
        }

        [Fact]
        public async Task Log_ProviderImplementation_ShouldFail()
        {
            var test = $@"using Microsoft.Extensions.Logging;
using Luno.SDK;
{DummyInfrastructure}

public class MyProvider : ILunoCredentialProvider {{ }}

public class TestClass
{{
    public void Test(ILogger logger, MyProvider provider)
    {{
        logger.LogInformation(""Provider: {{0}}"", {{|#0:provider|}});
    }}
}}";

            await VerifyCS.VerifyAnalyzerAsync(test, "ILunoCredentialProvider");
        }

        [Fact]
        public async Task Log_GenericLogger_ShouldFail()
        {
            var test = $@"using Microsoft.Extensions.Logging;
using Luno.SDK;

{DummyInfrastructure}

public class TestClass
{{
    public void Test(ILogger<TestClass> logger, LunoCredentials creds)
    {{
        logger.Log({{|#0:creds|}});
    }}
}}";

            await VerifyCS.VerifyAnalyzerAsync(test, "LunoCredentials");
        }
    }

    public static class VerifyCS
    {
        public static Task VerifyAnalyzerAsync(string source, string? expectedArgument = null)
        {
            var test = new CSharpAnalyzerTest<LunoGovernanceAnalyzer, XUnitVerifier>
            {
                TestCode = source,
            };

            if (expectedArgument != null)
            {
                // Explicitly map the diagnostic expectation to the numbered markup index #0
                var expected = VerifyCS.Diagnostic().WithLocation(0).WithArguments(expectedArgument);
                test.ExpectedDiagnostics.Add(expected);
            }

            return test.RunAsync();
        }

        public static DiagnosticResult Diagnostic()
            => Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<LunoGovernanceAnalyzer>.Diagnostic();
    }
}
