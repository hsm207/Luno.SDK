namespace Luno.SDK.Market;

/// <summary>
/// Represents the dynamic metadata rules and constraints of a given Luno market.
/// Follows a Strict Zero-Null Policy.
/// </summary>
public record MarketInfo
{
    /// <summary>
    /// The market identifier (e.g., "XBTMYR").
    /// </summary>
    public required string Pair { get; init; }

    /// <summary>
    /// The current operational status of the market.
    /// </summary>
    public required MarketStatus Status { get; init; }

    /// <summary>
    /// The base currency code (e.g., "XBT").
    /// </summary>
    public required string BaseCurrency { get; init; }

    /// <summary>
    /// The counter currency code (e.g., "MYR").
    /// </summary>
    public required string CounterCurrency { get; init; }

    /// <summary>
    /// The absolute minimum order volume permitted for this market.
    /// </summary>
    public required decimal MinVolume { get; init; }

    /// <summary>
    /// The absolute maximum order volume permitted for this market.
    /// </summary>
    public required decimal MaxVolume { get; init; }

    /// <summary>
    /// The maximum number of decimal places permitted for volume amounts.
    /// </summary>
    public required int VolumeScale { get; init; }

    /// <summary>
    /// The absolute minimum order price permitted for this market.
    /// </summary>
    public required decimal MinPrice { get; init; }

    /// <summary>
    /// The absolute maximum order price permitted for this market.
    /// </summary>
    public required decimal MaxPrice { get; init; }

    /// <summary>
    /// The maximum number of decimal places permitted for prices.
    /// </summary>
    public required int PriceScale { get; init; }

    /// <summary>
    /// The maximum number of decimal places permitted for fees.
    /// </summary>
    public required int FeeScale { get; init; }

    /// <summary>
    /// Identifies whether the market is currently active and capable of receiving trades.
    /// </summary>
    public bool IsTradable() => Status == MarketStatus.Active || Status == MarketStatus.PostOnly;
}
