using System.Net;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;
using Moq;
using Moq.Protected;
using Luno.SDK.Infrastructure.Market;
using Luno.SDK.Market;
using Xunit;

namespace Luno.SDK.Tests.Unit.Infrastructure.Market;

[Trait("Category", "Unit")]
public class LunoMarketClientTests
{
    private static LunoMarketClient CreateMarketClient(HttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.luno.com") };
        var adapter = new HttpClientRequestAdapter(new AnonymousAuthenticationProvider(), httpClient: httpClient);
        return new LunoMarketClient(adapter);
    }

    [Fact(DisplayName = "Given valid response, When calling GetTickersAsync, Then return mapped Ticker entities.")]
    public async Task GetTickersAsync_ValidResponse_ReturnsTickers()
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

        var client = CreateMarketClient(handlerMock.Object);

        // Act
        var tickers = new List<Ticker>();
        await foreach (var ticker in client.GetTickersAsync(CancellationToken.None))
        {
            tickers.Add(ticker);
        }

        // Assert
        Assert.Single(tickers);
        Assert.Equal("XBTZAR", tickers[0].Pair);
        Assert.Equal(1000100m, tickers[0].Ask);
    }

    [Fact(DisplayName = "Given no tickers in response, When calling GetTickersAsync, Then return empty list.")]
    public async Task GetTickersAsync_EmptyResponse_ReturnsEmptyList()
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

        var client = CreateMarketClient(handlerMock.Object);

        // Act
        var tickers = new List<Ticker>();
        await foreach (var ticker in client.GetTickersAsync(CancellationToken.None))
        {
            tickers.Add(ticker);
        }

        // Assert
        Assert.Empty(tickers);
    }

    [Fact(DisplayName = "Given valid response, When calling GetTickerAsync, Then return mapped Ticker entity.")]
    public async Task GetTickerAsync_ValidResponse_ReturnsTicker()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();
        var json = "{\"pair\":\"XBTZAR\",\"timestamp\":1772555388322,\"bid\":\"1000000\",\"ask\":\"1000100\",\"last_trade\":\"1000050\",\"rolling_24_hour_volume\":\"500\",\"status\":\"ACTIVE\"}";

        handlerMock
           .Protected()
           .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
           .ReturnsAsync(new HttpResponseMessage
           {
               StatusCode = HttpStatusCode.OK,
               Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
           });

        var client = CreateMarketClient(handlerMock.Object);

        // Act
        var result = await client.GetTickerAsync("XBTZAR", CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("XBTZAR", result.Pair);
        Assert.Equal(1000100m, result.Ask);
    }
}
