using System.Threading;
using System.Threading.Tasks;
using Luno.SDK.Trading;

namespace Luno.SDK.Application.Trading;

/// <summary>
/// A command representing a request to stop an active order.
/// </summary>
public record StopOrderCommand
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
public class StopOrderHandler(ILunoTradingClient tradingClient)
{
    /// <summary>
    /// Handles the stop order command.
    /// </summary>
    /// <param name="command">The command parameters specifying which ID to use.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>True if the order was successfully stopped.</returns>
    public async Task<bool> HandleAsync(
        StopOrderCommand command,
        CancellationToken ct = default)
    {
        if (!string.IsNullOrWhiteSpace(command.OrderId))
        {
            return await tradingClient.StopOrderAsync(command.OrderId, ct).ConfigureAwait(false);
        }

        if (!string.IsNullOrWhiteSpace(command.ClientOrderId))
        {
            return await tradingClient.StopOrderByClientOrderIdAsync(command.ClientOrderId, ct).ConfigureAwait(false);
        }

        throw new LunoValidationException("Either OrderId or ClientOrderId must be provided to stop an order.");
    }
}
