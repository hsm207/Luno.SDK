using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Luno.SDK.Market;

namespace Luno.SDK.Application.Market;

/// <summary>
/// Returns a list of all supported markets and their rules.
/// </summary>
/// <param name="Pairs">Optional market pairs to filter for (e.g., XBTMYR, ETHMYR).</param>
public record GetMarketsQuery(string[]? Pairs = null) : LunoQueryBase<IReadOnlyList<MarketInfo>>;

/// <summary>
/// Orchestrates the retrieval of market information.
/// </summary>
/// <param name="market">The specialized market operations client.</param>
internal class GetMarketsHandler(ILunoMarketOperations market) : ICommandHandler<GetMarketsQuery, IReadOnlyList<MarketInfo>>
{
    /// <summary>
    /// Returns a list of all supported markets and their rules.
    /// </summary>
    /// <param name="query">The query parameters.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>A list of <see cref="MarketInfo"/> objects.</returns>
    public Task<IReadOnlyList<MarketInfo>> HandleAsync(
        GetMarketsQuery query,
        CancellationToken ct = default)
    {
        return market.FetchMarketsAsync(query.Pairs, ct);
    }
}
