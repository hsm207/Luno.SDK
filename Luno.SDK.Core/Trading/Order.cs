namespace Luno.SDK.Trading;

/// <summary>
/// Abstract base record representing shared identity and state for all order types on the Luno exchange.
/// Leaf types: <see cref="LimitOrder"/>, <see cref="MarketOrder"/>, <see cref="StopLimitOrder"/>.
/// </summary>
/// <remarks>
/// All properties are get-only (strictly immutable) to prevent post-construction modification.
/// The "at least one AccountId" invariant is enforced in the protected constructor.
/// </remarks>
public abstract record Order
{
    /// <summary>Gets the unique Luno-assigned Order ID.</summary>
    public string OrderId { get; }

    /// <summary>Gets the client-defined deduplication ID, if any.</summary>
    public string? ClientOrderId { get; }

    /// <summary>Gets the behavioral type of the order (Limit, Market, StopLimit).</summary>
    public OrderType Type { get; }

    /// <summary>Gets the intention of the order (Buy or Sell).</summary>
    public OrderSide Side { get; }

    /// <summary>Gets the current status of the order.</summary>
    public OrderStatus Status { get; }

    /// <summary>Gets the currency pair for the order (e.g. XBTZAR).</summary>
    public string Pair { get; }

    /// <summary>Gets the creation timestamp in milliseconds.</summary>
    public long CreationTimestamp { get; }

    /// <summary>Gets the completion timestamp in milliseconds, or null if still active.</summary>
    public long? CompletedTimestamp { get; }

    /// <summary>Gets the expiration timestamp in milliseconds, if any.</summary>
    public long? ExpirationTimestamp { get; }

    /// <summary>Gets the base currency account ID.</summary>
    public long? BaseAccountId { get; }

    /// <summary>Gets the counter currency account ID.</summary>
    public long? CounterAccountId { get; }

    /// <summary>Gets the base amount filled (principal), if any.</summary>
    public decimal? FilledBase { get; }

    /// <summary>Gets the counter amount filled (principal), if any.</summary>
    public decimal? FilledCounter { get; }

    /// <summary>Gets the fee in base currency, if available.</summary>
    public decimal? FeeBase { get; }

    /// <summary>Gets the fee in counter currency, if available.</summary>
    public decimal? FeeCounter { get; }

    /// <summary>Gets whether the order is in a terminal state (Complete).</summary>
    public bool IsClosed => Status == OrderStatus.Complete;

    /// <summary>
    /// Initializes a new instance of the <see cref="Order"/> record.
    /// </summary>
    /// <exception cref="LunoValidationException">
    /// Thrown when both <paramref name="baseAccountId"/> and <paramref name="counterAccountId"/> are null.
    /// </exception>
    protected Order(
        string orderId,
        OrderType type,
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
    {
        if (baseAccountId == null && counterAccountId == null)
            throw new LunoValidationException(
                "Domain Invariant Violation: An order must specify at least a BaseAccountId or a CounterAccountId.");

        OrderId = orderId;
        Type = type;
        Side = side;
        Status = status;
        Pair = pair;
        CreationTimestamp = creationTimestamp;
        BaseAccountId = baseAccountId;
        CounterAccountId = counterAccountId;
        ClientOrderId = clientOrderId;
        CompletedTimestamp = completedTimestamp;
        ExpirationTimestamp = expirationTimestamp;
        FilledBase = filledBase;
        FilledCounter = filledCounter;
        FeeBase = feeBase;
        FeeCounter = feeCounter;
    }
}
