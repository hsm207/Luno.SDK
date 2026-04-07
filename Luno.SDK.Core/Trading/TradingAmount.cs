namespace Luno.SDK.Trading;

/// <summary>
/// Represents a strongly-typed intended trade amount to prevent unit ambiguity (Base vs. Quote).
/// </summary>
public record TradingAmount(decimal Value, TradingUnit Unit)
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

    /// <summary>
    /// Resolves the equivalent Base volume for the current trading amount given a limit price.
    /// </summary>
    /// <param name="limitPrice">The execution price to convert against.</param>
    public decimal ResolveBaseVolume(decimal limitPrice)
    {
        if (limitPrice <= 0) throw new ArgumentOutOfRangeException(nameof(limitPrice), "Limit price must be strictly greater than 0.");
        return Unit == TradingUnit.Quote ? Value / limitPrice : Value;
    }
}
