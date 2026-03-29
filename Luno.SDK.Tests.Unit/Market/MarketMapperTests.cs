using Luno.SDK.Market;
using Luno.SDK.Infrastructure.Market;
using GeneratedTicker = Luno.SDK.Infrastructure.Generated.Models.Ticker;
using GeneratedStatus = Luno.SDK.Infrastructure.Generated.Models.Ticker_status;
using GeneratedGetTickerResponse = Luno.SDK.Infrastructure.Generated.Models.GetTickerResponse;
using GeneratedGetTickerStatus = Luno.SDK.Infrastructure.Generated.Models.GetTickerResponse_status;
using GeneratedMarketInfo = Luno.SDK.Infrastructure.Generated.Models.MarketInfo;
using GeneratedMarketInfoStatus = Luno.SDK.Infrastructure.Generated.Models.MarketInfo_trading_status;
using Xunit;

namespace Luno.SDK.Tests.Unit.Market;

public class MarketMapperTests
{
    [Fact(DisplayName = "Given valid ticker DTO, When mapping, Then return populated domain entity.")]
    [Trait("Category", "Unit")]
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
    [Trait("Category", "Unit")]
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
    [Trait("Category", "Unit")]
    public void MapToEntity_TimestampIsNull_ThrowsLunoMappingException()
    {
        // Arrange
        var dto = new GeneratedTicker 
        { 
            Pair = "XBTZAR", 
            Status = GeneratedStatus.ACTIVE, 
            Timestamp = null,
            Ask = "1", Bid = "1", LastTrade = "1", Rolling24HourVolume = "1"
        };

        // Act & Assert
        var ex = Assert.Throws<LunoMappingException>(() => MarketMapper.MapToEntity(dto));
        Assert.Contains("timestamp", ex.Message);
        Assert.Equal(nameof(GeneratedTicker), ex.DtoType);
    }

    [Theory(DisplayName = "Given each TickerStatus enum value, When mapping status, Then return equivalent domain MarketStatus.")]
    [Trait("Category", "Unit")]
    [InlineData(GeneratedStatus.ACTIVE, MarketStatus.Active)]
    [InlineData(GeneratedStatus.POSTONLY, MarketStatus.PostOnly)]
    [InlineData(GeneratedStatus.DISABLED, MarketStatus.Disabled)]
    [InlineData(GeneratedStatus.UNKNOWN, MarketStatus.Unknown)]
    public void MapStatus_EnumIsProvided_ReturnsEquivalentStatus(GeneratedStatus status, MarketStatus expected)
    {
        // Act
        var result = MarketMapper.MapStatus(status);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact(DisplayName = "Given null ticker status, When mapping status, Then throw LunoMappingException.")]
    [Trait("Category", "Unit")]
    public void MapStatus_Null_ThrowsLunoMappingException()
    {
        // Act & Assert
        Assert.Throws<LunoMappingException>(() => MarketMapper.MapStatus((Luno.SDK.Infrastructure.Generated.Models.Ticker_status?)null));
    }

    [Fact(DisplayName = "Given valid GetTickerResponse DTO, When mapping, Then return populated domain entity.")]
    [Trait("Category", "Unit")]
    public void MapToEntity_ValidGetTickerResponse_ReturnsPopulatedEntity()
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
    [Trait("Category", "Unit")]
    public void MapToEntity_GetTickerResponsePairIsNull_ThrowsLunoMappingException()
    {
        // Arrange
        var dto = new GeneratedGetTickerResponse { Pair = null };

        // Act & Assert
        var ex = Assert.Throws<LunoMappingException>(() => MarketMapper.MapToEntity(dto));
        Assert.Contains("market pair", ex.Message);
        Assert.Equal(nameof(GeneratedGetTickerResponse), ex.DtoType);
    }

