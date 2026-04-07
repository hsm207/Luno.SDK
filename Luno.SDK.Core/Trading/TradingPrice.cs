namespace Luno.SDK.Trading;

/// <summary>
/// Represents a strongly-typed intended trade price.
/// </summary>
public record TradingPrice(decimal Value)
{
    /// <summary>
    /// Creates a trading price expressed in the Quote currency per 1 Base.
    /// </summary>
    /// <param name="value">The exact nominal price.</param>
    public static TradingPrice InQuote(decimal value)
    {
        if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value), "Trading price must be strictly greater than 0.");
        return new(value);
    }
}
