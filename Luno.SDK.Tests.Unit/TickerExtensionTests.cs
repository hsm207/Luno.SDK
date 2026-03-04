using Moq;
using Luno.SDK.Core.Market;
using Luno.SDK.Application.Market;

namespace Luno.SDK.Tests.Unit;

public class TickerExtensionTests
{
    [Fact(DisplayName = "GetTickersAsync should delegate call to specialized market client via handler")]
    public async Task GetTickersAsync_ShouldDelegateToMarketClient()
    {
        // Arrange
        var tickers = new List<Ticker>
        {
            new("XBTZAR", 1000000m, 990000m, 995000m, 50.5m, MarketStatus.Active, DateTimeOffset.UtcNow)
        };

        var marketClientMock = new Mock<ILunoMarketClient>();
        marketClientMock.Setup(x => x.GetTickersAsync(It.IsAny<CancellationToken>()))
            .Returns(tickers.ToAsyncEnumerable());

        var clientMock = new Mock<ILunoClient>();
        clientMock.Setup(x => x.Market).Returns(marketClientMock.Object);

        // Act
        await foreach (var response in clientMock.Object.GetTickersAsync())
        {
            // Assert
            Assert.Equal("XBTZAR", response.Pair);
            Assert.Equal(995000m, response.Price);
        }

        marketClientMock.Verify(x => x.GetTickersAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
