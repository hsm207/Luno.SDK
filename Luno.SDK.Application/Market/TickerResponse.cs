namespace Luno.SDK.Application.Market;

/// <summary>
/// Represents the application-layer response containing ticker data for a specific pair.
/// </summary>
/// <param name="Pair">The market pair (e.g., XBTZAR).</param>
/// <param name="Price">Last trade price.</param>
/// <param name="Spread">The current bid/ask spread.</param>
/// <param name="IsActive">Indicates if the market is currently active.</param>
/// <param name="Timestamp">Unix timestamp in milliseconds of the tick.</param>
/// <param name="Ask">The current lowest sell price.</param>
/// <param name="Bid">The current highest buy price.</param>
public record TickerResponse(
    string Pair,
    decimal Price,
    decimal Ask,
    decimal Bid,
    decimal Spread,
    bool IsActive,
    DateTimeOffset Timestamp
);
