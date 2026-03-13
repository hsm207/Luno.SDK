using System.Runtime.CompilerServices;
using Luno.SDK.Market;

namespace Luno.SDK.Application.Market;

/// <summary>
/// Returns the latest ticker indicators from all active Luno exchanges.
/// </summary>
public record GetTickersQuery;

/// <summary>
/// Orchestrates the retrieval of market tickers from the Luno API.
/// </summary>
/// <param name="marketClient">The specialized market client used to fetch raw market data.</param>
public class GetTickersHandler(ILunoMarketClient marketClient)
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
        await foreach (var ticker in marketClient.GetTickersAsync(ct).WithCancellation(ct))
        {
            yield return ticker.ToResponse();
        }
    }
}
