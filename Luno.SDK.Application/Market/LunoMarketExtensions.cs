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
    /// <param name="client">The <see cref="ILunoClient"/> instance to use for the request.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>An <see cref="IAsyncEnumerable{T}"/> of <see cref="TickerResponse"/> representing the market state.</returns>
    public static IAsyncEnumerable<TickerResponse> GetTickersAsync(
        this ILunoClient client,
        CancellationToken ct = default)
    {
        var handler = new GetTickersHandler(client.Market);
        return handler.HandleAsync(new GetTickersQuery(), ct);
    }
}
