using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Luno.SDK.Market;
using Luno.SDK.Application.Market;

namespace Luno.SDK;

/// <summary>
/// Provides fluent extension methods for Luno Market operations.
/// All operations follow a unified (Request, CancellationToken) pattern to ensure architectural consistency.
/// </summary>
public static class LunoMarketExtensions
{
    /// <summary>
    /// Asynchronously fetches a stream of market tickers.
    /// Processing is handled by <see cref="GetTickersHandler"/>.
    /// </summary>
    /// <param name="client">The <see cref="ILunoMarketClient"/> instance to use.</param>
    /// <param name="query">The query parameters defining which pairs to fetch.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns>An asynchronous stream of <see cref="TickerResponse"/> objects.</returns>
    public static IAsyncEnumerable<TickerResponse> GetTickersAsync(
        this ILunoMarketClient client,
        GetTickersQuery query,
        CancellationToken ct = default)
    {
        return client.Requests.CreateStreamAsync(query, ct);
    }

    /// <summary>
    /// Asynchronously fetches a market ticker for a specific pair.
    /// Processing is handled by <see cref="GetTickerHandler"/>.
    /// </summary>
    /// <param name="client">The <see cref="ILunoMarketClient"/> instance to use.</param>
    /// <param name="query">The query parameters defining the pair to fetch.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns>A task representing the asynchronous operation, returning a <see cref="TickerResponse"/>.</returns>
    public static Task<TickerResponse> GetTickerAsync(
        this ILunoMarketClient client,
        GetTickerQuery query,
        CancellationToken ct = default)
    {
        return client.Requests.SendAsync(query, ct);
    }

    /// <summary>
    /// Asynchronously fetches a list of markets and their associated rules.
    /// Processing is handled by <see cref="GetMarketsHandler"/>.
    /// </summary>
    /// <param name="client">The <see cref="ILunoMarketClient"/> instance to use.</param>
    /// <param name="query">The query parameters defining filters for the markets.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns>A list of <see cref="MarketInfo"/> objects.</returns>
    public static Task<IReadOnlyList<MarketInfo>> GetMarketsAsync(
        this ILunoMarketClient client,
        GetMarketsQuery query,
        CancellationToken ct = default)
    {
        return client.Requests.SendAsync(query, ct);
    }
}
