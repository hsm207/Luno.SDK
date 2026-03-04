using System.Net;
using Luno.SDK;
using Luno.SDK.Core.Market;
using Luno.SDK.Application.Market;
using Moq;
using Moq.Protected;
using Xunit;

namespace Luno.SDK.Tests.Unit;

public class GetMarketHeartbeatTests
{
    [Fact(DisplayName = "GetMarketHeartbeatHandler should correctly orchestrate the client and map entities to responses")]
    public async Task HandleAsync_ShouldMapRealEntitiesToResponses()
    {
        // Arrange
        var marketClientMock = new Mock<ILunoMarketClient>();
        var timestamp = DateTimeOffset.FromUnixTimeMilliseconds(1772555388322L);
        var ticker = new Ticker("ETHXBT", 0.04m, 0.03m, 0.035m, 10m, MarketStatus.Active, timestamp);
        
        marketClientMock
            .Setup(m => m.GetTickersAsync(It.IsAny<CancellationToken>()))
            .Returns(ToAsyncEnumerable(ticker));

        var handler = new GetMarketHeartbeatHandler(marketClientMock.Object);

        // Act
        var responses = new List<MarketHeartbeatResponse>();
        await foreach (var response in handler.HandleAsync(new GetMarketHeartbeatQuery()))
        {
            responses.Add(response);
        }

        // Assert
        var result = Assert.Single(responses);
        Assert.Equal("ETHXBT", result.Pair);
        Assert.Equal(0.035m, result.Price);
        Assert.Equal(0.01m, result.Spread);
        Assert.True(result.IsActive);
        Assert.Equal(timestamp, result.Timestamp);
    }

    private static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(params T[] items)
    {
        foreach (var item in items)
        {
            yield return item;
            await Task.Yield();
        }
    }
}
