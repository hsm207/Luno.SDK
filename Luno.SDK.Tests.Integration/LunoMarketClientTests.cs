using System.Net;
using Microsoft.Extensions.Logging.Abstractions;
using Luno.SDK;
using Luno.SDK.Core.Market;
using Luno.SDK.Infrastructure.Telemetry;
using Moq;
using Moq.Protected;
using Xunit;

namespace Luno.SDK.Tests.Integration;

public class LunoMarketClientTests
{
    private static LunoMarketClient CreateClient(HttpClient httpClient)
    {
        return new LunoMarketClient(
            httpClient, 
            new LunoTelemetry(), 
            NullLogger.Instance);
    }

    [Fact(DisplayName = "GetTickersAsync should correctly parse and map raw JSON response to domain entities")]
    public async Task GetTickersAsync_WithValidJson_ShouldReturnMappedEntities()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();
        
        var json = "{\"tickers\":[{\"pair\":\"XBTZAR\",\"timestamp\":1772555388322,\"bid\":\"1000000\",\"ask\":\"1000100\",\"last_trade\":\"1000050\",\"rolling_24_hour_volume\":\"500\",\"status\":\"ACTIVE\"}]}";
        
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
        };

        handlerMock
           .Protected()
           .Setup<Task<HttpResponseMessage>>(
              "SendAsync",
              ItExpr.IsAny<HttpRequestMessage>(),
              ItExpr.IsAny<CancellationToken>()
           )
           .ReturnsAsync(response);

        using var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("https://api.luno.com")
        };

        var client = CreateClient(httpClient);

        // Act
        var tickers = new List<Ticker>();
        await foreach (var ticker in client.GetTickersAsync())
        {
            tickers.Add(ticker);
        }

        // Assert
        var result = Assert.Single(tickers);
        Assert.Equal("XBTZAR", result.Pair);
        Assert.Equal(MarketStatus.Active, result.Status);
        Assert.Equal(DateTimeOffset.FromUnixTimeMilliseconds(1772555388322L), result.Timestamp);
    }

    [Fact(DisplayName = "GetTickersAsync should throw InvalidOperationException when API returns null ticker list")]
    public async Task GetTickersAsync_WithNullTickers_ShouldThrowException()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();
        
        // Success status but missing 'tickers' field
        var json = "{}"; 
        
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
        };

        handlerMock
           .Protected()
           .Setup<Task<HttpResponseMessage>>(
              "SendAsync",
              ItExpr.IsAny<HttpRequestMessage>(),
              ItExpr.IsAny<CancellationToken>()
           )
           .ReturnsAsync(response);

        using var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("https://api.luno.com")
        };

        var client = CreateClient(httpClient);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await foreach (var _ in client.GetTickersAsync()) { }
        });
        
        Assert.Equal("API returned a successful response but the ticker list was missing or null.", exception.Message);
    }
}
