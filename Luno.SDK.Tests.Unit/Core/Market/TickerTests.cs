using Luno.SDK.Market;
using Xunit;

namespace Luno.SDK.Tests.Unit.Core.Market;

public class TickerTests
{
    [Fact(DisplayName = "Given ask and bid prices, When accessing spread, Then return the difference.")]
    public void SpreadWhenCalledShouldReturnDifference()
    {
        // Arrange
        var ticker = new Ticker("XBTZAR", 1000100m, 1000000m, 1000050m, 500m, MarketStatus.Active, DateTimeOffset.UtcNow);

        // Act & Assert
        Assert.Equal(100m, ticker.Spread);
    }

    [Theory(DisplayName = "Given various MarketStatus values, When checking IsActive, Then return true only for Active status.")]
    [InlineData(MarketStatus.Active, true)]
    [InlineData(MarketStatus.PostOnly, false)]
    [InlineData(MarketStatus.Disabled, false)]
    [InlineData(MarketStatus.Unknown, false)]
    public void IsActiveWhenStatusIsProvidedShouldReturnCorrectValue(MarketStatus status, bool expected)
    {
        // Arrange
        var ticker = new Ticker("XBTZAR", 1m, 1m, 1m, 1m, status, DateTimeOffset.UtcNow);

        // Act & Assert
        Assert.Equal(expected, ticker.IsActive);
    }
}
