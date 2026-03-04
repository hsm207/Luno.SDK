using System.Runtime.CompilerServices;
using Luno.SDK.Core.Market;

namespace Luno.SDK.Application.Market;

/// <summary>
/// Represents a query to list all current market tickers.
/// </summary>
public record ListTickersQuery;

/// <summary>
/// Represents the application-layer response containing ticker data for a specific pair.
/// </summary>
/// <param name="Pair">The market pair (e.g., XBTZAR).</param>
/// <param name="Price">The last trade price.</param>
/// <param name="Spread">The current bid/ask spread.</param>
/// <param name="IsActive">Indicates if the market is currently active.</param>
/// <param name="Timestamp">The timestamp of the ticker data.</param>
public record TickerResponse(
    string Pair,
    decimal Price,
    decimal Spread,
    bool IsActive,
    DateTimeOffset Timestamp
);

/// <summary>
/// Orchestrates the retrieval of market tickers from the Luno API.
/// </summary>
/// <param name="marketClient">The specialized market client used to fetch raw market data.</param>
public class ListTickersHandler(ILunoMarketClient marketClient)
{
    /// <summary>
    /// Handles the request to list tickers and streams the results as application-layer DTOs.
    /// </summary>
    /// <param name="query">The query parameters.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>A stream of <see cref="TickerResponse"/> objects.</returns>
    public async IAsyncEnumerable<TickerResponse> HandleAsync(
        ListTickersQuery query, 
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        await foreach (var ticker in marketClient.GetTickersAsync(ct))
        {
            yield return new TickerResponse(
                ticker.Pair,
                ticker.LastTrade,
                ticker.Spread,
                ticker.IsActive,
                ticker.Timestamp
            );
        }
    }
}
