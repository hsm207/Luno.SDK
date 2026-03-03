// Copyright 2026 Google LLC
// Licensed under the Apache License, Version 2.0

using System.Net;
using Luno.SDK;
using Luno.SDK.Application.Market;
using Moq;
using Moq.Protected;
using Xunit;

namespace Luno.SDK.Tests.Unit;

public class LunoMarketExtensionsTests
{
    [Fact(DisplayName = "GetMarketHeartbeatAsync should exercise the FULL human stack! 🏛️💎")]
    public async Task GetMarketHeartbeatAsync_ShouldStreamFromRealClient()
    {
        // Arrange
        // We mock the NETWORK, not our code! 🛡️✨
        var handlerMock = new Mock<HttpMessageHandler>();
        var json = "{\"tickers\":[{\"pair\":\"XBTZAR\",\"timestamp\":1772555388322,\"bid\":\"1000000\",\"ask\":\"1000100\",\"last_trade\":\"1000050\",\"rolling_24_hour_volume\":\"500\",\"status\":\"ACTIVE\"}]}";
        
        handlerMock.Protected()
           .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
           .ReturnsAsync(new HttpResponseMessage
           {
               StatusCode = HttpStatusCode.OK,
               Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
           });

        using var httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("https://api.luno.com") };
        
        // Use the REAL implementation! 🤌✨
        using var luno = new LunoClient(httpClient: httpClient);

        // Act
        var responses = new List<MarketHeartbeatResponse>();
        await foreach (var response in luno.GetMarketHeartbeatAsync())
        {
            responses.Add(response);
        }

        // Assert
        var result = Assert.Single(responses);
        Assert.Equal("XBTZAR", result.Pair);
        Assert.Equal(1000050m, result.Price); // Verifies the Mapper worked! 🗺️✨
        Assert.True(result.IsActive);         // Verifies the Entity logic! 💓
    }
}
