using Luno.SDK.Market;
using Luno.SDK.Application.Market;

namespace Luno.SDK;

/// <summary>
/// Provides fluent extension methods for Luno Market operations.
/// </summary>
public static class LunoMarketExtensions
{
    /// <summary>
    /// Asynchronously fetches a stream of market tickers for all available pairs.
    /// </summary>
    /// <param name="client">The <see cref="ILunoMarketClient"/> instance to use for the request.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>An <see cref="IAsyncEnumerable{T}"/> of <see cref="TickerResponse"/> representing the market state.</returns>
    public static IAsyncEnumerable<TickerResponse> GetTickersAsync(
        this ILunoMarketClient client,
        CancellationToken ct = default)
    {
        return client.GetTickersAsync(pairs: null, ct: ct);
    }

    /// <summary>
    /// Asynchronously fetches a stream of market tickers for the specified pairs.
    /// </summary>
    /// <param name="client">The <see cref="ILunoMarketClient"/> instance to use for the request.</param>
    /// <param name="pairs">The market pairs to filter for (e.g., XBTMYR, ETHMYR).</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>An <see cref="IAsyncEnumerable{T}"/> of <see cref="TickerResponse"/> representing the market state.</returns>
    public static IAsyncEnumerable<TickerResponse> GetTickersAsync(
        this ILunoMarketClient client,
        IEnumerable<string>? pairs,
        CancellationToken ct = default)
    {
        return client.Commands.DispatchAsync<GetTickersQuery, IAsyncEnumerable<TickerResponse>>(new GetTickersQuery(pairs?.ToArray()), ct);
    }

    /// <summary>
    /// Asynchronously fetches a market ticker for a specific pair.
    /// </summary>
    /// <param name="client">The <see cref="ILunoMarketClient"/> instance to use for the request.</param>
    /// <param name="pair">The market pair to fetch (e.g., XBTZAR).</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, returning a <see cref="TickerResponse"/> representing the market state.</returns>
    public static Task<TickerResponse> GetTickerAsync(
        this ILunoMarketClient client,
        string pair,
        CancellationToken ct = default)
    {
        return client.Commands.DispatchAsync<GetTickerQuery, Task<TickerResponse>>(new GetTickerQuery(pair), ct);
    }
}
