using System.Net;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;
using Moq;
using Moq.Protected;
using Luno.SDK.Application.Market;
using Luno.SDK.Infrastructure.Market;
using Xunit;
using Luno.SDK;

namespace Luno.SDK.Tests.Unit.Application.Market;

[Trait("Category", "Unit")]
public class LunoMarketExtensionsTests
{
    private static ILunoClient CreateClient(HttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.luno.com") };
        var adapter = new HttpClientRequestAdapter(new AnonymousAuthenticationProvider(), httpClient: httpClient);

        var clientMock = new Mock<ILunoClient>();
        clientMock.Setup(c => c.Market).Returns(new LunoMarketClient(adapter));

        return clientMock.Object;
    }

    [Fact(DisplayName = "Given valid ILunoClient, When GetTickersAsync is called, Then routes correctly and returns response.")]
    public async Task GetTickersAsync_ValidClient_RoutesCorrectly()
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

        var client = CreateClient(handlerMock.Object);

        // Act
        var results = new List<TickerResponse>();
        await foreach (var response in client.GetTickersAsync(CancellationToken.None))
        {
            results.Add(response);
        }

        // Assert
        Assert.Single(results);
        Assert.Equal("XBTZAR", results[0].Pair);
    }

    [Fact(DisplayName = "Given valid ILunoClient, When GetTickerAsync is called, Then routes correctly and returns response.")]
    public async Task GetTickerAsync_ValidClient_RoutesCorrectly()
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

        var client = CreateClient(handlerMock.Object);

        // Act
        var result = await client.GetTickerAsync("XBTZAR", CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("XBTZAR", result.Pair);
    }
}
