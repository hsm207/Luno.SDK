using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Kiota.Abstractions;
using Moq;
using Xunit;
using Luno.SDK.Trading;
using Luno.SDK.Infrastructure.Trading;
using Luno.SDK.Infrastructure.Generated;
using Luno.SDK.Application.Trading;

namespace Luno.SDK.Tests.Unit.Infrastructure;

public class LunoTradingClientBoundaryTests
{
    private readonly Mock<IRequestAdapter> _requestAdapterMock;

    public LunoTradingClientBoundaryTests()
    {
        _requestAdapterMock = new Mock<IRequestAdapter>();
    }

    [Fact(DisplayName = "Given an invalid OrderType bypassing domain, When LunoTradingClient maps type, Then throw InvalidOperationException")]
    public async Task PostLimitOrderAsync_InvalidOrderType_ThrowsInvalidOperationException()
    {
        var client = new LunoTradingClient(_requestAdapterMock.Object);

        // Force an invalid enum that somehow bypassed Request Validation
        var parameters = new LimitOrderParameters
        {
            Pair = "XBTZAR",
            Type = (OrderType)999,
            Volume = 1m,
            Price = 1000m,
            BaseAccountId = 1,
            CounterAccountId = 2
        };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => client.PostLimitOrderAsync(parameters));
        Assert.Contains("Unreachable state due to Domain invariants", ex.Message);
        Assert.IsType<ArgumentOutOfRangeException>(ex.InnerException);
        Assert.Contains("Invalid order type", ex.InnerException.Message);
    }

    [Fact(DisplayName = "Given an invalid TimeInForce bypassing domain, When LunoTradingClient maps type, Then throw InvalidOperationException")]
    public async Task PostLimitOrderAsync_InvalidTimeInForce_ThrowsInvalidOperationException()
    {
        var client = new LunoTradingClient(_requestAdapterMock.Object);

        var parameters = new LimitOrderParameters
        {
            Pair = "XBTZAR",
            Type = OrderType.Bid,
            Volume = 1m,
            Price = 1000m,
            BaseAccountId = 1,
            CounterAccountId = 2,
            TimeInForce = (TimeInForce)999
        };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => client.PostLimitOrderAsync(parameters));
        Assert.Contains("Unreachable state due to Domain invariants", ex.Message);
        Assert.IsType<ArgumentOutOfRangeException>(ex.InnerException);
        Assert.Contains("Invalid time in force", ex.InnerException.Message);
    }

    [Fact(DisplayName = "Given an invalid StopDirection bypassing domain, When LunoTradingClient maps type, Then throw InvalidOperationException")]
    public async Task PostLimitOrderAsync_InvalidStopDirection_ThrowsInvalidOperationException()
    {
        var client = new LunoTradingClient(_requestAdapterMock.Object);

        var parameters = new LimitOrderParameters
        {
            Pair = "XBTZAR",
            Type = OrderType.Bid,
            Volume = 1m,
            Price = 1000m,
            BaseAccountId = 1,
            CounterAccountId = 2,
            StopPrice = 500m,
            StopDirection = (StopDirection)999
        };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => client.PostLimitOrderAsync(parameters));
        Assert.Contains("Unreachable state due to Domain invariants", ex.Message);
        Assert.IsType<ArgumentOutOfRangeException>(ex.InnerException);
        Assert.Contains("Invalid stop direction", ex.InnerException.Message);
    }
}
