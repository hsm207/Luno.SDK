namespace Luno.SDK.Trading;

/// <summary>
/// A reference to an order resulting from a core domain operation.
/// </summary>
public record OrderReference
{
    /// <summary>
    /// Gets or sets the unique Luno-assigned Order ID.
    /// </summary>
    public required string OrderId { get; init; }
}
