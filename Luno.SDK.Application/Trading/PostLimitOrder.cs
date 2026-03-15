using System.Threading;
using System.Threading.Tasks;
using Luno.SDK.Trading;

namespace Luno.SDK.Application.Trading;

/// <summary>
/// A command representing a request from the consumer to place a limit order.
/// </summary>
public record PostLimitOrderCommand
{
    /// <summary>Gets or sets the currency pair (e.g., XBTZAR).</summary>
    public required string Pair { get; init; }

    /// <summary>Gets or sets the order type (Bid or Ask).</summary>
    public required OrderType Type { get; init; }

    /// <summary>Gets or sets the volume to buy or sell.</summary>
    public required decimal Volume { get; init; }

    /// <summary>Gets or sets the limit price.</summary>
    public required decimal Price { get; init; }

    /// <summary>Gets or sets the base currency account ID.</summary>
    public required long? BaseAccountId { get; init; }

    /// <summary>Gets or sets the counter currency account ID.</summary>
    public required long? CounterAccountId { get; init; }

    /// <summary>Gets or sets the client-defined order ID for idempotency.</summary>
    public string? ClientOrderId { get; init; }

    /// <summary>Gets or sets the time in force. Defaults to GTC.</summary>
    public TimeInForce TimeInForce { get; init; } = TimeInForce.GTC;

    /// <summary>Gets or sets whether this is a post-only order.</summary>
    public bool PostOnly { get; init; }

    /// <summary>Gets or sets the trigger price for stop-limit orders.</summary>
    public decimal? StopPrice { get; init; }

    /// <summary>Gets or sets the side of the trigger price to activate the order.</summary>
    public StopDirection? StopDirection { get; init; }

    /// <summary>Unix timestamp in milliseconds for request validity.</summary>
    public long? Timestamp { get; init; }

    /// <summary>Milliseconds after timestamp for request expiration.</summary>
    public long? TTL { get; init; }
}

/// <summary>
/// Orchestrates the process of posting a limit order via the Luno API.
/// Owns validation, boundary-DTO mapping, and idempotency reconciliation.
/// </summary>
/// <param name="tradingClient">The specialized trading client used to post order parameters.</param>
public class PostLimitOrderHandler(ILunoTradingClient tradingClient)
{
    /// <summary>Handles the limit order command.</summary>
    /// <param name="command">The command parameters.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>A response containing the generated Order ID.</returns>
    public async Task<OrderResponse> HandleAsync(PostLimitOrderCommand command, CancellationToken ct = default)
    {
        // 1. Build and validate the Domain Value Object (stays in Application; never crosses into Infra).
        var parameters = new LimitOrderParameters
        {
            Pair             = command.Pair,
            Type             = command.Type,
            Volume           = command.Volume,
            Price            = command.Price,
            BaseAccountId    = command.BaseAccountId,
            CounterAccountId = command.CounterAccountId,
            ClientOrderId    = command.ClientOrderId,
            TimeInForce      = command.TimeInForce,
            PostOnly         = command.PostOnly,
            StopPrice        = command.StopPrice,
            StopDirection    = command.StopDirection,
            Timestamp        = command.Timestamp,
            TTL              = command.TTL,
        };
        parameters.Validate();

        // 2. Map validated domain params to the behavior-free boundary DTO before crossing into Infrastructure.
        var request = new LimitOrderRequest
        {
            Pair             = parameters.Pair,
            Type             = parameters.Type,
            Volume           = parameters.Volume,
            Price            = parameters.Price,
            BaseAccountId    = parameters.BaseAccountId,
            CounterAccountId = parameters.CounterAccountId,
            ClientOrderId    = parameters.ClientOrderId,
            TimeInForce      = parameters.TimeInForce,
            PostOnly         = parameters.PostOnly,
            StopPrice        = parameters.StopPrice,
            StopDirection    = parameters.StopDirection,
            Timestamp        = parameters.Timestamp,
            TTL              = parameters.TTL,
        };

        // 3. Post the order. Let LunoIdempotencyException propagate when no ClientOrderId was given.
        try
        {
            var reference = await tradingClient.PostLimitOrderAsync(request, ct).ConfigureAwait(false);
            return new OrderResponse { OrderId = reference.OrderId };
        }
        catch (LunoIdempotencyException) when (!string.IsNullOrWhiteSpace(parameters.ClientOrderId))
        {
            return await ReconcileDuplicateAsync(parameters, ct).ConfigureAwait(false);
        }
    }

    // ── Private helpers ──────────────────────────────────────────────────────────

    private async Task<OrderResponse> ReconcileDuplicateAsync(LimitOrderParameters expected, CancellationToken ct)
    {
        var existing = await tradingClient.GetOrderAsync(clientOrderId: expected.ClientOrderId, ct: ct)
                                          .ConfigureAwait(false);

        EnsureParametersMatch(existing, expected);

        return new OrderResponse { OrderId = existing.OrderId };
    }

    /// <summary>
    /// Validates that a previously-placed order with the same ClientOrderId
    /// matches the current request's parameters. Throws on the first mismatch.
    /// </summary>
    private static void EnsureParametersMatch(Order existing, LimitOrderParameters expected)
    {
        if (existing.LimitPrice.HasValue && existing.LimitPrice != expected.Price)
            throw new LunoIdempotencyException(
                $"Idempotency conflict: existing order has Price={existing.LimitPrice} but request has Price={expected.Price}.");

        if (existing.LimitVolume.HasValue && existing.LimitVolume != expected.Volume)
            throw new LunoIdempotencyException(
                $"Idempotency conflict: existing order has Volume={existing.LimitVolume} but request has Volume={expected.Volume}.");

        if (existing.Side.HasValue && existing.Side != expected.Type)
            throw new LunoIdempotencyException(
                $"Idempotency conflict: existing order has Side={existing.Side} but request has Side={expected.Type}.");
    }
}
