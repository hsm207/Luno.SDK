using System.Threading;
using System.Threading.Tasks;
using Luno.SDK.Trading;

namespace Luno.SDK.Application.Trading;

/// <summary>
/// A command representing a request from the consumer to place a limit order.
/// </summary>
public record PostLimitOrderCommand
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
}

/// <summary>
/// Orchestrates the process of posting a limit order via the Luno API.
/// </summary>
/// <param name="tradingClient">The specialized trading client used to post order parameters.</param>
public class PostLimitOrderHandler(ILunoTradingClient tradingClient)
{
    /// <summary>
    /// Handles the limit order command.
    /// </summary>
    /// <param name="command">The command parameters.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>A response containing the generated Order ID.</returns>
    public async Task<OrderResponse> HandleAsync(
        PostLimitOrderCommand command,
        CancellationToken ct = default)
    {
        var parameters = new LimitOrderParameters
        {
            Pair = command.Pair,
            Type = command.Type,
            Volume = command.Volume,
            Price = command.Price,
            BaseAccountId = command.BaseAccountId,
            CounterAccountId = command.CounterAccountId,
            ClientOrderId = command.ClientOrderId,
            TimeInForce = command.TimeInForce,
            PostOnly = command.PostOnly,
            StopPrice = command.StopPrice,
            StopDirection = command.StopDirection,
            Timestamp = command.Timestamp,
            TTL = command.TTL
        };

        // Validate domain rules before dispatching
        parameters.Validate();

        var reference = await tradingClient.PostLimitOrderAsync(parameters, ct).ConfigureAwait(false);

        return new OrderResponse { OrderId = reference.OrderId };
    }
}
