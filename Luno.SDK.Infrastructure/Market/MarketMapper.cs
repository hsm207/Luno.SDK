using System.Globalization;
using System.Text.Json;
using Luno.SDK.Market;
using GeneratedTicker = Luno.SDK.Infrastructure.Generated.Models.Ticker;
using GeneratedStatus = Luno.SDK.Infrastructure.Generated.Models.Ticker_status;
using GeneratedGetTickerResponse = Luno.SDK.Infrastructure.Generated.Models.GetTickerResponse;
using GeneratedGetTickerStatus = Luno.SDK.Infrastructure.Generated.Models.GetTickerResponse_status;
using GeneratedMarketInfo = Luno.SDK.Infrastructure.Generated.Models.MarketInfo;
using GeneratedMarketInfoStatus = Luno.SDK.Infrastructure.Generated.Models.MarketInfo_trading_status;
using System.Runtime.CompilerServices;

namespace Luno.SDK.Infrastructure.Market;

/// <summary>
/// Provides mapping functionality to convert between generated DTOs and domain entities.
/// </summary>
internal static class MarketMapper
{
    // Production-verified scale ranges (Empirically discovered on 2026-03-28)
    private const int MinPriceScale = -3;
    private const int MaxPriceScale = 8;
    private const int MinVolumeScale = 0;
    private const int MaxVolumeScale = 6;
    private const int MinFeeScale = 8;
    private const int MaxFeeScale = 8;

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

    /// <summary>
    /// Maps a generated single ticker response DTO to a domain entity.
    /// </summary>
    /// <exception cref="LunoMappingException">Thrown when the ticker DTO is missing required data.</exception>
    public static Ticker MapToEntity(GeneratedGetTickerResponse dto) => new(
        dto.Pair ?? throw new LunoMappingException("API returned a ticker without a valid market pair identifier.", nameof(GeneratedGetTickerResponse)),
        ParseDecimal(dto.Ask),
        ParseDecimal(dto.Bid),
        ParseDecimal(dto.LastTrade),
        ParseDecimal(dto.Rolling24HourVolume),
        MapStatus(dto.Status),
        DateTimeOffset.FromUnixTimeMilliseconds(GetTimestamp(dto))
    );

    /// <summary>
    /// Maps a generated market info DTO to a domain entity.
    /// </summary>
    /// <exception cref="LunoMappingException">Thrown when mapping fails.</exception>
    /// <exception cref="LunoDataException">Thrown when data invariants are violated.</exception>
    public static MarketInfo MapToEntity(GeneratedMarketInfo dto)
    {
        var minVolume = ParseDecimal(dto.MinVolume);

        if (minVolume <= 0)
        {
            throw new LunoDataException($"Minimum volume must be greater than zero, got {minVolume}");
        }

        return new MarketInfo
        {
            Pair = dto.MarketId ?? throw new LunoMappingException("API returned a market without a valid market id.", nameof(GeneratedMarketInfo)),
            Status = MapStatus(dto.TradingStatus),
            BaseCurrency = dto.BaseCurrency ?? throw new LunoMappingException("API returned a market without a valid base currency.", nameof(GeneratedMarketInfo)),
            CounterCurrency = dto.CounterCurrency ?? throw new LunoMappingException("API returned a market without a valid counter currency.", nameof(GeneratedMarketInfo)),
            MinVolume = minVolume,
            MaxVolume = ParseDecimal(dto.MaxVolume),
            VolumeScale = DowncastScale(dto.VolumeScale, MinVolumeScale, MaxVolumeScale),
            MinPrice = ParseDecimal(dto.MinPrice),
            MaxPrice = ParseDecimal(dto.MaxPrice),
            PriceScale = DowncastScale(dto.PriceScale, MinPriceScale, MaxPriceScale),
            FeeScale = DowncastScale(dto.FeeScale, MinFeeScale, MaxFeeScale)
        };
    }

    private static int DowncastScale(long? scale, int min, int max, [CallerArgumentExpression("scale")] string paramName = "")
    {
        if (scale is null)
            throw new LunoMappingException($"Missing scale property.", paramName);

        // Rationale: These ranges were empirically verified against all 144 Luno markets on 2026-03-28.
        if (scale < min || scale > max)
            throw new LunoDataException($"Scale {scale} for '{paramName}' is outside the empirically verified production range of {min} to {max}.");

        return (int)scale.Value;
    }

    private static long GetTimestamp(GeneratedTicker dto) =>
        dto.Timestamp ?? throw new LunoMappingException("API returned a ticker without a valid timestamp.", nameof(GeneratedTicker));

    private static long GetTimestamp(GeneratedGetTickerResponse dto) =>
        dto.Timestamp ?? throw new LunoMappingException("API returned a ticker without a valid timestamp.", nameof(GeneratedGetTickerResponse));

    private static decimal ParseDecimal(string? value, [CallerArgumentExpression("value")] string paramName = "") =>
        decimal.TryParse(value, CultureInfo.InvariantCulture, out var result) ? result : throw new LunoMappingException($"Failed to parse decimal value '{value}'.", paramName);

    /// <summary>
    /// Maps the generated ticker status to the domain status enum.
    /// </summary>
    public static MarketStatus MapStatus(GeneratedStatus? status) => status switch
    {
        GeneratedStatus.ACTIVE => MarketStatus.Active,
        GeneratedStatus.POSTONLY => MarketStatus.PostOnly,
        GeneratedStatus.DISABLED => MarketStatus.Disabled,
        GeneratedStatus.UNKNOWN => MarketStatus.Unknown,
        _ => throw new LunoMappingException($"Unmapped or null market status '{status}'.", nameof(GeneratedStatus)),
    };

    /// <summary>
    /// Maps the generated GetTickerResponse status to the domain status enum.
    /// </summary>
    public static MarketStatus MapStatus(GeneratedGetTickerStatus? status) => status switch
    {
        GeneratedGetTickerStatus.ACTIVE => MarketStatus.Active,
        GeneratedGetTickerStatus.POSTONLY => MarketStatus.PostOnly,
        GeneratedGetTickerStatus.DISABLED => MarketStatus.Disabled,
        GeneratedGetTickerStatus.UNKNOWN => MarketStatus.Unknown,
        _ => throw new LunoMappingException($"Unmapped or null market status '{status}'.", nameof(GeneratedGetTickerStatus)),
    };

    /// <summary>
    /// Maps the generated MarketInfo status to the domain status enum.
    /// </summary>
    public static MarketStatus MapStatus(GeneratedMarketInfoStatus? status) => status switch
    {
        GeneratedMarketInfoStatus.ACTIVE => MarketStatus.Active,
        GeneratedMarketInfoStatus.POST_ONLY => MarketStatus.PostOnly,
        GeneratedMarketInfoStatus.SUSPENDED => MarketStatus.Suspended,
        GeneratedMarketInfoStatus.UNKNOWN => MarketStatus.Unknown,
        _ => throw new LunoMappingException($"Unmapped or null market status '{status}'.", nameof(GeneratedMarketInfoStatus)),
    };
}
