using Luno.SDK.Core.Market;
using Luno.SDK.Application.Market;
using Xunit;

namespace Luno.SDK.Tests.Unit;

public class GetTickersTests
{
    private class StubLunoMarketClient(IEnumerable<Ticker> tickers) : ILunoMarketClient
    {
        public async IAsyncEnumerable<Ticker> GetTickersAsync(
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
        {
            foreach (var ticker in tickers)
            {
                ct.ThrowIfCancellationRequested();
                yield return ticker;
                await Task.Yield(); // Ensure it's actually async!
            }
        }
    }

    [Fact(DisplayName = "Given market client returns tickers, When handling query, Then stream mapped ticker responses.")]
    public async Task HandleWhenApiSucceedsShouldStreamMappedResponses()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var tickers = new List<Ticker>
        {
            new("XBTZAR", 1000000m, 990000m, 995000m, 50.5m, MarketStatus.Active, now),
            new("ETHZAR", 50000m, 49000m, 49500m, 120.2m, MarketStatus.Active, now)
        };

        var handler = new GetTickersHandler(new StubLunoMarketClient(tickers));

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
        
        Assert.Equal("ETHZAR", results[1].Pair);
        Assert.Equal(49500m, results[1].Price);
        Assert.Equal(1000m, results[1].Spread);
    }

    [Fact(DisplayName = "Given market client returns empty stream, When handling query, Then stream nothing.")]
    public async Task HandleWhenApiReturnsEmptyShouldStreamNothing()
    {
        // Arrange
        var handler = new GetTickersHandler(new StubLunoMarketClient(Enumerable.Empty<Ticker>()));

        // Act
        var results = new List<TickerResponse>();
        await foreach (var response in handler.HandleAsync(new GetTickersQuery()))
        {
            results.Add(response);
        }

        // Assert
        Assert.Empty(results);
    }

    [Fact(DisplayName = "Given cancellation token is cancelled, When handling query, Then stop streaming results.")]
    public async Task HandleWhenCancelledShouldStopStreaming()
    {
        // Arrange
        var tickers = new List<Ticker> { new("XBTZAR", 1m, 1m, 1m, 1m, MarketStatus.Active, DateTimeOffset.UtcNow) };
        var handler = new GetTickersHandler(new StubLunoMarketClient(tickers));
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
        {
            await foreach (var _ in handler.HandleAsync(new GetTickersQuery(), cts.Token)) { }
        });
    }
}
