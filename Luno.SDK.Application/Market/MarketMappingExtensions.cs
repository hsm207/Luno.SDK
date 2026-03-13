using Luno.SDK.Market;

namespace Luno.SDK.Application.Market;

/// <summary>
/// Provides extension methods for mapping market domain entities to application-layer responses.
/// </summary>
internal static class MarketMappingExtensions
{
    /// <summary>
    /// Maps a <see cref="Ticker"/> domain entity to a <see cref="TickerResponse"/> DTO.
    /// </summary>
    public static TickerResponse ToResponse(this Ticker ticker) => new(
        ticker.Pair,
        ticker.LastTrade,
        ticker.Spread,
        ticker.IsActive,
        ticker.Timestamp
    );
}
