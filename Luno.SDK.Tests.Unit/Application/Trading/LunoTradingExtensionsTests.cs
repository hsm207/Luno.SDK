using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Luno.SDK;
using Luno.SDK.Trading;
using Luno.SDK.Application.Trading;

namespace Luno.SDK.Tests.Unit.Application.Trading;

public class LunoTradingExtensionsTests
{
    [Fact(DisplayName = "Given an OrderQuote, When mapped ToCommand, Then all properties map correctly")]
    public void OrderQuote_ToCommand_MapsAllProperties()
    {
        // Arrange
        var quote = new OrderQuote("XBTMYR", OrderSide.Buy, 0.001m, 250000m, "MYR");
        long baseAccount = 1;
        long counterAccount = 2;
        string clientOrderId = "test-ref-123";
        TimeInForce tif = TimeInForce.FOK;
        long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        // Act
        var cmd = quote.ToCommand(baseAccount, counterAccount, clientOrderId, tif, postOnly: true, timestamp: timestamp);

        // Assert
        Assert.Equal("XBTMYR", cmd.Pair);
        Assert.Equal(OrderSide.Buy, cmd.Side);
        Assert.Equal(0.001m, cmd.Volume);
        Assert.Equal(250000m, cmd.Price);
        Assert.Equal(baseAccount, cmd.BaseAccountId);
        Assert.Equal(counterAccount, cmd.CounterAccountId);
        Assert.Equal(clientOrderId, cmd.ClientOrderId);
        Assert.Equal(tif, cmd.TimeInForce);
        Assert.True(cmd.PostOnly);
        Assert.Equal(timestamp, cmd.Timestamp);
    }

    [Fact(DisplayName = "Given a trading client, When CalculateOrderSizeAsync invoked, Then dispatcher is called")]
    public async Task TradingClient_CalculateOrderSizeAsync_DispatchesQuery()
    {
        // Arrange
        var clientMock = new Mock<ILunoTradingClient>();
        var dispatcherMock = new Mock<ILunoCommandDispatcher>();
        clientMock.SetupGet(c => c.Commands).Returns(dispatcherMock.Object);

        var query = new CalculateOrderSizeQuery("XBTMYR", OrderSide.Sell, TradingAmount.InQuote(100m));
        var quote = new OrderQuote("XBTMYR", OrderSide.Sell, 0.001m, 250000m, "MYR");

        dispatcherMock
            .Setup(d => d.DispatchAsync<CalculateOrderSizeQuery, OrderQuote>(query, It.IsAny<LunoRequestOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(quote);

        // Act
        var result = await clientMock.Object.CalculateOrderSizeAsync(query);

        // Assert
        Assert.Equal(quote, result);
        dispatcherMock.Verify(d => d.DispatchAsync<CalculateOrderSizeQuery, OrderQuote>(query, It.IsAny<LunoRequestOptions>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
