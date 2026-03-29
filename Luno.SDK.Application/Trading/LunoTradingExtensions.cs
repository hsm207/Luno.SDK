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
        return client.Commands.DispatchAsync<PostLimitOrderCommand, Task<OrderResponse>>(command, ct);
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
        return client.Commands.DispatchAsync<StopOrderCommand, Task<OrderResponse>>(command, ct);
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
        return client.Commands.DispatchAsync<ListOrdersQuery, Task<IReadOnlyList<OrderDetailsResponse>>>(query, ct);
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
}
