using Luno.SDK;
using Luno.SDK.Application.Market;
using Luno.SDK.Application.Trading;
using Luno.SDK.Trading;

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
        Console.WriteLine("\n=== Concept 01: Market Data ===");
        
        // 1. Initialize the standalone client
        var luno = new LunoClient();

        // 2. Unfiltered Demo
        Console.WriteLine("\n📡 Fetching ALL latest tickers from Luno...");
        await foreach (var ticker in luno.Market.GetTickersAsync().Take(5))
        {
            PrintTicker(ticker);
        }

        // 3. Filtered Demo
        var pairs = new[] { "XBTMYR", "ETHMYR" };
        Console.WriteLine($"\n🎯 Fetching FILTERED tickers ({string.Join(", ", pairs)}) from Luno...");
        await foreach (var ticker in luno.Market.GetTickersAsync(pairs))
        {
            PrintTicker(ticker);
        }

        Console.WriteLine("\n--- Demonstration Complete ---");
    }

    private static void PrintTicker(TickerResponse ticker)
    {
        var statusStr = ticker.IsActive ? "ACTIVE" : "DISABLED";
        Console.WriteLine($"[{ticker.Timestamp:HH:mm:ss.fff}] [{statusStr,-8}] {ticker.Pair,-10} | Price: {ticker.Price,12:N2}");
    }
}
