namespace Luno.SDK.Trading;

/// <summary>
/// Represents a strongly-typed intended trade price.
/// </summary>
public readonly record struct TradingPrice(decimal Value)
{
    /// <summary>
    /// Creates a trading price expressed in the Quote currency per 1 Base.
    /// </summary>
    /// <param name="value">The exact nominal price.</param>
    public static TradingPrice InQuote(decimal value) => new(value);
}