    [Fact(DisplayName = "Given GetTickerResponse DTO with null timestamp, When mapping, Then throw LunoMappingException.")]
    [Trait("Category", "Unit")]
    public void MapToEntity_GetTickerResponseTimestampIsNull_ThrowsLunoMappingException()
    {
        // Arrange
        var dto = new GeneratedGetTickerResponse 
        { 
            Pair = "XBTZAR", 
            Status = GeneratedGetTickerStatus.ACTIVE, 
            Timestamp = null,
            Ask = "1", Bid = "1", LastTrade = "1", Rolling24HourVolume = "1"
        };

        // Act & Assert
        var ex = Assert.Throws<LunoMappingException>(() => MarketMapper.MapToEntity(dto));
        Assert.Contains("timestamp", ex.Message);
        Assert.Equal(nameof(GeneratedGetTickerResponse), ex.DtoType);
    }

    [Theory(DisplayName = "Given each GetTickerResponse status enum value, When mapping status, Then return equivalent domain MarketStatus.")]
    [Trait("Category", "Unit")]
    [InlineData(GeneratedGetTickerStatus.ACTIVE, MarketStatus.Active)]
    [InlineData(GeneratedGetTickerStatus.POSTONLY, MarketStatus.PostOnly)]
    [InlineData(GeneratedGetTickerStatus.DISABLED, MarketStatus.Disabled)]
    [InlineData(GeneratedGetTickerStatus.UNKNOWN, MarketStatus.Unknown)]
    public void MapStatus_GetTickerResponseEnumIsProvided_ReturnsEquivalentStatus(GeneratedGetTickerStatus status, MarketStatus expected)
    {
        // Act
        var result = MarketMapper.MapStatus(status);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact(DisplayName = "Given null GetTickerResponse status, When mapping status, Then throw LunoMappingException.")]
    [Trait("Category", "Unit")]
    public void MapStatus_GetTickerResponseNull_ThrowsLunoMappingException()
    {
        // Act & Assert
        Assert.Throws<LunoMappingException>(() => MarketMapper.MapStatus((Luno.SDK.Infrastructure.Generated.Models.GetTickerResponse_status?)null));
    }

    [Fact(DisplayName = "Given a force-casted invalid Ticker_status, When mapping status, Then throw LunoMappingException.")]
    [Trait("Category", "Unit")]
    public void MapStatus_InvalidTickerStatus_ThrowsLunoMappingException()
    {
        // Arrange — bypass normal enum values via explicit cast
        var invalidStatus = (Luno.SDK.Infrastructure.Generated.Models.Ticker_status)999;

        // Act & Assert
        Assert.Throws<LunoMappingException>(() => MarketMapper.MapStatus(invalidStatus));
    }

    [Fact(DisplayName = "Given a force-casted invalid GetTickerResponse_status, When mapping status, Then throw LunoMappingException.")]
    [Trait("Category", "Unit")]
    public void MapStatus_InvalidGetTickerResponseStatus_ThrowsLunoMappingException()
    {
        // Arrange
        var invalidStatus = (Luno.SDK.Infrastructure.Generated.Models.GetTickerResponse_status)999;

        // Act & Assert
        Assert.Throws<LunoMappingException>(() => MarketMapper.MapStatus(invalidStatus));
    }

    [Fact(DisplayName = "Given an unparseable decimal string, When mapping ticker, Then throw LunoMappingException (Phase 0 Fix).")]
    [Trait("Category", "Unit")]
    public void MapToEntity_InvalidDecimalString_ThrowsLunoMappingException()
    {
        // Arrange
        var dto = new GeneratedTicker
        {
            Pair = "XBTZAR",
            Ask = "1000100",
            Bid = "InvalidDecimal", // Chaos!
            LastTrade = "1000050",
            Rolling24HourVolume = "500",
            Status = GeneratedStatus.ACTIVE,
            Timestamp = 1772555388322L
        };

        // Act & Assert
        var ex = Assert.Throws<LunoMappingException>(() => MarketMapper.MapToEntity(dto));
        Assert.Contains("Failed to parse decimal value", ex.Message);
    }

