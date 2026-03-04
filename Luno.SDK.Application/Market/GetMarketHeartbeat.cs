using System.Runtime.CompilerServices;
using Luno.SDK.Core.Market;

namespace Luno.SDK.Application.Market;

/// <summary>
/// Represents a query to retrieve the current market heartbeat.
/// </summary>
public record GetMarketHeartbeatQuery;

/// <summary>
/// Represents the response containing market heartbeat data for a specific pair.
/// </summary>
/// <param name="Pair">The market pair (e.g., XBTZAR).</param>
/// <param name="Price">The last trade price.</param>
/// <param name="Spread">The current bid/ask spread.</param>
/// <param name="IsActive">Indicates if the market is currently active.</param>
/// <param name="Timestamp">The timestamp of the market data.</param>
public record MarketHeartbeatResponse(
    string Pair,
    decimal Price,
    decimal Spread,
    bool IsActive,
    DateTimeOffset Timestamp
);

/// <summary>
/// Orchestrates the retrieval of market heartbeats from the Luno API.
/// </summary>
/// <param name="marketClient">The specialized market client used to fetch raw market data.</param>
public class GetMarketHeartbeatHandler(ILunoMarketClient marketClient)
{
    /// <summary>
    /// Handles the market heartbeat query and streams the results.
    /// </summary>
    /// <param name="query">The query parameters.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>A stream of market heartbeat responses.</returns>
    public async IAsyncEnumerable<MarketHeartbeatResponse> HandleAsync(
        GetMarketHeartbeatQuery query, 
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        await foreach (var ticker in marketClient.GetTickersAsync(ct))
        {
            yield return new MarketHeartbeatResponse(
                ticker.Pair,
                ticker.LastTrade,
                ticker.Spread,
                ticker.IsActive,
                ticker.Timestamp
            );
        }
    }
}
