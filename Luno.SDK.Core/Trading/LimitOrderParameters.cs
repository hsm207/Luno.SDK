using System;

namespace Luno.SDK.Trading;

/// <summary>
/// Domain parameters required to construct and validate a new Limit Order before dispatching to the exchange.
/// </summary>
public record LimitOrderParameters
{
    /// <summary>
    /// Gets or sets the currency pair (e.g., XBTZAR).
    /// </summary>
    public required string Pair { get; init; }

    /// <summary>
    /// Gets or sets the order type (Bid or Ask).
    /// </summary>
    public required OrderType Type { get; init; }

    /// <summary>
    /// Gets or sets the volume to buy or sell.
    /// </summary>
    public required decimal Volume { get; init; }

    /// <summary>
    /// Gets or sets the limit price.
    /// </summary>
    public required decimal Price { get; init; }

    /// <summary>
    /// Gets or sets the base currency account ID.
    /// </summary>
    public required long? BaseAccountId { get; init; }

    /// <summary>
    /// Gets or sets the counter currency account ID.
    /// </summary>
    public required long? CounterAccountId { get; init; }

    /// <summary>
    /// Gets or sets the client-defined order ID for idempotency.
    /// </summary>
    public string? ClientOrderId { get; init; }

    /// <summary>
    /// Gets or sets the time in force. Defaults to GTC.
    /// </summary>
    public TimeInForce TimeInForce { get; init; } = TimeInForce.GTC;

    /// <summary>
    /// Gets or sets whether this is a post-only order.
    /// </summary>
    public bool PostOnly { get; init; }

    /// <summary>
    /// Gets or sets the trigger price for stop-limit orders.
    /// </summary>
    public decimal? StopPrice { get; init; }

    /// <summary>
    /// Gets or sets the side of the trigger price to activate the order.
    /// </summary>
    public StopDirection? StopDirection { get; init; }

    /// <summary>
    /// Unix timestamp in milliseconds for request validity.
    /// </summary>
    public long? Timestamp { get; init; }

    /// <summary>
    /// Milliseconds after timestamp for request expiration.
    /// </summary>
    public long? TTL { get; init; }

    /// <summary>
    /// Validates the domain parameters before issuing a command.
    /// </summary>
    /// <exception cref="LunoValidationException">Thrown if rules are violated.</exception>
    public void Validate()
    {
        if (!Enum.IsDefined(typeof(OrderType), Type))
        {
            throw new LunoValidationException($"Invalid OrderType: {Type}");
        }

        if (!Enum.IsDefined(typeof(TimeInForce), TimeInForce))
        {
            throw new LunoValidationException($"Invalid TimeInForce: {TimeInForce}");
        }

        if (StopDirection.HasValue && !Enum.IsDefined(typeof(StopDirection), StopDirection.Value))
        {
            throw new LunoValidationException($"Invalid StopDirection: {StopDirection.Value}");
        }

        if (PostOnly && TimeInForce != TimeInForce.GTC)
        {
            throw new LunoValidationException("PostOnly cannot be used with a TimeInForce other than GTC.");
        }

        if (!BaseAccountId.HasValue || !CounterAccountId.HasValue)
        {
            throw new LunoValidationException("Explicit Account Mandate violated: Both BaseAccountId and CounterAccountId must be explicitly provided to prevent accidental trading on default accounts.");
        }

        if (StopPrice.HasValue != StopDirection.HasValue)
        {
            throw new LunoValidationException("For Stop-Limit orders, both StopPrice and StopDirection must be provided together.");
        }
    }
}
