namespace Luno.SDK.Trading;

/// <summary>
/// Specifies the intention of an order: whether to buy or sell funds in the market.
/// Aligned with Luno V2 API ontology (BUY/SELL).
/// </summary>
public abstract record OrderSide
{
    private OrderSide() { }

    /// <summary>Represents a buy order intention.</summary>
    public sealed record BuySide : OrderSide;

    /// <summary>Represents a sell order intention.</summary>
    public sealed record SellSide : OrderSide;

    /// <summary>A singleton instance representing a buy order.</summary>
    public static readonly OrderSide Buy = new BuySide();

    /// <summary>A singleton instance representing a sell order.</summary>
    public static readonly OrderSide Sell = new SellSide();
}
