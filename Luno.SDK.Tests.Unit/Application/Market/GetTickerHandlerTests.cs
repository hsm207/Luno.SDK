using System.Net;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;
using Moq;
using Moq.Protected;
using Luno.SDK.Application.Market;
using Luno.SDK.Infrastructure.Market;
using Xunit;

namespace Luno.SDK.Tests.Unit.Application.Market;

[Trait("Category", "Unit")]
public class GetTickerHandlerTests
{
    private static LunoMarketClient CreateMarketClient(HttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.luno.com") };
        var adapter = new HttpClientRequestAdapter(new AnonymousAuthenticationProvider(), httpClient: httpClient);
        return new LunoMarketClient(adapter);
    }

    [Fact(DisplayName = "Given market client returns ticker, When handling query, Then return mapped TickerResponse.")]
    public async Task Handle_ApiSucceeds_ReturnsMappedResponse()
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

        var marketClient = CreateMarketClient(handlerMock.Object);
        var handler = new GetTickerHandler(marketClient);

        // Act
        var result = await handler.HandleAsync(new GetTickerQuery("XBTZAR"));

        // Assert
        Assert.NotNull(result);
        Assert.Equal("XBTZAR", result.Pair);
        Assert.Equal(1000050m, result.Price);
        Assert.Equal(100m, result.Spread);
        Assert.True(result.IsActive);
    }

    [Fact(DisplayName = "Given equality check, When compared, Then return expected result.")]
    public void Equality_IdenticalQueries_ReturnsTrue()
    {
        var q1 = new GetTickerQuery("XBTZAR");
        var q2 = new GetTickerQuery("XBTZAR");

        Assert.Equal(q1, q2);
        Assert.Equal(q1.GetHashCode(), q2.GetHashCode());
        Assert.NotNull(q1.ToString());
    }
}
