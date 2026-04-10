using System.Threading;
using System.Threading.Tasks;
using Luno.SDK.Trading;

namespace Luno.SDK.Application.Trading;

/// <summary>
/// A command representing a request to stop an active order.
/// </summary>
public record StopOrderCommand : LunoCommandBase<OrderResponse>
{
    /// <summary>
    /// Gets or sets the Luno-assigned Order ID.
    /// Provide this OR the ClientOrderId.
    /// </summary>
    public string? OrderId { get; init; }

    /// <summary>
    /// Gets or sets the client-defined order ID used for deduplication.
    /// Provide this OR the OrderId.
    /// </summary>
    public string? ClientOrderId { get; init; }
}

/// <summary>
/// Orchestrates the process of stopping an order via the Luno API.
/// </summary>
/// <param name="tradingClient">The specialized trading client.</param>
internal class StopOrderHandler(ILunoTradingOperations tradingClient) : ICommandHandler<StopOrderCommand, OrderResponse>
{
    /// <summary>
    /// Handles the stop order command.
    /// </summary>
    /// <param name="command">The command parameters specifying which ID to use.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>A response containing the Order ID that was stopped.</returns>
    public async Task<OrderResponse> HandleAsync(
        StopOrderCommand command,
        CancellationToken ct = default)
    {
        Validate(command);

        string? orderId = command.OrderId;

        if (string.IsNullOrWhiteSpace(orderId))
        {
            // High-level Policy: Lookup the order to get the exchange-assigned ID if not known.
            var order = await tradingClient.FetchOrderAsync(clientOrderId: command.ClientOrderId, ct: ct).ConfigureAwait(false);

            // Optimization: If the order is already complete (Filled or Cancelled), we satisfy the user's intent immediately.
            if (order.IsClosed)
            {
                return new OrderResponse { OrderId = order.OrderId };
            }

            orderId = order.OrderId;
        }

        // Dispatch the atomic stop operation
        await tradingClient.FetchStopOrderAsync(orderId, ct).ConfigureAwait(false);

        return new OrderResponse { OrderId = orderId };
    }

    private static void Validate(StopOrderCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.OrderId) && string.IsNullOrWhiteSpace(command.ClientOrderId))
        {
            throw new LunoValidationException("Either OrderId or ClientOrderId must be provided to stop an order.");
        }
    }
}
