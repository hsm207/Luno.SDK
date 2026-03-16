using System.Threading;
using System.Threading.Tasks;
using Luno.SDK.Trading;

namespace Luno.SDK;

/// <summary>
/// Defines the low-level data-fetching operations for Luno Trading.
/// This interface is used by handlers to avoid a circular dependency on the command dispatcher.
/// </summary>
public interface ILunoTradingOperations
{
    /// <summary>
    /// Asynchronously posts a new limit order to Luno.
    /// </summary>
    Task<OrderReference> FetchPostLimitOrderAsync(LimitOrderRequest request, CancellationToken ct = default);

    /// <summary>
    /// Asynchronously stops an active order using the Luno standard Order ID.
    /// </summary>
    Task<bool> FetchStopOrderAsync(string orderId, CancellationToken ct = default);

    /// <summary>
    /// Asynchronously retrieves detailed information about an order.
    /// </summary>
    Task<Order> FetchOrderAsync(string? orderId = null, string? clientOrderId = null, CancellationToken ct = default);
}

/// <summary>
/// Defines the full contract for Luno Trading operations, including command dispatching.
/// </summary>
public interface ILunoTradingClient : ILunoTradingOperations
{
    /// <summary>
    /// Gets the command dispatcher used to orchestrate trading application-layer logic.
    /// </summary>
    ILunoCommandDispatcher Commands { get; }
}
