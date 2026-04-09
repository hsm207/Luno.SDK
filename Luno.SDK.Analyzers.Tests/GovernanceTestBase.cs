namespace Luno.SDK.Analyzers.Tests
{
    /// <summary>
    /// Shared test infrastructure providing the mock Kiota and Logging types.
    /// </summary>
    public abstract class GovernanceTestBase
    {
        protected const string DummyInfrastructure = """
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
            """;
    }
}
