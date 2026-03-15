using System.Threading.Tasks;
using Moq;
using Xunit;
using Luno.SDK;
using Luno.SDK.Trading;
using Luno.SDK.Application.Trading;

namespace Luno.SDK.Tests.Unit.Trading;

public class LunoTradingClientTests
{
    private readonly Mock<ILunoTradingClient> _tradingClientMock;
    private readonly Mock<ILunoClient> _lunoClientMock;

    public LunoTradingClientTests()
    {
        _tradingClientMock = new Mock<ILunoTradingClient>();
        _lunoClientMock = new Mock<ILunoClient>();
        _lunoClientMock.Setup(c => c.Trading).Returns(_tradingClientMock.Object);
    }

    [Fact(DisplayName = "Given a request with PostOnly = true and TimeInForce = IOC, When posting limit order, Then throw LunoValidationException.")]
    public async Task PostLimitOrderAsync_PostOnlyWithIOC_ThrowsValidationException()
    {
        // Arrange
        var command = new PostLimitOrderCommand
        {
            Pair = "XBTZAR",
            Type = OrderType.Bid,
            Volume = 1m,
            Price = 1000m,
            BaseAccountId = 1,
            CounterAccountId = 2,
            PostOnly = true,
            TimeInForce = TimeInForce.IOC
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<LunoValidationException>(async () =>
            await _lunoClientMock.Object.PostLimitOrderAsync(command));

        Assert.Contains("PostOnly cannot be used", ex.Message);
    }

    [Fact(DisplayName = "Given a request with null internal accounts, When posting limit order, Then throw LunoValidationException.")]
    public async Task PostLimitOrderAsync_NullAccounts_ThrowsValidationException()
    {
        // Arrange
        var command = new PostLimitOrderCommand
        {
            Pair = "XBTZAR",
            Type = OrderType.Bid,
            Volume = 1m,
            Price = 1000m,
            BaseAccountId = null, // Missing base account
            CounterAccountId = 2
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<LunoValidationException>(async () =>
            await _lunoClientMock.Object.PostLimitOrderAsync(command));

        Assert.Contains("Explicit Account Mandate violated", ex.Message);
    }

    [Fact(DisplayName = "Given an invalid OrderType cast, When validating, Then throw LunoValidationException.")]
    public void Validate_InvalidOrderType_ThrowsValidationException()
    {
        var parameters = new LimitOrderParameters
        {
            Pair = "XBTZAR",
            Type = (OrderType)999, // Invalid cast
            Volume = 1m,
            Price = 1000m,
            BaseAccountId = 1,
            CounterAccountId = 2
        };

        var ex = Assert.Throws<LunoValidationException>(() => parameters.Validate());
        Assert.Contains("Invalid OrderType", ex.Message);
    }

    [Fact(DisplayName = "Given an invalid TimeInForce cast, When validating, Then throw LunoValidationException.")]
    public void Validate_InvalidTimeInForce_ThrowsValidationException()
    {
        var parameters = new LimitOrderParameters
        {
            Pair = "XBTZAR",
            Type = OrderType.Bid,
            Volume = 1m,
            Price = 1000m,
            BaseAccountId = 1,
            CounterAccountId = 2,
            TimeInForce = (TimeInForce)999 // Invalid cast
        };

        var ex = Assert.Throws<LunoValidationException>(() => parameters.Validate());
        Assert.Contains("Invalid TimeInForce", ex.Message);
    }

    [Fact(DisplayName = "Given an invalid StopDirection cast, When validating, Then throw LunoValidationException.")]
    public void Validate_InvalidStopDirection_ThrowsValidationException()
    {
        var parameters = new LimitOrderParameters
        {
            Pair = "XBTZAR",
            Type = OrderType.Bid,
            Volume = 1m,
            Price = 1000m,
            BaseAccountId = 1,
            CounterAccountId = 2,
            StopPrice = 100m,
            StopDirection = (StopDirection)999 // Invalid cast
        };

        var ex = Assert.Throws<LunoValidationException>(() => parameters.Validate());
        Assert.Contains("Invalid StopDirection", ex.Message);
    }

    [Fact]
    public async Task StopOrderAsync_WithOrderId_ReturnsOrderResponse()
    {
        // Arrange
        var orderId = "BX123";

        _tradingClientMock.Setup(x => x.StopOrderAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _lunoClientMock.Object.StopOrderAsync(orderId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(orderId, result.OrderId);
        Assert.True(result.Success);
        _tradingClientMock.Verify(x => x.StopOrderAsync(orderId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task StopOrderAsync_WithClientOrderId_WhenPending_CallsStopOrderAsync()
    {
        // Arrange
        var clientOrderId = "MyClient123";
        var assignedOrderId = "BX123";

        var command = new StopOrderCommand { ClientOrderId = clientOrderId };

        var pendingOrder = new Order { OrderId = assignedOrderId, ClientOrderId = clientOrderId, Status = OrderStatus.Pending };

        _tradingClientMock.Setup(x => x.GetOrderAsync(null, clientOrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pendingOrder);

        _tradingClientMock.Setup(x => x.StopOrderAsync(assignedOrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _lunoClientMock.Object.StopOrderAsync(command);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(assignedOrderId, result.OrderId);
        
        // Ensure both the lookup and the stop were called
        _tradingClientMock.Verify(x => x.GetOrderAsync(null, clientOrderId, It.IsAny<CancellationToken>()), Times.Once);
        _tradingClientMock.Verify(x => x.StopOrderAsync(assignedOrderId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
