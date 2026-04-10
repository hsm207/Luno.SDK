using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

using Luno.SDK.Tests.Integration.Infrastructure;

namespace Luno.SDK.Tests.Integration.Security;

/// <summary>
/// This test is designed to verify if sensitive headers are being leaked into the logs.
/// It uses a high-verbosity Trace logger to capture System.Net.Http internal logs.
/// </summary>
public class LoggingHardeningTests(ITestOutputHelper output)
{
    [Fact(DisplayName = "Given a high-verbosity Trace log level, When executing a private request, Then sensitive headers must be redacted")]
    public async Task GivenHighVerbosity_WhenLoggingHeaders_ThenRedacted()
    {
        // Setup
        var logSink = new StringBuilder();
        var services = new ServiceCollection();

        // 1. Configure logging to capture System.Net.Http at Trace level
        services.AddLogging(builder =>
        {
            builder.AddProvider(new MemoryLoggerProvider(logSink));
            builder.AddFilter("System.Net.Http", LogLevel.Trace);
            builder.SetMinimumLevel(LogLevel.Trace);
        });

        // 2. Configure Luno Client with dummy credentials
        const string testKey = "so_exposed_it_hurts";
        const string testSecret = "my_dirty_little_secret";

        services.AddLunoClient(opt =>
        {
            opt.WithCredentials(testKey, testSecret);
        });

        var sp = services.BuildServiceProvider();
        var client = sp.GetRequiredService<ILunoClient>();

        // Act
        try
        {
            await client.Accounts.GetBalancesAsync(new Luno.SDK.Application.Account.GetBalancesQuery());
        }
        catch
        {
            // We don't care about network failure, only the logs generated during construction
        }

        var logOutput = logSink.ToString();
        output.WriteLine("--- CAPTURED HTTP LOGS ---");
        output.WriteLine(logOutput);

        // Assert
        var expectedSecretPayload = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{testKey}:{testSecret}"));

        bool isSecretExposed = logOutput.Contains(expectedSecretPayload);

        if (isSecretExposed)
        {
            output.WriteLine("❌ SECURITY FAIL: The Authorization header is fully exposed in the logs!");
        }
        else
        {
            output.WriteLine("✅ SECURITY PASS: The Authorization header is masked or absent. Defense in depth successful!");
        }

        Assert.DoesNotContain(expectedSecretPayload, logOutput);
    }
}
