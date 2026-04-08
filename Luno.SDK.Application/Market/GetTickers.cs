using System.Runtime.CompilerServices;
using Luno.SDK.Market;

namespace Luno.SDK.Application.Market;

/// <summary>
/// Returns the latest ticker indicators from all active Luno exchanges.
/// </summary>
/// <param name="Pairs">Optional market pairs to filter for (e.g., XBTMYR, ETHMYR).</param>
public record GetTickersQuery(string[]? Pairs = null) : LunoQueryBase<TickerResponse>;

/// <summary>
/// Orchestrates the retrieval of market tickers.
/// </summary>
/// <param name="market">The specialized market client.</param>
internal class GetTickersHandler(ILunoMarketOperations market) : IStreamCommandHandler<GetTickersQuery, TickerResponse>
{
    /// <summary>
    /// Returns the latest ticker indicators from all active Luno exchanges.
    /// </summary>
    /// <param name="query">The query parameters.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>A stream of <see cref="TickerResponse"/> objects.</returns>
    public async IAsyncEnumerable<TickerResponse> HandleAsync(
        GetTickersQuery query,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        await foreach (var ticker in market.FetchTickersAsync(query.Pairs, ct).WithCancellation(ct))
        {
            yield return ticker.ToResponse();
        }
    }
}
