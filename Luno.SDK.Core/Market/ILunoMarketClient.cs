namespace Luno.SDK.Market;

/// <summary>
/// Defines the interface for a client specialized in Luno Market data operations.
/// </summary>
public interface ILunoMarketClient
{
    /// <summary>
    /// Asynchronously retrieves the latest tickers for all available market pairs.
    /// </summary>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>An <see cref="IAsyncEnumerable{T}"/> of <see cref="Ticker"/>.</returns>
    IAsyncEnumerable<Ticker> GetTickersAsync(CancellationToken ct = default);

    /// <summary>
    /// Asynchronously retrieves the latest ticker for a single market pair.
    /// </summary>
    /// <param name="pair">The market pair (e.g., XBTZAR).</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the asynchronous operation, returning a <see cref="Ticker"/>.</returns>
    Task<Ticker> GetTickerAsync(string pair, CancellationToken ct = default);
}
