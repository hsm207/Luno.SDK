namespace Luno.SDK.Trading;

/// <summary>
/// Detailed information about an order on the Luno exchange.
/// </summary>
public record Order
{
    /// <summary>Gets the unique Luno-assigned Order ID.</summary>
    public required string OrderId { get; init; }

    /// <summary>Gets the client-defined deduplication ID, if any.</summary>
    public string? ClientOrderId { get; init; }

    /// <summary>Gets the creation timestamp in milliseconds.</summary>
    public required long CreationTimestamp { get; init; }

    /// <summary>Gets the expiration timestamp in milliseconds, if any.</summary>
    public long? ExpirationTimestamp { get; init; }

    /// <summary>Gets the current status of the order.</summary>
    public required OrderStatus Status { get; init; }

    /// <summary>Gets whether the order is in a terminal state (Complete).</summary>
    public bool IsClosed => Status == OrderStatus.Complete;

    /// <summary>Gets the limit price of the order.</summary>
    public required decimal LimitPrice { get; init; }

    /// <summary>Gets the limit volume of the order.</summary>
    public required decimal LimitVolume { get; init; }

    /// <summary>Gets the base amount filled (principal).</summary>
    public decimal? FilledBase { get; init; }

    /// <summary>Gets the counter amount filled (principal).</summary>
    public decimal? FilledCounter { get; init; }

    /// <summary>Gets the side (Bid/Ask) of the order.</summary>
    public required OrderType Side { get; init; }

    /// <summary>Gets the currency pair for the order.</summary>
    public required string Pair { get; init; }

    /// <summary>Gets the fee in base currency, if available.</summary>
    public decimal? FeeBase { get; init; }

    /// <summary>Gets the fee in counter currency, if available.</summary>
    public decimal? FeeCounter { get; init; }
}

