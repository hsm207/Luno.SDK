namespace Luno.SDK.Trading;

/// <summary>
/// A limit order with a specified price and volume.
/// </summary>
public record LimitOrder : Order
{
    /// <summary>Gets the limit price of the order.</summary>
    public decimal LimitPrice { get; }

    /// <summary>Gets the limit volume of the order.</summary>
    public decimal LimitVolume { get; }

    /// <summary>Gets the time-in-force behavior for this order.</summary>
    public TimeInForce TimeInForce { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LimitOrder"/> record.
    /// </summary>
    public LimitOrder(
        string orderId,
        OrderSide side,
        OrderStatus status,
        string pair,
        long creationTimestamp,
        long? baseAccountId,
        long? counterAccountId,
        decimal limitPrice,
        decimal limitVolume,
        TimeInForce? timeInForce = null,
        string? clientOrderId = null,
        long? completedTimestamp = null,
        long? expirationTimestamp = null,
        decimal? filledBase = null,
        decimal? filledCounter = null,
        decimal? feeBase = null,
        decimal? feeCounter = null)
        : base(orderId, OrderType.Limit, side, status, pair, creationTimestamp,
               baseAccountId, counterAccountId, clientOrderId,
               completedTimestamp, expirationTimestamp,
               filledBase, filledCounter, feeBase, feeCounter)
    {
        LimitPrice = limitPrice;
        LimitVolume = limitVolume;
        TimeInForce = timeInForce ?? TimeInForce.GTC;
    }
}
