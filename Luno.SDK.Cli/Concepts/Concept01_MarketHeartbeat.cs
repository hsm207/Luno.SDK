using Luno.SDK;

namespace Luno.SDK.Cli.Concepts;

/// <summary>
/// Provides a demonstration of fetching real-time market tickers using the Luno SDK.
/// </summary>
public static class Concept01_MarketHeartbeat
{
    /// <summary>
    /// Runs the Market Heartbeat demonstration.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task RunAsync()
    {
        Console.WriteLine("--- Demonstration: Market Heartbeat ---");
        
        // 1. Initialize the client using the standalone defaults
        var luno = new LunoClient();

        // 2. Use the fluent extension to stream market tickers
        await foreach (var heartbeat in luno.GetMarketHeartbeatAsync())
        {
            var statusStr = heartbeat.IsActive ? "ACTIVE" : "DISABLED";
            Console.WriteLine($"[{heartbeat.Timestamp:HH:mm:ss.fff}] [{statusStr,-8}] {heartbeat.Pair,-10} | Price: {heartbeat.Price,12:N2}");
        }
        
        Console.WriteLine("--- Demonstration Complete ---");
    }
}
