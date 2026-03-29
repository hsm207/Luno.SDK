namespace Luno.SDK.Trading;

/// <summary>
/// Specifies the intention of an order: whether to buy or sell funds in the market.
/// Aligned with Luno V2 API ontology (BUY/SELL).
/// </summary>
public enum OrderSide
{
    /// <summary>
    /// A buy order.
    /// </summary>
    Buy,

    /// <summary>
    /// A sell order.
    /// </summary>
    Sell
}
