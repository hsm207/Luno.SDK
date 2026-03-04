using Moq;
using Luno.SDK.Core.Market;
using Luno.SDK.Application.Market;

namespace Luno.SDK.Tests.Unit;

/// <summary>
/// Provides unit tests for the <see cref="GetTickersHandler"/> class.
/// </summary>
public class GetTickersTests
{
    [Fact(DisplayName = "HandleAsync should stream and map tickers from market client")]
    public async Task HandleAsync_ShouldStreamAndMapTickers()
    {
        // Arrange
        var tickers = new List<Ticker>
        {
            new("XBTZAR", 1000000m, 990000m, 995000m, 50.5m, MarketStatus.Active, DateTimeOffset.UtcNow),
            new("ETHZAR", 50000m, 49000m, 49500m, 120.2m, MarketStatus.Active, DateTimeOffset.UtcNow)
        };

        var marketClientMock = new Mock<ILunoMarketClient>();
        marketClientMock.Setup(x => x.GetTickersAsync(It.IsAny<CancellationToken>()))
            .Returns(tickers.ToAsyncEnumerable());

        var handler = new GetTickersHandler(marketClientMock.Object);

        // Act
        var results = new List<TickerResponse>();
        await foreach (var response in handler.HandleAsync(new GetTickersQuery()))
        {
            results.Add(response);
        }

        // Assert
        Assert.Equal(2, results.Count);
        Assert.Equal("XBTZAR", results[0].Pair);
        Assert.Equal(995000m, results[0].Price);
        Assert.Equal(10000m, results[0].Spread);
        Assert.True(results[0].IsActive);
        
        Assert.Equal("ETHZAR", results[1].Pair);
        Assert.Equal(49500m, results[1].Price);
        Assert.Equal(1000m, results[1].Spread);
        Assert.True(results[1].IsActive);
    }
}
