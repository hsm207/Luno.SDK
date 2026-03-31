namespace Luno.SDK.Trading;

/// <summary>
/// A stop-limit order that triggers when a stop price condition is met,
/// then enters the order book as a limit order.
/// </summary>
public record StopLimitOrder : Order
{
    /// <summary>Gets the trigger price for the stop condition.</summary>
    public decimal StopPrice { get; }

    /// <summary>Gets the direction of the trigger (Above or Below).</summary>
    public StopDirection StopDirection { get; }

    /// <summary>Gets the limit price of the order once triggered.</summary>
    public decimal LimitPrice { get; }

    /// <summary>Gets the limit volume of the order once triggered.</summary>
    public decimal LimitVolume { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="StopLimitOrder"/> record.
    /// </summary>
    public StopLimitOrder(
        string orderId,
        OrderSide side,
        OrderStatus status,
        string pair,
        long creationTimestamp,
        long? baseAccountId,
        long? counterAccountId,
        decimal stopPrice,
        StopDirection stopDirection,
        decimal limitPrice,
        decimal limitVolume,
        string? clientOrderId = null,
        long? completedTimestamp = null,
        long? expirationTimestamp = null,
        decimal? filledBase = null,
        decimal? filledCounter = null,
        decimal? feeBase = null,
        decimal? feeCounter = null)
        : base(orderId, OrderType.StopLimit, side, status, pair, creationTimestamp,
               baseAccountId, counterAccountId, clientOrderId,
               completedTimestamp, expirationTimestamp,
               filledBase, filledCounter, feeBase, feeCounter)
    {
        StopPrice = stopPrice;
        StopDirection = stopDirection;
        LimitPrice = limitPrice;
        LimitVolume = limitVolume;
    }
}
