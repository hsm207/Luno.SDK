using System.Threading;
using System.Threading.Tasks;
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
    /// <param name="client">The <see cref="ILunoClient"/> instance to use.</param>
    /// <param name="command">The limit order command parameters.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation, returning the <see cref="OrderResponse"/>.</returns>
    public static Task<OrderResponse> PostLimitOrderAsync(
        this ILunoClient client,
        PostLimitOrderCommand command,
        CancellationToken ct = default)
    {
        var handler = new PostLimitOrderHandler(client.Trading);
        return handler.HandleAsync(command, ct);
    }

    /// <summary>
    /// Asynchronously stops an active order using its command parameters.
    /// </summary>
    /// <param name="client">The <see cref="ILunoClient"/> instance to use.</param>
    /// <param name="command">The stop order command.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns>A response containing the stopped Order ID.</returns>
    public static Task<OrderResponse> StopOrderAsync(
        this ILunoClient client,
        StopOrderCommand command,
        CancellationToken ct = default)
    {
        var handler = new StopOrderHandler(client.Trading);
        return handler.HandleAsync(command, ct);
    }

    /// <summary>
    /// Asynchronously stops an active order using the exchange OrderId.
    /// </summary>
    /// <param name="client">The <see cref="ILunoClient"/> instance to use.</param>
    /// <param name="orderId">The Luno-assigned Order ID.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns>A response containing the stopped Order ID.</returns>
    public static Task<OrderResponse> StopOrderAsync(
        this ILunoClient client,
        string orderId,
        CancellationToken ct = default)
    {
        return client.StopOrderAsync(new StopOrderCommand { OrderId = orderId }, ct);
    }

    /// <summary>
    /// Asynchronously stops an active order using the ClientOrderId.
    /// </summary>
    /// <param name="client">The <see cref="ILunoClient"/> instance to use.</param>
    /// <param name="clientOrderId">The client-defined deduplication ID.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns>A response containing the stopped Order ID.</returns>
    public static Task<OrderResponse> StopOrderByClientOrderIdAsync(
        this ILunoClient client,
        string clientOrderId,
        CancellationToken ct = default)
    {
        return client.StopOrderAsync(new StopOrderCommand { ClientOrderId = clientOrderId }, ct);
    }
}
