using System.Runtime.CompilerServices;
using Luno.SDK.Market;

namespace Luno.SDK.Application.Market;

/// <summary>
/// Returns the latest ticker indicators for a specific pair.
/// </summary>
/// <param name="Pair">The market pair to fetch (e.g., XBTZAR).</param>
public record GetTickerQuery(string Pair);

/// <summary>
/// Orchestrates the retrieval of a single market ticker.
/// </summary>
/// <param name="market">The specialized market client.</param>
internal class GetTickerHandler(ILunoMarketOperations market) : ICommandHandler<GetTickerQuery, TickerResponse>
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
        Validate(query);
        var ticker = await market.FetchTickerAsync(query.Pair, ct);
        return ticker.ToResponse();
    }

    /// <summary>
    /// Validates the query against Application-layer business rules.
    /// </summary>
    private static void Validate(GetTickerQuery query)
    {
        if (string.IsNullOrWhiteSpace(query.Pair))
        {
            throw new LunoValidationException("Pair must be provided to fetch a ticker.");
        }
    }
}
