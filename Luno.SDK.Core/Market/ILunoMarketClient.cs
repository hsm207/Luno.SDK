using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Luno.SDK.Market;

/// <summary>
/// Defines the low-level data-fetching operations for Luno Market.
/// This interface is used by handlers to avoid a circular dependency on the command dispatcher.
/// </summary>
internal interface ILunoMarketOperations
{
    /// <summary>
    /// Asynchronously fetches a stream of market tickers.
    /// </summary>
    /// <param name="pairs">Optional market pairs to filter for.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns>An <see cref="IAsyncEnumerable{T}"/> of <see cref="Ticker"/>.</returns>
    IAsyncEnumerable<Ticker> FetchTickersAsync(string[]? pairs = null, CancellationToken ct = default);
    
    /// <summary>
    /// Asynchronously fetches a market ticker for a specific pair.
    /// </summary>
    /// <param name="pair">The market pair to fetch (e.g. XBTZAR).</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns>A <see cref="Task"/> returning the <see cref="Ticker"/>.</returns>
    Task<Ticker> FetchTickerAsync(string pair, CancellationToken ct = default);

    /// <summary>
    /// Asynchronously fetches a list of markets.
    /// </summary>
    /// <param name="pairs">Optional market pairs to filter for.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns>A <see cref="Task"/> returning a list of <see cref="MarketInfo"/>.</returns>
    Task<IReadOnlyList<MarketInfo>> FetchMarketsAsync(string[]? pairs = null, CancellationToken ct = default);
}

/// <summary>
/// Defines the full contract for Luno Market data operations, including command dispatching.
/// </summary>
public interface ILunoMarketClient
{
    /// <summary>
    /// Gets the request dispatcher used to orchestrate market application-layer logic.
    /// </summary>
    ILunoRequestDispatcher Requests { get; }
}
