using System.Runtime.CompilerServices;
using Luno.SDK.Market;

namespace Luno.SDK.Application.Market;

/// <summary>
/// Returns the latest ticker indicators for a specific pair.
/// </summary>
/// <param name="Pair">The market pair to fetch (e.g., XBTZAR).</param>
public record GetTickerQuery(string Pair);

/// <summary>
/// Orchestrates the retrieval of a single market ticker from the Luno API.
/// </summary>
/// <param name="marketClient">The specialized market client used to fetch raw market data.</param>
public class GetTickerHandler(ILunoMarketClient marketClient)
{
    /// <summary>
    /// Returns the latest ticker indicators for a specific pair.
    /// </summary>
    /// <param name="query">The query parameters.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>A <see cref="TickerResponse"/> object.</returns>
    public async Task<TickerResponse> HandleAsync(
        GetTickerQuery query,
        CancellationToken ct = default)
    {
        var ticker = await marketClient.GetTickerAsync(query.Pair, ct);
        return ticker.ToResponse();
    }
}
