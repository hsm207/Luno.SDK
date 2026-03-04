using Luno.SDK.Application.Market;

namespace Luno.SDK;

/// <summary>
/// Provides fluent extension methods for Luno Market operations.
/// </summary>
public static class LunoMarketExtensions
{
    /// <summary>
    /// Asynchronously fetches a stream of market heartbeats (tickers) for all available pairs.
    /// </summary>
    /// <param name="client">The <see cref="ILunoClient"/> instance to use for the request.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>An <see cref="IAsyncEnumerable{T}"/> of <see cref="MarketHeartbeatResponse"/> representing the market state.</returns>
    public static IAsyncEnumerable<MarketHeartbeatResponse> GetMarketHeartbeatAsync(
        this ILunoClient client, 
        CancellationToken ct = default)
    {
        var handler = new GetMarketHeartbeatHandler(client.Market);
        return handler.HandleAsync(new GetMarketHeartbeatQuery(), ct);
    }
}
