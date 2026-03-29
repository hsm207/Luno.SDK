using System.Net;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;
using Moq;
using Moq.Protected;
using Luno.SDK.Application.Market;
using Luno.SDK.Infrastructure.Generated;
using Xunit;

namespace Luno.SDK.Tests.Unit.Application.Market;

/// <summary>
/// Unit tests for <see cref="GetTickersHandler"/>.
/// Redundant happy-path tests have been removed as they are covered by LunoMarketClientTests (Integration).
/// Unique edge cases like cancellation and equality are preserved.
/// </summary>
public class GetTickersTests
{
    private static LunoMarketClient CreateMarketClient(HttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.luno.com") };
        var adapter = new HttpClientRequestAdapter(new AnonymousAuthenticationProvider(), httpClient: httpClient);
        var apiClient = new LunoApiClient(adapter);
        return new LunoMarketClient(apiClient, new Mock<ILunoCommandDispatcher>().Object);
    }

    [Fact(DisplayName = "Given cancellation token is cancelled, When handling query, Then stop streaming results.")]
    public async Task Handle_Cancelled_StopsStreaming()
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
    public void Equality_IdenticalTickerResponses_ReturnsTrue()
    {
        var timestamp = DateTimeOffset.UtcNow;
        var r1 = new TickerResponse("XBTZAR", 100m, 10m, true, timestamp);
        var r2 = new TickerResponse("XBTZAR", 100m, 10m, true, timestamp);

        Assert.Equal(r1, r2);
        Assert.Equal(r1.GetHashCode(), r2.GetHashCode());
        Assert.NotNull(r1.ToString());
    }
}
