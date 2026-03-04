using Microsoft.Extensions.DependencyInjection;
using Luno.SDK;

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

        // 2. Add the Luno Client with default options
        services.AddLunoClient();

        // 3. Build the provider
        var serviceProvider = services.BuildServiceProvider();

        // 4. Resolve the ILunoClient instance
        var luno = serviceProvider.GetRequiredService<ILunoClient>();

        Console.WriteLine("📡 Fetching heartbeats via DI-resolved client...");

        // 5. Use the fluent extension to stream tickers
        int count = 0;
        await foreach (var ticker in luno.ListTickersAsync())
        {
            var statusStr = ticker.IsActive ? "ACTIVE" : "DISABLED";
            Console.WriteLine($"[{ticker.Timestamp:HH:mm:ss.fff}] [{statusStr,-8}] {ticker.Pair,-10} | Price: {ticker.Price,12:N2}");
            
            if (++count >= 5) break;
        }

        Console.WriteLine("--- Demonstration Complete ---");
    }
}
