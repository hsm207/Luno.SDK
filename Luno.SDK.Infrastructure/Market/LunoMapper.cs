using System.Globalization;
using System.Text.Json;
using Luno.SDK.Core.Market;
using GeneratedTicker = Luno.SDK.Infrastructure.Generated.Models.Ticker;
using GeneratedStatus = Luno.SDK.Infrastructure.Generated.Models.Ticker_status;

namespace Luno.SDK.Infrastructure.Market;

/// <summary>
/// Provides mapping functionality to convert between generated DTOs and domain entities.
/// </summary>
internal static class LunoMapper
{
    /// <summary>
    /// Maps a generated ticker DTO to a domain entity.
    /// </summary>
    public static Ticker MapToEntity(GeneratedTicker dto) => new(
        dto.Pair ?? string.Empty,
        ParseDecimal(dto.Ask),
        ParseDecimal(dto.Bid),
        ParseDecimal(dto.LastTrade),
        ParseDecimal(dto.Rolling24HourVolume),
        MapStatus(dto.Status),
        DateTimeOffset.FromUnixTimeMilliseconds(GetTimestamp(dto))
    );

    /// <summary>
    /// Maps raw string values to a domain entity.
    /// </summary>
    public static Ticker MapFromRaw(string pair, string? ask, string? bid, string? last, string? vol, string? status, long timestamp) => new(
        pair,
        ParseDecimal(ask),
        ParseDecimal(bid),
        ParseDecimal(last),
        ParseDecimal(vol),
        MapStatus(status),
        DateTimeOffset.FromUnixTimeMilliseconds(timestamp)
    );

    private static long GetTimestamp(GeneratedTicker dto)
    {
        if (dto.Timestamp.HasValue && dto.Timestamp > 0) return Convert.ToInt64(dto.Timestamp.Value);
        if (dto.AdditionalData.TryGetValue("timestamp", out var rawValue) && rawValue is JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Number && element.TryGetInt64(out var value)) return value;
        }
        return 0;
    }

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

    /// <summary>
    /// Maps a string status value to the domain status enum.
    /// </summary>
    public static MarketStatus MapStatus(string? status) => status switch
    {
        "ACTIVE" => MarketStatus.Active,
        "POSTONLY" => MarketStatus.PostOnly,
        "DISABLED" => MarketStatus.Disabled,
        _ => MarketStatus.Unknown
    };
}
