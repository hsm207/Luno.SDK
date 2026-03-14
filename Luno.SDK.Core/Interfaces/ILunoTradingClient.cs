using System.Threading;
using System.Threading.Tasks;
using Luno.SDK.Trading;

namespace Luno.SDK;

/// <summary>
/// Client for managing high-fidelity order lifecycles on the Luno exchange.
/// </summary>
public interface ILunoTradingClient
{
    /// <summary>
    /// Places an idempotent limit order on the exchange.
    /// </summary>
    /// <param name="request">The parameters of the limit order to post.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>A response containing the exchange OrderId.</returns>
    Task<OrderResponse> PostLimitOrderAsync(PostLimitOrderRequest request, CancellationToken ct = default);

    /// <summary>
    /// Stops an existing order using the exchange-provided OrderId.
    /// </summary>
    /// <param name="orderId">The exchange OrderId of the order to stop.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>True if the stop request was successful.</returns>
    Task<bool> StopOrderAsync(string orderId, CancellationToken ct = default);

    /// <summary>
    /// Stops an existing order using the client-provided ClientOrderId.
    /// </summary>
    /// <param name="clientOrderId">The client_order_id UUID of the order to stop.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>True if the stop request was successful.</returns>
    Task<bool> StopOrderByClientOrderIdAsync(string clientOrderId, CancellationToken ct = default);
}
