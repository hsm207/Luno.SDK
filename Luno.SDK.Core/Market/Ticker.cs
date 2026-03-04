namespace Luno.SDK.Core.Market;

/// <summary>
/// Represents a market ticker containing the latest trading information for a specific pair.
/// </summary>
/// <param name="Pair">The market pair identifier (e.g., XBTZAR).</param>
/// <param name="Ask">The current lowest sell price.</param>
/// <param name="Bid">The current highest buy price.</param>
/// <param name="LastTrade">The price of the most recent trade.</param>
/// <param name="Rolling24HourVolume">The total trading volume over the last 24 hours.</param>
/// <param name="Status">The current operational status of the market.</param>
/// <param name="Timestamp">The timestamp of the ticker data.</param>
public record Ticker(
    string Pair,
    decimal Ask,
    decimal Bid,
    decimal LastTrade,
    decimal Rolling24HourVolume,
    MarketStatus Status,
    DateTimeOffset Timestamp
)
{
    /// <summary>
    /// Gets the difference between the Ask and Bid prices.
    /// </summary>
    public decimal Spread => Ask - Bid;
    
    /// <summary>
    /// Gets a value indicating whether the market is currently active.
    /// </summary>
    public bool IsActive => Status is MarketStatus.Active;
}
