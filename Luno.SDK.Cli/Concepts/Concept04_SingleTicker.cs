using Luno.SDK;

namespace Luno.SDK.Cli.Concepts;

/// <summary>
/// Provides a demonstration of fetching real-time market ticker for a single pair using the Luno SDK.
/// </summary>
public static class Concept04_SingleTicker
{
    /// <summary>
    /// Runs the Single Ticker demonstration.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task RunAsync()
    {
        Console.WriteLine("--- Demonstration: Single Ticker ---");
        Console.WriteLine("📡 Fetching latest ticker for XBTZAR from Luno...");

        // 1. Initialize the standalone client
        var luno = new LunoClient();

        // 2. Use the fluent extension to fetch the single ticker
        // This method automatically maps the raw domain entities to application-layer DTOs.
        var ticker = await luno.GetTickerAsync("XBTZAR");

        var statusStr = ticker.IsActive ? "ACTIVE" : "DISABLED";
        Console.WriteLine($"[{ticker.Timestamp:HH:mm:ss.fff}] [{statusStr,-8}] {ticker.Pair,-10} | Price: {ticker.Price,12:N2}");

        Console.WriteLine("--- Demonstration Complete ---");
    }
}
