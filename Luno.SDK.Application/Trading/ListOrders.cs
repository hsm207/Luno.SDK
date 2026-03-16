using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Luno.SDK.Trading;

namespace Luno.SDK.Application.Trading;

/// <summary>
/// A query to retrieve a list of orders based on various filters.
/// </summary>
/// <param name="State">Optional filter for order state (e.g. PENDING, COMPLETE).</param>
/// <param name="Pair">Optional filter for currency pair (e.g. XBTZAR).</param>
/// <param name="CreatedBefore">Optional filter for orders created before this timestamp.</param>
/// <param name="Limit">Optional limit on the number of results (defaults to 100, max 1000).</param>
public record ListOrdersQuery(
    OrderStatus? State = null,
    string? Pair = null,
    long? CreatedBefore = null,
    long? Limit = null);

/// <summary>
/// Orchestrates the retrieval of a list of orders.
/// </summary>
/// <param name="trading">The specialized trading operations client.</param>
public class ListOrdersHandler(ILunoTradingOperations trading) : ICommandHandler<ListOrdersQuery, Task<IReadOnlyList<OrderDetailsResponse>>>
{
    /// <summary>
    /// Handles the retrieval of the order list.
    /// </summary>
    /// <param name="query">The query parameters.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>A list of mapped <see cref="OrderDetailsResponse"/> objects.</returns>
    public async Task<IReadOnlyList<OrderDetailsResponse>> HandleAsync(ListOrdersQuery query, CancellationToken ct = default)
    {
        var orders = await trading.FetchListOrdersAsync(
            state: query.State,
            pair: query.Pair,
            createdBefore: query.CreatedBefore,
            limit: query.Limit,
            ct: ct).ConfigureAwait(false);

        return orders.Select(o => o.ToResponse()).ToList();
    }
}

/// <summary>
/// Internal mapping glue for Trading application DTOs.
/// </summary>
internal static class TradingMappingExtensions
{
    public static OrderDetailsResponse ToResponse(this Order order)
    {
        return new OrderDetailsResponse
        {
            OrderId = order.OrderId,
            CreationTimestamp = DateTimeOffset.FromUnixTimeMilliseconds(order.CreationTimestamp),
            ExpirationTimestamp = order.ExpirationTimestamp.HasValue 
                ? DateTimeOffset.FromUnixTimeMilliseconds(order.ExpirationTimestamp.Value) 
                : null,
            State = order.Status,
            Pair = order.Pair,
            Type = order.Side,
            LimitVolume = order.LimitVolume,
            LimitPrice = order.LimitPrice,
            FilledBase = order.FilledBase ?? 0,
            FilledCounter = order.FilledCounter ?? 0,
            FeeBase = order.FeeBase ?? 0,
            FeeCounter = order.FeeCounter ?? 0
        };
    }
}
