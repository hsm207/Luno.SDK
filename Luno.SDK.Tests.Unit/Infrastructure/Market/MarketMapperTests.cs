using Luno.SDK.Market;
using Luno.SDK.Infrastructure.Market;
using GeneratedTicker = Luno.SDK.Infrastructure.Generated.Models.Ticker;
using GeneratedStatus = Luno.SDK.Infrastructure.Generated.Models.Ticker_status;
using GeneratedGetTickerResponse = Luno.SDK.Infrastructure.Generated.Models.GetTickerResponse;
using GeneratedGetTickerStatus = Luno.SDK.Infrastructure.Generated.Models.GetTickerResponse_status;
using Xunit;

namespace Luno.SDK.Tests.Unit.Infrastructure.Market;

[Trait("Category", "Unit")]
public class MarketMapperTests
{
    [Fact(DisplayName = "Given valid ticker DTO, When mapping, Then return populated domain entity.")]
    public void MapToEntity_ValidTickerDto_ReturnsPopulatedEntity()
    {
        // Arrange
        var dto = new GeneratedTicker
        {
            Pair = "XBTZAR",
            Ask = "1000100",
            Bid = "1000000",
            LastTrade = "1000050",
            Rolling24HourVolume = "500",
            Status = GeneratedStatus.ACTIVE,
            Timestamp = 1772555388322L
        };

        // Act
        var result = MarketMapper.MapToEntity(dto);

        // Assert
        Assert.Equal("XBTZAR", result.Pair);
        Assert.Equal(1000100m, result.Ask);
        Assert.Equal(1000000m, result.Bid);
        Assert.Equal(1000050m, result.LastTrade);
        Assert.Equal(500m, result.Rolling24HourVolume);
        Assert.Equal(DateTimeOffset.FromUnixTimeMilliseconds(1772555388322L), result.Timestamp);
        Assert.Equal(MarketStatus.Active, result.Status);
    }

    [Fact(DisplayName = "Given ticker DTO with null pair, When mapping, Then throw LunoMappingException.")]
    public void MapToEntity_PairIsNull_ThrowsLunoMappingException()
    {
        // Arrange
        var dto = new GeneratedTicker { Pair = null };

        // Act & Assert
        var ex = Assert.Throws<LunoMappingException>(() => MarketMapper.MapToEntity(dto));
        Assert.Contains("market pair", ex.Message);
        Assert.Equal(nameof(GeneratedTicker), ex.DtoType);
    }

    [Fact(DisplayName = "Given ticker DTO with null timestamp, When mapping, Then throw LunoMappingException.")]
    public void MapToEntity_TimestampIsNull_ThrowsLunoMappingException()
    {
        // Arrange
        var dto = new GeneratedTicker { Pair = "XBTZAR", Timestamp = null };

        // Act & Assert
        var ex = Assert.Throws<LunoMappingException>(() => MarketMapper.MapToEntity(dto));
        Assert.Contains("timestamp", ex.Message);
        Assert.Equal(nameof(GeneratedTicker), ex.DtoType);
    }

    [Theory(DisplayName = "Given each TickerStatus enum value, When mapping status, Then return equivalent domain MarketStatus.")]
    [InlineData(GeneratedStatus.ACTIVE, MarketStatus.Active)]
    [InlineData(GeneratedStatus.POSTONLY, MarketStatus.PostOnly)]
    [InlineData(GeneratedStatus.DISABLED, MarketStatus.Disabled)]
    public void MapStatus_EnumIsProvided_ReturnsEquivalentStatus(GeneratedStatus status, MarketStatus expected)
    {
        // Act
        var result = MarketMapper.MapStatus(status);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact(DisplayName = "Given null ticker status, When mapping status, Then return MarketStatus.Unknown.")]
    public void MapStatus_Null_ReturnsUnknown()
    {
        // Act
        var result = MarketMapper.MapStatus((GeneratedStatus?)null);

        // Assert
        Assert.Equal(MarketStatus.Unknown, result);
    }

    [Fact(DisplayName = "Given valid GetTickerResponse DTO, When mapping, Then return populated domain entity.")]
    public void MapToEntity_ValidGetTickerResponseDto_ReturnsPopulatedEntity()
    {
        // Arrange
        var dto = new GeneratedGetTickerResponse
        {
            Pair = "XBTZAR",
            Ask = "1000100",
            Bid = "1000000",
            LastTrade = "1000050",
            Rolling24HourVolume = "500",
            Status = GeneratedGetTickerStatus.ACTIVE,
            Timestamp = 1772555388322L
        };

        // Act
        var result = MarketMapper.MapToEntity(dto);

        // Assert
        Assert.Equal("XBTZAR", result.Pair);
        Assert.Equal(1000100m, result.Ask);
        Assert.Equal(1000000m, result.Bid);
        Assert.Equal(1000050m, result.LastTrade);
        Assert.Equal(500m, result.Rolling24HourVolume);
        Assert.Equal(DateTimeOffset.FromUnixTimeMilliseconds(1772555388322L), result.Timestamp);
        Assert.Equal(MarketStatus.Active, result.Status);
    }

    [Fact(DisplayName = "Given GetTickerResponse DTO with null pair, When mapping, Then throw LunoMappingException.")]
    public void MapToEntity_GetTickerResponse_PairIsNull_ThrowsLunoMappingException()
    {
        // Arrange
        var dto = new GeneratedGetTickerResponse { Pair = null };

        // Act & Assert
        var ex = Assert.Throws<LunoMappingException>(() => MarketMapper.MapToEntity(dto));
        Assert.Contains("market pair", ex.Message);
        Assert.Equal(nameof(GeneratedGetTickerResponse), ex.DtoType);
    }

    [Fact(DisplayName = "Given GetTickerResponse DTO with null timestamp, When mapping, Then throw LunoMappingException.")]
    public void MapToEntity_GetTickerResponse_TimestampIsNull_ThrowsLunoMappingException()
    {
        // Arrange
        var dto = new GeneratedGetTickerResponse { Pair = "XBTZAR", Timestamp = null };

        // Act & Assert
        var ex = Assert.Throws<LunoMappingException>(() => MarketMapper.MapToEntity(dto));
        Assert.Contains("timestamp", ex.Message);
        Assert.Equal(nameof(GeneratedGetTickerResponse), ex.DtoType);
    }

    [Theory(DisplayName = "Given each GetTickerResponse_status enum value, When mapping status, Then return equivalent domain MarketStatus.")]
    [InlineData(GeneratedGetTickerStatus.ACTIVE, MarketStatus.Active)]
    [InlineData(GeneratedGetTickerStatus.POSTONLY, MarketStatus.PostOnly)]
    [InlineData(GeneratedGetTickerStatus.DISABLED, MarketStatus.Disabled)]
    public void MapStatus_GetTickerResponse_EnumIsProvided_ReturnsEquivalentStatus(GeneratedGetTickerStatus status, MarketStatus expected)
    {
        // Act
        var result = MarketMapper.MapStatus(status);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact(DisplayName = "Given null GetTickerResponse status, When mapping status, Then return MarketStatus.Unknown.")]
    public void MapStatus_GetTickerResponse_Null_ReturnsUnknown()
    {
        // Act
        var result = MarketMapper.MapStatus((GeneratedGetTickerStatus?)null);

        // Assert
        Assert.Equal(MarketStatus.Unknown, result);
    }
}
