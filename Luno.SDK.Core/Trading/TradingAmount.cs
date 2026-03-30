namespace Luno.SDK.Trading;

/// <summary>
/// Represents a strongly-typed intended trade amount to prevent unit ambiguity (Base vs. Quote).
/// </summary>
public readonly record struct TradingAmount(decimal Value, TradingUnit Unit)
{
    /// <summary>
    /// Creates a trading amount expressed in the Base currency.
    /// </summary>
    /// <param name="value">The decimal value to spend/trade in Base.</param>
    public static TradingAmount InBase(decimal value) 
    {
        if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value), "Trading amount must be strictly greater than 0.");
        return new(value, TradingUnit.Base);
    }

    /// <summary>
    /// Creates a trading amount expressed in the Quote (Counter) currency.
    /// </summary>
    /// <param name="value">The decimal value to spend/trade in Quote.</param>
    public static TradingAmount InQuote(decimal value)
    {
        if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value), "Trading amount must be strictly greater than 0.");
        return new(value, TradingUnit.Quote);
    }
}
