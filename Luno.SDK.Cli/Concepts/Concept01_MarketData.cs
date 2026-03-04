using Luno.SDK;

namespace Luno.SDK.Cli.Concepts;

/// <summary>
/// Provides a demonstration of fetching real-time market tickers using the Luno SDK.
/// </summary>
public static class Concept01_MarketData
{
    /// <summary>
    /// Runs the Market Data demonstration.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task RunAsync()
    {
        Console.WriteLine("--- Demonstration: Market Data ---");
        Console.WriteLine("📡 Fetching latest tickers from Luno...");

        // 1. Initialize the standalone client
        var luno = new LunoClient();

        // 2. Use the fluent extension to stream tickers
        // This method automatically maps the raw domain entities to application-layer DTOs.
        await foreach (var ticker in luno.GetTickersAsync())
        {
            var statusStr = ticker.IsActive ? "ACTIVE" : "DISABLED";
            Console.WriteLine($"[{ticker.Timestamp:HH:mm:ss.fff}] [{statusStr,-8}] {ticker.Pair,-10} | Price: {ticker.Price,12:N2}");
        }
        
        Console.WriteLine("--- Demonstration Complete ---");
    }
}
