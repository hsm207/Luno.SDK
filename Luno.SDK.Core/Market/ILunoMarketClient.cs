using Luno.SDK.Core.Market;

namespace Luno.SDK;

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
}
