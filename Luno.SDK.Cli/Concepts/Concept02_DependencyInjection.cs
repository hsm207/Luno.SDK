using Microsoft.Extensions.DependencyInjection;
using Luno.SDK;
using Luno.SDK.Infrastructure.Market;

namespace Luno.SDK.Cli.Concepts;

/// <summary>
/// Provides a demonstration of integrating the Luno SDK into a Dependency Injection container.
/// </summary>
public static class Concept02_DependencyInjection
{
    /// <summary>
    /// Runs the Dependency Injection demonstration.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task RunAsync()
    {
        Console.WriteLine("--- Demonstration: Dependency Injection ---");

        // 1. Setup the Service Collection (Composition Root)
        var services = new ServiceCollection();

        // 2. Add the Luno Client with custom options
        services.AddLunoClient(options =>
        {
            options.ApiVersion = "1";
        });

        // 3. Build the provider
        var serviceProvider = services.BuildServiceProvider();

        // 4. Resolve the ILunoClient instance
        var luno = serviceProvider.GetRequiredService<ILunoClient>();

        Console.WriteLine("📡 Fetching heartbeats via DI-resolved client...");

        // 5. Use the fluent extension to stream tickers
        int count = 0;
        await foreach (var heartbeat in luno.GetMarketHeartbeatAsync())
        {
            var statusStr = heartbeat.IsActive ? "ACTIVE" : "DISABLED";
            Console.WriteLine($"[{heartbeat.Timestamp:HH:mm:ss.fff}] [{statusStr,-8}] {heartbeat.Pair,-10} | Price: {heartbeat.Price,12:N2}");
            
            if (++count >= 5) break;
        }

        Console.WriteLine("--- Demonstration Complete ---");
    }
}
