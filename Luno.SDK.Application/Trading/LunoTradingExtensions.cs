using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Luno.SDK.Trading;
using Luno.SDK.Application.Trading;

namespace Luno.SDK;

/// <summary>
/// Provides fluent extension methods for Luno Trading operations.
/// All operations follow a unified (Request, CancellationToken) pattern to ensure architectural consistency.
/// </summary>
public static class LunoTradingExtensions
{
    /// <summary>
    /// Asynchronously posts a new limit order.
    /// Processing is handled by <see cref="PostLimitOrderHandler"/>.
    /// </summary>
    /// <param name="client">The <see cref="ILunoTradingClient"/> instance to use.</param>
    /// <param name="command">The limit order command parameters.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns>A task representing the asynchronous operation, returning the <see cref="OrderResponse"/>.</returns>
    public static Task<OrderResponse> PostLimitOrderAsync(
        this ILunoTradingClient client,
        PostLimitOrderCommand command,
        CancellationToken ct = default)
    {
        return client.Requests.SendAsync(command, ct);
    }

    /// <summary>
    /// Asynchronously stops an active order.
    /// Processing is handled by <see cref="StopOrderHandler"/>.
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
        return client.Requests.SendAsync(command, ct);
    }

    /// <summary>
    /// Asynchronously retrieves a list of orders.
    /// Processing is handled by <see cref="ListOrdersHandler"/>.
    /// </summary>
    /// <param name="client">The <see cref="ILunoTradingClient"/> instance to use.</param>
    /// <param name="query">The query parameters.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns>A list of mapped <see cref="OrderDetailsResponse"/> objects.</returns>
    public static Task<IReadOnlyList<OrderDetailsResponse>> ListOrdersAsync(
        this ILunoTradingClient client,
        ListOrdersQuery query,
        CancellationToken ct = default)
    {
        return client.Requests.SendAsync(query, ct);
    }

    /// <summary>
    /// Calculates the optimal limit order size (Volume and Price) for a given spend amount.
    /// Processing is handled by <see cref="CalculateOrderSizeHandler"/>.
    /// </summary>
    /// <param name="client">The <see cref="ILunoTradingClient"/> instance to use.</param>
    /// <param name="query">The calculation query parameters.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns>A task representing the asynchronous operation, returning the <see cref="OrderQuote"/>.</returns>
    public static Task<OrderQuote> CalculateOrderSizeAsync(
        this ILunoTradingClient client,
        CalculateOrderSizeQuery query,
        CancellationToken ct = default)
    {
        return client.Requests.SendAsync(query, ct);
    }

    /// <summary>
    /// Transforms the finalized mathematical <see cref="OrderQuote"/> directly into a <see cref="PostLimitOrderCommand"/>.
    /// </summary>
    public static PostLimitOrderCommand ToCommand(
        this OrderQuote quote,
        long baseAccountId, 
        long counterAccountId, 
        string? clientOrderId = null,
        TimeInForce? timeInForce = null,
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
            TimeInForce = timeInForce ?? TimeInForce.GTC,
            PostOnly = postOnly,
            Timestamp = timestamp,
            TTL = ttl
        };
    }
}
