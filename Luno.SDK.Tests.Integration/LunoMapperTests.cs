// Copyright 2026 Google LLC
// Licensed under the Apache License, Version 2.0

using Luno.SDK.Core.Market; // Updated! 🤌
using Luno.SDK.Infrastructure.Market; // Updated! 🤌
using CoreTicker = Luno.SDK.Core.Market.Ticker;
using GeneratedTicker = Luno.SDK.Infrastructure.Generated.Models.Ticker;
using GeneratedStatus = Luno.SDK.Infrastructure.Generated.Models.Ticker_status;
using Xunit;

namespace Luno.SDK.Tests.Integration;

public class LunoMapperTests
{
    [Fact(DisplayName = "LunoMapper should handle valid generated tickers perfectly! 🤖🤌")]
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
        Assert.Equal(DateTimeOffset.FromUnixTimeMilliseconds(1772555388322L), result.Timestamp);
        Assert.Equal(MarketStatus.Active, result.Status);
    }

    [Theory(DisplayName = "LunoMapper should map all generated statuses correctly 🗺️🤌")]
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
