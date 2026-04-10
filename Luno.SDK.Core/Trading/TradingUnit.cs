namespace Luno.SDK.Trading;

/// <summary>
/// Specifies the currency unit for a trading amount.
/// </summary>
public enum TradingUnit
{
    /// <summary>
    /// The base currency in the pair (e.g., BTC in BTC/MYR).
    /// </summary>
    Base,

    /// <summary>
    /// The quote or counter currency in the pair (e.g., MYR in BTC/MYR).
    /// </summary>
    Quote
}
