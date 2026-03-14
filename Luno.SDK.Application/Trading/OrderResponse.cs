namespace Luno.SDK.Application.Trading;

/// <summary>
/// Represents the result of an order placement operation returned to the caller.
/// </summary>
public record OrderResponse
{
    /// <summary>
    /// Gets or sets the unique Luno-assigned Order ID.
    /// </summary>
    public required string OrderId { get; init; }
}
