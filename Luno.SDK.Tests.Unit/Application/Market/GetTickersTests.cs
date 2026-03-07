using System.Net;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;
using Moq;
using Moq.Protected;
using Luno.SDK.Application.Market;
using Xunit;

namespace Luno.SDK.Tests.Unit.Application.Market;

public class GetTickersTests
{
    private static LunoMarketClient CreateMarketClient(HttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.luno.com") };
        var adapter = new HttpClientRequestAdapter(new AnonymousAuthenticationProvider(), httpClient: httpClient);
        return new LunoMarketClient(adapter);
    }

    [Fact(DisplayName = "Given market client returns tickers, When handling query, Then stream mapped ticker responses.")]
    public async Task HandleWhenApiSucceedsShouldStreamMappedResponses()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();
        var json = "{\"tickers\":[{\"pair\":\"XBTZAR\",\"timestamp\":1772555388322,\"bid\":\"1000000\",\"ask\":\"1000100\",\"last_trade\":\"1000050\",\"rolling_24_hour_volume\":\"500\",\"status\":\"ACTIVE\"}]}";

        handlerMock
           .Protected()
           .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
           .ReturnsAsync(new HttpResponseMessage
           {
               StatusCode = HttpStatusCode.OK,
               Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
           });

        var marketClient = CreateMarketClient(handlerMock.Object);
        var handler = new GetTickersHandler(marketClient);

        // Act
        var results = new List<TickerResponse>();
        await foreach (var response in handler.HandleAsync(new GetTickersQuery()))
        {
            results.Add(response);
        }

        // Assert
        Assert.Single(results);
        Assert.Equal("XBTZAR", results[0].Pair);
        Assert.Equal(1000050m, results[0].Price);
        Assert.Equal(100m, results[0].Spread);
        Assert.True(results[0].IsActive);
    }

    [Fact(DisplayName = "Given market client returns empty stream, When handling query, Then stream nothing.")]
    public async Task HandleWhenApiReturnsEmptyShouldStreamNothing()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();
        var json = "{\"tickers\":[]}";

        handlerMock
           .Protected()
           .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
           .ReturnsAsync(new HttpResponseMessage
           {
               StatusCode = HttpStatusCode.OK,
               Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
           });

        var marketClient = CreateMarketClient(handlerMock.Object);
        var handler = new GetTickersHandler(marketClient);

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
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
           .Protected()
           .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
           .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK });

        var marketClient = CreateMarketClient(handlerMock.Object);
        var handler = new GetTickersHandler(marketClient);
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act & Assert
        // HttpClient throws TaskCanceledException which inherits from OperationCanceledException.
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await foreach (var _ in handler.HandleAsync(new GetTickersQuery(), cts.Token)) { }
        });
    }

    [Fact(DisplayName = "Given identical TickerResponses, When compared, Then return true")]
    public void Equality_GivenIdenticalTickerResponses_WhenCompared_ThenReturnTrue()
    {
        var timestamp = DateTimeOffset.UtcNow;
        var r1 = new TickerResponse("XBTZAR", 100m, 10m, true, timestamp);
        var r2 = new TickerResponse("XBTZAR", 100m, 10m, true, timestamp);

        Assert.Equal(r1, r2);
        Assert.Equal(r1.GetHashCode(), r2.GetHashCode());
        Assert.NotNull(r1.ToString());
    }
}
