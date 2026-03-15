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

    /// <summary>Gets the current status of the order.</summary>
    public OrderStatus Status { get; init; }

    /// <summary>Gets whether the order is in a terminal state (Complete).</summary>
    public bool IsClosed => Status == OrderStatus.Complete;

    /// <summary>Gets the limit price of the order, if available.</summary>
    public decimal? LimitPrice { get; init; }

    /// <summary>Gets the limit volume of the order, if available.</summary>
    public decimal? LimitVolume { get; init; }

    /// <summary>Gets the side (Bid/Ask) of the order, if available.</summary>
    public OrderType? Side { get; init; }
}

