using Luno.SDK.Trading;

namespace Luno.SDK.Application.Trading;

/// <summary>
/// Represents comprehensive details of an order returned in a list.
/// </summary>
public record OrderDetailsResponse
{
    /// <summary>Gets or sets the unique Luno-assigned Order ID.</summary>
    public required string OrderId { get; init; }

    /// <summary>Gets or sets the creation timestamp.</summary>
    public DateTimeOffset CreationTimestamp { get; init; }

    /// <summary>Gets or sets the expiration timestamp (if any).</summary>
    public DateTimeOffset? ExpirationTimestamp { get; init; }

    /// <summary>Gets or sets the current state of the order.</summary>
    public OrderStatus State { get; init; }

    /// <summary>Gets or sets the currency pair.</summary>
    public required string Pair { get; init; }

    /// <summary>Gets or sets the order type (Bid or Ask).</summary>
    public OrderType Type { get; init; }

    /// <summary>Gets or sets the original volume.</summary>
    public decimal LimitVolume { get; init; }

    /// <summary>Gets or sets the limit price.</summary>
    public decimal LimitPrice { get; init; }

    /// <summary>Gets or sets the filled volume in base currency.</summary>
    public decimal FilledBase { get; init; }

    /// <summary>Gets or sets the filled amount in counter currency.</summary>
    public decimal FilledCounter { get; init; }

    /// <summary>Gets or sets the fee in base currency.</summary>
    public decimal FeeBase { get; init; }

    /// <summary>Gets or sets the fee in counter currency.</summary>
    public decimal FeeCounter { get; init; }
}
