using System;
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

    /// <summary>Gets or sets the order side (Buy or Sell).</summary>
    public required OrderSide Side { get; init; }

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
public class PostLimitOrderHandler(ILunoTradingOperations tradingClient) : ICommandHandler<PostLimitOrderCommand, Task<OrderResponse>>
{
    /// <summary>Handles the limit order command.</summary>
    /// <param name="command">The command parameters.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>A response containing the generated Order ID.</returns>
    public async Task<OrderResponse> HandleAsync(PostLimitOrderCommand command, CancellationToken ct = default)
    {
        // 1. Validate the command.
        Validate(command);

        // 2. Map directly to the boundary DTO
        var request = new LimitOrderRequest
        {
            Pair             = command.Pair,
            Side             = command.Side,
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

        // 3. Post the order. Let LunoIdempotencyException propagate when no ClientOrderId was given.
        try
        {
            var reference = await tradingClient.FetchPostLimitOrderAsync(request, ct).ConfigureAwait(false);
            return new OrderResponse { OrderId = reference.OrderId };
        }
        catch (LunoIdempotencyException) when (!string.IsNullOrWhiteSpace(request.ClientOrderId))
        {
            return await ReconcileDuplicateAsync(request, ct).ConfigureAwait(false);
        }
    }

    // ── Private helpers ──────────────────────────────────────────────────────────

    /// <summary>
    /// Validates the command against Application-layer business rules.
    /// These rules share the same "reason to change" as the Use Case itself (SRP).
    /// </summary>
    private static void Validate(PostLimitOrderCommand command)
    {
        if (!Enum.IsDefined(typeof(OrderSide), command.Side))
        {
            throw new LunoValidationException($"Invalid OrderSide: {command.Side}");
        }

        if (!Enum.IsDefined(typeof(TimeInForce), command.TimeInForce))
        {
            throw new LunoValidationException($"Invalid TimeInForce: {command.TimeInForce}");
        }

        if (command.StopDirection.HasValue && !Enum.IsDefined(typeof(StopDirection), command.StopDirection.Value))
        {
            throw new LunoValidationException($"Invalid StopDirection: {command.StopDirection.Value}");
        }

        if (command.PostOnly && command.TimeInForce != TimeInForce.GTC)
        {
            throw new LunoValidationException("PostOnly cannot be used with a TimeInForce other than GTC.");
        }

        if (command.BaseAccountId.GetValueOrDefault() <= 0 || command.CounterAccountId.GetValueOrDefault() <= 0)
        {
            throw new LunoValidationException("Explicit Account Mandate violated: Both BaseAccountId and CounterAccountId must be explicitly provided with valid positive IDs to prevent accidental trading on default accounts.");
        }

        if (command.StopPrice.HasValue != command.StopDirection.HasValue)
        {
            throw new LunoValidationException("For Stop-Limit orders, both StopPrice and StopDirection must be provided together.");
        }
    }

    private async Task<OrderResponse> ReconcileDuplicateAsync(LimitOrderRequest expected, CancellationToken ct)
    {
        // ARCHITECTURAL DECISION: The audit identified the downcast in EnsureParametersMatch 
        // as an LSP violation. However, we intentionally retain this 'tension' in the Application 
        // layer to preserve the purity of surrounding layers:
        // 1. ILunoTradingOperations remains a 'dumb' 1:1 wrapper of the Kiota client (no 'fake' typed queries).
        // 2. The Order Domain Entity remains agnostic of Application-layer parameter DTOs.
        var existing = await tradingClient.FetchOrderAsync(clientOrderId: expected.ClientOrderId, ct: ct)
                                          .ConfigureAwait(false);

        EnsureParametersMatch(existing, expected);

        return new OrderResponse { OrderId = existing.OrderId };
    }

    /// <summary>
    /// Validates that a previously-placed order with the same ClientOrderId
    /// matches the current request's parameters. Throws on the first mismatch.
    /// </summary>
    private static void EnsureParametersMatch(Order existing, LimitOrderRequest expected)
    {
        if (existing.Pair != expected.Pair)
            throw new LunoIdempotencyException(
                $"Idempotency conflict: existing order has Pair={existing.Pair} but request has Pair={expected.Pair}.");

        if (existing.Side != expected.Side)
            throw new LunoIdempotencyException(
                $"Idempotency conflict: existing order has Side={existing.Side} but request has Side={expected.Side}.");

        if (existing is not LimitOrder limitOrder)
            throw new LunoIdempotencyException(
                $"Idempotency conflict: existing order has Type={existing.Type} but request has Type={OrderType.Limit}.");

        if (limitOrder.LimitPrice != expected.Price)
            throw new LunoIdempotencyException(
                $"Idempotency conflict: existing order has Price={limitOrder.LimitPrice} but request has Price={expected.Price}.");

        if (limitOrder.LimitVolume != expected.Volume)
            throw new LunoIdempotencyException(
                $"Idempotency conflict: existing order has Volume={limitOrder.LimitVolume} but request has Volume={expected.Volume}.");

        if (existing.BaseAccountId != expected.BaseAccountId)
            throw new LunoIdempotencyException(
                $"Idempotency conflict: existing order has BaseAccountId={existing.BaseAccountId} but request has BaseAccountId={expected.BaseAccountId}.");

        if (existing.CounterAccountId != expected.CounterAccountId)
            throw new LunoIdempotencyException(
                $"Idempotency conflict: existing order has CounterAccountId={existing.CounterAccountId} but request has CounterAccountId={expected.CounterAccountId}.");

        if (limitOrder.TimeInForce != expected.TimeInForce)
            throw new LunoIdempotencyException(
                $"Idempotency conflict: existing order has TimeInForce={limitOrder.TimeInForce} but request has TimeInForce={expected.TimeInForce}.");
    }
}
