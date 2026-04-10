namespace Luno.SDK.Trading;

/// <summary>
/// A market order executed at the best available price.
/// No behavior-specific required fields for retrieval.
/// </summary>
public record MarketOrder : Order
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MarketOrder"/> record.
    /// </summary>
    public MarketOrder(
        string orderId,
        OrderSide side,
        OrderStatus status,
        string pair,
        long creationTimestamp,
        long? baseAccountId,
        long? counterAccountId,
        string? clientOrderId = null,
        long? completedTimestamp = null,
        long? expirationTimestamp = null,
        decimal? filledBase = null,
        decimal? filledCounter = null,
        decimal? feeBase = null,
        decimal? feeCounter = null)
        : base(orderId, OrderType.Market, side, status, pair, creationTimestamp,
               baseAccountId, counterAccountId, clientOrderId,
               completedTimestamp, expirationTimestamp,
               filledBase, filledCounter, feeBase, feeCounter)
    {
    }
}
