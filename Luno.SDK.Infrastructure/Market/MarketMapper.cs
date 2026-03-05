using System.Globalization;
using System.Text.Json;
using Luno.SDK.Market;
using GeneratedTicker = Luno.SDK.Infrastructure.Generated.Models.Ticker;
using GeneratedStatus = Luno.SDK.Infrastructure.Generated.Models.Ticker_status;

namespace Luno.SDK.Infrastructure.Market;

/// <summary>
/// Provides mapping functionality to convert between generated DTOs and domain entities.
/// </summary>
internal static class MarketMapper
{
    /// <summary>
    /// Maps a generated ticker DTO to a domain entity.
    /// </summary>
    /// <exception cref="LunoMappingException">Thrown when the ticker DTO is missing required data.</exception>
    public static Ticker MapToEntity(GeneratedTicker dto) => new(
        dto.Pair ?? throw new LunoMappingException("API returned a ticker without a valid market pair identifier.", nameof(GeneratedTicker)),
        ParseDecimal(dto.Ask),
        ParseDecimal(dto.Bid),
        ParseDecimal(dto.LastTrade),
        ParseDecimal(dto.Rolling24HourVolume),
        MapStatus(dto.Status),
        DateTimeOffset.FromUnixTimeMilliseconds(GetTimestamp(dto))
    );

    private static long GetTimestamp(GeneratedTicker dto) => 
        dto.Timestamp ?? throw new LunoMappingException("API returned a ticker without a valid timestamp.", nameof(GeneratedTicker));

    private static decimal ParseDecimal(string? value) => 
        decimal.TryParse(value, CultureInfo.InvariantCulture, out var result) ? result : 0m;

    /// <summary>
    /// Maps the generated ticker status to the domain status enum.
    /// </summary>
    public static MarketStatus MapStatus(GeneratedStatus? status) => status switch
    {
        GeneratedStatus.ACTIVE => MarketStatus.Active,
        GeneratedStatus.POSTONLY => MarketStatus.PostOnly,
        GeneratedStatus.DISABLED => MarketStatus.Disabled,
        _ => MarketStatus.Unknown
    };
}
