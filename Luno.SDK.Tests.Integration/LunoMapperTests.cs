using Luno.SDK.Core.Market;
using Luno.SDK.Infrastructure.Market;
using CoreTicker = Luno.SDK.Core.Market.Ticker;
using GeneratedTicker = Luno.SDK.Infrastructure.Generated.Models.Ticker;
using GeneratedStatus = Luno.SDK.Infrastructure.Generated.Models.Ticker_status;
using Xunit;

namespace Luno.SDK.Tests.Integration;

public class LunoMapperTests
{
    [Fact(DisplayName = "LunoMapper should correctly map a valid generated ticker to the domain entity")]
    public void MapToEntity_WithValidGeneratedTicker_ShouldReturnMappedEntity()
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
        var result = LunoMapper.MapToEntity(dto);

        // Assert
        Assert.Equal("XBTZAR", result.Pair);
        Assert.Equal(1000100m, result.Ask);
        Assert.Equal(1000000m, result.Bid);
        Assert.Equal(1000050m, result.LastTrade);
        Assert.Equal(500m, result.Rolling24HourVolume);
        Assert.Equal(DateTimeOffset.FromUnixTimeMilliseconds(1772555388322L), result.Timestamp);
        Assert.Equal(MarketStatus.Active, result.Status);

        // Derived domain properties
        Assert.Equal(100m, result.Spread);
        Assert.True(result.IsActive);
    }

    [Theory(DisplayName = "LunoMapper should map all generated ticker statuses to domain equivalents")]
    [InlineData(GeneratedStatus.ACTIVE, MarketStatus.Active)]
    [InlineData(GeneratedStatus.POSTONLY, MarketStatus.PostOnly)]
    [InlineData(GeneratedStatus.DISABLED, MarketStatus.Disabled)]
    [InlineData(null, MarketStatus.Unknown)]
    public void MapStatus_ShouldMapCorrectly(GeneratedStatus? status, MarketStatus expected)
    {
        // Act
        var result = LunoMapper.MapStatus(status);

        // Assert
        Assert.Equal(expected, result);
    }
}
