using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Luno.SDK.Market;

/// <summary>
/// Defines the contract for Luno Market data operations.
/// </summary>
public interface ILunoMarketClient
{
    /// <summary>
    /// Gets the command dispatcher used to orchestrate market application-layer logic.
    /// </summary>
    ILunoCommandDispatcher Commands { get; }

    /// <summary>
    /// Asynchronously fetches a stream of market tickers for all available pairs.
    /// </summary>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns>An <see cref="IAsyncEnumerable{T}"/> of <see cref="Ticker"/>.</returns>
    IAsyncEnumerable<Ticker> FetchTickersAsync(CancellationToken ct = default);
    
    /// <summary>
    /// Asynchronously fetches a market ticker for a specific pair.
    /// </summary>
    /// <param name="pair">The market pair to fetch (e.g. XBTZAR).</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns>A <see cref="Task"/> returning the <see cref="Ticker"/>.</returns>
    Task<Ticker> FetchTickerAsync(string pair, CancellationToken ct = default);
}
