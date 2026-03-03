using System.Net;
using Luno.SDK;
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
        var handlerMock = new Mock<HttpMessageHandler>();
        var json = "{\"tickers\":[{\"pair\":\"ETHXBT\",\"timestamp\":1772555388322,\"bid\":\"0.03\",\"ask\":\"0.04\",\"last_trade\":\"0.035\",\"rolling_24_hour_volume\":\"10\",\"status\":\"ACTIVE\"}]}";
        
        handlerMock.Protected()
           .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
           .ReturnsAsync(new HttpResponseMessage
           {
               StatusCode = HttpStatusCode.OK,
               Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
           });

        using var httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("https://api.luno.com") };
        
        // Use the real implementation for verification
        using var luno = new LunoClient(httpClient: httpClient);
        var handler = new GetMarketHeartbeatHandler(luno);

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
    }
}
