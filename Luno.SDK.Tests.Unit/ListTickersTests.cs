using Luno.SDK.Application.Market;
using Luno.SDK.Core.Market;
using Moq;
using Xunit;

namespace Luno.SDK.Tests.Unit;

public class ListTickersTests
{
    [Fact(DisplayName = "HandleAsync should correctly map domain Tickers to application TickerResponse")]
    public async Task HandleAsync_ShouldMapTickerToResponse()
    {
        // Arrange
        var marketClientMock = new Mock<ILunoMarketClient>();
        var timestamp = DateTimeOffset.UtcNow;
        // Ask should be higher than Bid for a positive spread
        var ticker = new Ticker("XBTZAR", 1000100m, 1000000m, 1000050m, 500m, MarketStatus.Active, timestamp);

        marketClientMock
            .Setup(m => m.GetTickersAsync(It.IsAny<CancellationToken>()))
            .Returns(new[] { ticker }.ToAsyncEnumerable());

        var handler = new ListTickersHandler(marketClientMock.Object);

        // Act
        var results = new List<TickerResponse>();
        await foreach (var response in handler.HandleAsync(new ListTickersQuery()))
        {
            results.Add(response);
        }

        // Assert
        var result = Assert.Single(results);
        Assert.Equal("XBTZAR", result.Pair);
        Assert.Equal(1000050m, result.Price);
        Assert.Equal(100m, result.Spread);
        Assert.True(result.IsActive);
        Assert.Equal(timestamp, result.Timestamp);
    }
}

internal static class TestExtensions
{
    public static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(this IEnumerable<T> enumerable)
    {
        foreach (var item in enumerable)
        {
            yield return await Task.FromResult(item);
        }
    }
}
