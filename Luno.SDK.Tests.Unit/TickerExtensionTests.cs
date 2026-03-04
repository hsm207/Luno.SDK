using Luno.SDK.Application.Market;
using Luno.SDK.Core.Market;
using Moq;
using Xunit;

namespace Luno.SDK.Tests.Unit;

public class TickerExtensionTests
{
    [Fact(DisplayName = "ListTickersAsync should delegate call to specialized market client via handler")]
    public async Task ListTickersAsync_ShouldDelegateToMarketClient()
    {
        // Arrange
        var clientMock = new Mock<ILunoClient>();
        var marketClientMock = new Mock<ILunoMarketClient>();
        
        clientMock.Setup(c => c.Market).Returns(marketClientMock.Object);
        
        var ticker = new Ticker("XBTZAR", 1000100m, 1000000m, 1000050m, 500m, MarketStatus.Active, DateTimeOffset.UtcNow);
        
        marketClientMock
            .Setup(m => m.GetTickersAsync(It.IsAny<CancellationToken>()))
            .Returns(new[] { ticker }.ToAsyncEnumerable());

        // Act
        var results = new List<TickerResponse>();
        await foreach (var response in clientMock.Object.ListTickersAsync())
        {
            results.Add(response);
        }

        // Assert
        var result = Assert.Single(results);
        Assert.Equal("XBTZAR", result.Pair);
        marketClientMock.Verify(m => m.GetTickersAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
