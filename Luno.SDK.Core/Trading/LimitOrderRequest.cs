namespace Luno.SDK.Trading;

/// <summary>
/// A behavior-free boundary DTO for crossing the Application → Infrastructure boundary.
/// Constructed by the Application layer after validating a <see cref="LimitOrderParameters"/> value object.
/// Infrastructure accepts this record and maps it directly on to the Kiota-generated API client — nothing more.
/// </summary>
public record LimitOrderRequest
{
    /// <summary>The currency pair to trade (e.g. XBTZAR).</summary>
    public required string Pair { get; init; }

    /// <summary>The order side (Buy or Sell).</summary>
    public required OrderSide Side { get; init; }

    /// <summary>Amount of cryptocurrency to buy or sell.</summary>
    public required decimal Volume { get; init; }

    /// <summary>Limit price as a decimal.</summary>
    public required decimal Price { get; init; }

    /// <summary>The base currency account to use in the trade.</summary>
    public required long? BaseAccountId { get; init; }

    /// <summary>The counter currency account to use in the trade.</summary>
    public required long? CounterAccountId { get; init; }

    /// <summary>Client-defined order ID for idempotency, if provided.</summary>
    public string? ClientOrderId { get; init; }

    /// <summary>Time in force behaviour. Defaults to GTC.</summary>
    public TimeInForce TimeInForce { get; init; } = TimeInForce.GTC;

    /// <summary>Whether this is a post-only order.</summary>
    public bool PostOnly { get; init; }

    /// <summary>Trigger price for stop-limit orders, if applicable.</summary>
    public decimal? StopPrice { get; init; }

    /// <summary>Side of the trigger price to activate the order, if applicable.</summary>
    public StopDirection? StopDirection { get; init; }

    /// <summary>Unix timestamp in milliseconds of when the request was created and sent.</summary>
    public long? Timestamp { get; init; }

    /// <summary>Milliseconds after <see cref="Timestamp"/> for which the request is valid.</summary>
    public long? TTL { get; init; }
}
