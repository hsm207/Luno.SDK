using System;

namespace Luno.SDK.Trading;

/// <summary>
/// Detailed information about an order on the Luno exchange.
/// </summary>
public record Order
{
    /// <summary>
    /// Gets the unique Luno-assigned Order ID.
    /// </summary>
    public required string OrderId { get; init; }

    /// <summary>
    /// Gets the client-defined deduplication ID, if any.
    /// </summary>
    public string? ClientOrderId { get; init; }

    /// <summary>
    /// Gets the current status of the order.
    /// </summary>
    public OrderStatus Status { get; init; }

    /// <summary>
    /// Gets whether the order is in a terminal state (Complete).
    /// </summary>
    public bool IsClosed => Status == OrderStatus.Complete;
}