    [Fact(DisplayName = "Given MarketInfo DTO with null MinVolume, When mapping, Then throw LunoMappingException.")]
    [Trait("Category", "Unit")]
    public void MapToEntity_MarketInfoNullMinVolume_ThrowsLunoMappingException()
    {
        // Arrange
        var dto = new GeneratedMarketInfo
        {
            MarketId = "XBTZAR",
            BaseCurrency = "XBT",
            CounterCurrency = "ZAR",
            MinVolume = null, // Chaos!
            MaxVolume = "100",
            VolumeScale = 8,
            MinPrice = "1",
            MaxPrice = "1000000",
            PriceScale = 0,
            FeeScale = 8,
            TradingStatus = GeneratedMarketInfoStatus.ACTIVE
        };

        // Act & Assert
        var ex = Assert.Throws<LunoMappingException>(() => MarketMapper.MapToEntity(dto));
        Assert.Contains("Failed to parse decimal value", ex.Message);
    }

    [Fact(DisplayName = "Given MarketInfo DTO with negative or zero MinVolume, When mapping, Then throw LunoDataException.")]
    [Trait("Category", "Unit")]
    public void MapToEntity_MarketInfoZeroMinVolume_ThrowsLunoDataException()
    {
        // Arrange
        var dto = new GeneratedMarketInfo
        {
            MarketId = "XBTZAR",
            BaseCurrency = "XBT",
            CounterCurrency = "ZAR",
            MinVolume = "0", // Invalid invariant!
            MaxVolume = "100",
            VolumeScale = 8,
            MinPrice = "1",
            MaxPrice = "1000000",
            PriceScale = 0,
            FeeScale = 8,
            TradingStatus = GeneratedMarketInfoStatus.ACTIVE
        };

        // Act & Assert
        var ex = Assert.Throws<LunoDataException>(() => MarketMapper.MapToEntity(dto));
        Assert.Contains("Minimum volume must be greater than zero", ex.Message);
    }

    [Fact(DisplayName = "Given MarketInfo DTO with scale > 28, When mapping, Then throw LunoDataException.")]
    [Trait("Category", "Unit")]
    public void MapToEntity_MarketInfoInvalidScale_ThrowsLunoDataException()
    {
        // Arrange
        var dto = new GeneratedMarketInfo
        {
            MarketId = "XBTZAR",
            BaseCurrency = "XBT",
            CounterCurrency = "ZAR",
            MinVolume = "0.00000001",
            MaxVolume = "100",
            VolumeScale = 99, // Chaos! Max scale for decimal is 28
            MinPrice = "1",
            MaxPrice = "1000000",
            PriceScale = 0,
            FeeScale = 8,
            TradingStatus = GeneratedMarketInfoStatus.ACTIVE
        };

        // Act & Assert
        var ex = Assert.Throws<LunoDataException>(() => MarketMapper.MapToEntity(dto));
        Assert.Contains("is outside the allowable range of 0 to 28", ex.Message);
    }

    [Fact(DisplayName = "Given valid MarketInfo DTO, When mapping, Then return populated domain entity.")]
    [Trait("Category", "Unit")]
    public void MapToEntity_ValidMarketInfo_ReturnsPopulatedEntity()
    {
        // Arrange
        var dto = new GeneratedMarketInfo
        {
            MarketId = "XBTMYR",
            BaseCurrency = "XBT",
            CounterCurrency = "MYR",
            MinVolume = "0.0005",
            MaxVolume = "100",
            VolumeScale = 6,
            MinPrice = "1",
            MaxPrice = "1000000",
            PriceScale = 0,
            FeeScale = 8,
            TradingStatus = GeneratedMarketInfoStatus.ACTIVE
        };

        // Act
        var result = MarketMapper.MapToEntity(dto);

        // Assert
        Assert.Equal("XBTMYR", result.Pair);
        Assert.Equal(0.0005m, result.MinVolume);
        Assert.Equal(6, result.VolumeScale);
        Assert.Equal(1m, result.MinPrice);
        Assert.Equal(100m, result.MaxVolume);
    }
}
