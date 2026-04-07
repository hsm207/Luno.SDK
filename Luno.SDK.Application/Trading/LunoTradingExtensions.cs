using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Luno.SDK.Trading;
using Luno.SDK.Application.Trading;

namespace Luno.SDK;

/// <summary>
/// Provides fluent extension methods for Luno Trading operations.
/// </summary>
public static class LunoTradingExtensions
{
    /// <summary>
    /// Asynchronously posts a new limit order.
    /// </summary>
    /// <param name="client">The <see cref="ILunoTradingClient"/> instance to use.</param>
    /// <param name="command">The limit order command parameters.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation, returning the <see cref="OrderResponse"/>.</returns>
    public static Task<OrderResponse> PostLimitOrderAsync(
        this ILunoTradingClient client,
        PostLimitOrderCommand command,
        CancellationToken ct = default)
    {
        return client.Commands.DispatchAsync<PostLimitOrderCommand, OrderResponse>(command, ct);
    }

    /// <summary>
    /// Asynchronously stops an active order using its command parameters.
    /// </summary>
    /// <param name="client">The <see cref="ILunoTradingClient"/> instance to use.</param>
    /// <param name="command">The stop order command.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns>A response containing the stopped Order ID.</returns>
    public static Task<OrderResponse> StopOrderAsync(
        this ILunoTradingClient client,
        StopOrderCommand command,
        CancellationToken ct = default)
    {
        return client.Commands.DispatchAsync<StopOrderCommand, OrderResponse>(command, ct);
    }

    /// <summary>
    /// Asynchronously stops an active order using the exchange OrderId.
    /// </summary>
    /// <param name="client">The <see cref="ILunoTradingClient"/> instance to use.</param>
    /// <param name="orderId">The Luno-assigned Order ID.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns>A response containing the stopped Order ID.</returns>
    public static Task<OrderResponse> StopOrderAsync(
        this ILunoTradingClient client,
        string orderId,
        CancellationToken ct = default)
    {
        return client.StopOrderAsync(new StopOrderCommand { OrderId = orderId }, ct);
    }

    /// <summary>
    /// Asynchronously stops an active order using the ClientOrderId.
    /// </summary>
    /// <param name="client">The <see cref="ILunoTradingClient"/> instance to use.</param>
    /// <param name="clientOrderId">The client-defined deduplication ID.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns>A response containing the stopped Order ID.</returns>
    public static Task<OrderResponse> StopOrderByClientOrderIdAsync(
        this ILunoTradingClient client,
        string clientOrderId,
        CancellationToken ct = default)
    {
        return client.StopOrderAsync(new StopOrderCommand { ClientOrderId = clientOrderId }, ct);
    }

    /// <summary>
    /// Asynchronously retrieves a list of orders.
    /// </summary>
    public static Task<IReadOnlyList<OrderDetailsResponse>> ListOrdersAsync(
        this ILunoTradingClient client,
        ListOrdersQuery query,
        CancellationToken ct = default)
    {
        return client.Commands.DispatchAsync<ListOrdersQuery, IReadOnlyList<OrderDetailsResponse>>(query, ct);
    }

    /// <summary>
    /// Asynchronously retrieves a list of orders for a specific pair and state.
    /// </summary>
    public static Task<IReadOnlyList<OrderDetailsResponse>> ListOrdersAsync(
        this ILunoTradingClient client,
        string? pair = null,
        OrderStatus? state = null,
        CancellationToken ct = default)
    {
        return client.ListOrdersAsync(new ListOrdersQuery(State: state, Pair: pair), ct);
    }

    /// <summary>
    /// Calculates the optimal limit order size (Volume and Price) for a given spend amount.
    /// Ensures mathematical boundaries and precision rules are strictly maintained.
    /// </summary>
    public static Task<OrderQuote> CalculateOrderSizeAsync(
        this ILunoTradingClient client,
        CalculateOrderSizeQuery query,
        CancellationToken ct = default)
    {
        return client.Commands.DispatchAsync<CalculateOrderSizeQuery, OrderQuote>(query, ct);
    }

    /// <summary>
    /// Transforms the finalized mathematical <see cref="OrderQuote"/> directly into a <see cref="PostLimitOrderCommand"/>.
    /// </summary>
    public static PostLimitOrderCommand ToCommand(
        this OrderQuote quote,
        long baseAccountId, 
        long counterAccountId, 
        string? clientOrderId = null,
        TimeInForce timeInForce = TimeInForce.GTC,
        bool postOnly = false,
        long? timestamp = null,
        long? ttl = null)
    {
        return new PostLimitOrderCommand
        {
            Pair = quote.Pair,
            Side = quote.Side,
            Volume = quote.Volume,
            Price = quote.Price,
            BaseAccountId = baseAccountId,
            CounterAccountId = counterAccountId,
            ClientOrderId = clientOrderId,
            TimeInForce = timeInForce,
            PostOnly = postOnly,
            Timestamp = timestamp,
            TTL = ttl
        };
    }
}
