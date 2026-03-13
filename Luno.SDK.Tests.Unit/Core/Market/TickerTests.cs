using Luno.SDK.Market;
using Xunit;

namespace Luno.SDK.Tests.Unit.Core.Market;

[Trait("Category", "Unit")]
public class TickerTests
{
    [Fact(DisplayName = "Given ask and bid prices, When accessing spread, Then return the difference.")]
    public void Spread_Always_ReturnsDifference()
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
    public void IsActive_StatusIsProvided_ReturnsCorrectValue(MarketStatus status, bool expected)
    {
        // Arrange
        var ticker = new Ticker("XBTZAR", 1m, 1m, 1m, 1m, status, DateTimeOffset.UtcNow);

        // Act & Assert
        Assert.Equal(expected, ticker.IsActive);
    }

    [Fact(DisplayName = "Given identical tickers, When compared, Then return true")]
    public void Equality_IdenticalTickers_ReturnsTrue()
    {
        var timestamp = DateTimeOffset.UtcNow;
        var ticker1 = new Ticker("XBTZAR", 100m, 90m, 95m, 1m, MarketStatus.Active, timestamp);
        var ticker2 = new Ticker("XBTZAR", 100m, 90m, 95m, 1m, MarketStatus.Active, timestamp);

        Assert.Equal(ticker1, ticker2);
        Assert.Equal(ticker1.GetHashCode(), ticker2.GetHashCode());
        Assert.NotNull(ticker1.ToString());
    }
}
