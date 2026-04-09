using System.Threading.Tasks;
using Luno.SDK.Analyzers.Infrastructure;

namespace Luno.SDK.Analyzers.Tests
{
    /// <summary>
    /// Fluent C# 14 extensions to keep our security tests pristine and readable.
    /// </summary>
    public static class GovernanceTestExtensions
    {
        /// <summary>
        /// Verifies that the source code triggers a security violation for the specified type.
        /// </summary>
        public static async Task ShouldFailWith(this string source, string expectedType)
        {
            await VerifyCS.VerifyAnalyzerAsync(source, expectedType);
        }

        /// <summary>
        /// Verifies that the source code passes all governance checks.
        /// </summary>
        public static async Task ShouldPass(this string source)
        {
            await VerifyCS.VerifyAnalyzerAsync(source);
        }
    }
}
