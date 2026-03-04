using System.Runtime.CompilerServices;
using Luno.SDK.Application.Market;
using Luno.SDK.Core.Market;
using Luno.SDK;
using Xunit;

namespace Luno.SDK.Tests.Unit.Market;

public class LunoMarketExtensionsTests
{
    private class StubLunoMarketClient : ILunoMarketClient
    {
        private readonly IEnumerable<Ticker> _tickers;

        public StubLunoMarketClient(IEnumerable<Ticker> tickers)
        {
            _tickers = tickers;
        }

        public async IAsyncEnumerable<Ticker> GetTickersAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            foreach (var ticker in _tickers)
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return ticker;
                await Task.Yield();
            }
        }
    }

    private class StubLunoClient : ILunoClient
    {
        public ILunoMarketClient Market { get; }
        public ILunoTelemetry Telemetry { get; } = null!;

        public StubLunoClient(ILunoMarketClient market)
        {
            Market = market;
        }
    }

    [Fact(DisplayName = "Given valid client, When GetTickersAsync extension is called, Then return mapped tickers from client")]
    public async Task GetTickersAsync_GivenValidClient_WhenGetTickersAsyncExtensionIsCalled_ThenReturnMappedTickersFromClient()
    {
        // Arrange
        var tickers = new List<Ticker>
        {
            new("XBTZAR", 1000000m, 990000m, 995000m, 50.5m, MarketStatus.Active, DateTimeOffset.UtcNow),
            new("ETHZAR", 50000m, 49000m, 49500m, 120.2m, MarketStatus.Active, DateTimeOffset.UtcNow)
        };
        var stubClient = new StubLunoClient(new StubLunoMarketClient(tickers));

        // Act
        var results = new List<TickerResponse>();
        await foreach (var response in stubClient.GetTickersAsync())
        {
            results.Add(response);
        }

        // Assert
        Assert.Equal(2, results.Count);

        // Explicit coverage for TickerResponse equality
        var response1 = results[0];
        var response2 = new TickerResponse(
            response1.Pair, response1.Price, response1.Spread, response1.IsActive, response1.Timestamp);

        Assert.Equal(response1, response2);
        Assert.Equal(response1.GetHashCode(), response2.GetHashCode());
        Assert.True(response1 == response2);
        Assert.False(response1 != response2);
        Assert.NotNull(response1.ToString());
    }
}
