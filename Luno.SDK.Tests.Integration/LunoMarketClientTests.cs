// Copyright 2026 Google LLC
// Licensed under the Apache License, Version 2.0

using System.Net;
using Luno.SDK;
using Luno.SDK.Core.Market;
using Moq;
using Moq.Protected;
using Xunit;

namespace Luno.SDK.Tests.Integration;

public class LunoMarketClientTests
{
    [Fact(DisplayName = "GetTickersAsync should parse raw Luno JSON into human-soul entities 🏛️💎")]
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

        using var client = new LunoMarketClient(new LunoClientOptions { BaseUrl = "https://api.luno.com" }, httpClient);

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
}
