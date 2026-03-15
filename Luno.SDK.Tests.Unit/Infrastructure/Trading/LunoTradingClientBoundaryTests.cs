using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Kiota.Abstractions;
using Moq;
using Xunit;
using Luno.SDK.Trading;
using Luno.SDK.Infrastructure.Trading;
using Luno.SDK.Infrastructure.Generated;

namespace Luno.SDK.Tests.Unit.Infrastructure;

/// <summary>
/// Regression tests that intentionally bypass the Domain validation layer by casting invalid enum values
/// directly into a <see cref="LimitOrderRequest"/> boundary DTO. These verify that Infrastructure's
/// enum-mapping switch expressions throw <see cref="InvalidOperationException"/> on unreachable states.
/// </summary>
public class LunoTradingClientBoundaryTests
{
    private readonly Mock<IRequestAdapter> _requestAdapterMock = new();

    [Fact(DisplayName = "Given an invalid OrderType bypassing domain, When LunoTradingClient maps type, Then throw InvalidOperationException")]
    public async Task PostLimitOrderAsync_InvalidOrderType_ThrowsInvalidOperationException()
    {
        var apiClient = new LunoApiClient(_requestAdapterMock.Object);
        var client = new LunoTradingClient(apiClient, new Mock<ILunoCommandDispatcher>().Object);

        var request = new LimitOrderRequest
        {
            Pair             = "XBTZAR",
            Type             = (OrderType)999,   // bypassing domain validation deliberately
            Volume           = 1m,
            Price            = 1000m,
            BaseAccountId    = 1,
            CounterAccountId = 2,
        };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => client.FetchPostLimitOrderAsync(request));
        Assert.Contains("Unreachable state due to Domain invariants", ex.Message);
        Assert.IsType<ArgumentOutOfRangeException>(ex.InnerException);
        Assert.Contains("Invalid order type", ex.InnerException!.Message);
    }

    [Fact(DisplayName = "Given an invalid TimeInForce bypassing domain, When LunoTradingClient maps type, Then throw InvalidOperationException")]
    public async Task PostLimitOrderAsync_InvalidTimeInForce_ThrowsInvalidOperationException()
    {
        var apiClient = new LunoApiClient(_requestAdapterMock.Object);
        var client = new LunoTradingClient(apiClient, new Mock<ILunoCommandDispatcher>().Object);

        var request = new LimitOrderRequest
        {
            Pair             = "XBTZAR",
            Type             = OrderType.Bid,
            Volume           = 1m,
            Price            = 1000m,
            BaseAccountId    = 1,
            CounterAccountId = 2,
            TimeInForce      = (TimeInForce)999,
        };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => client.FetchPostLimitOrderAsync(request));
        Assert.Contains("Unreachable state due to Domain invariants", ex.Message);
        Assert.IsType<ArgumentOutOfRangeException>(ex.InnerException);
        Assert.Contains("Invalid time in force", ex.InnerException!.Message);
    }

    [Fact(DisplayName = "Given an invalid StopDirection bypassing domain, When LunoTradingClient maps type, Then throw InvalidOperationException")]
    public async Task PostLimitOrderAsync_InvalidStopDirection_ThrowsInvalidOperationException()
    {
        var apiClient = new LunoApiClient(_requestAdapterMock.Object);
        var client = new LunoTradingClient(apiClient, new Mock<ILunoCommandDispatcher>().Object);

        var request = new LimitOrderRequest
        {
            Pair             = "XBTZAR",
            Type             = OrderType.Bid,
            Volume           = 1m,
            Price            = 1000m,
            BaseAccountId    = 1,
            CounterAccountId = 2,
            StopPrice        = 500m,
            StopDirection    = (StopDirection)999,
        };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => client.FetchPostLimitOrderAsync(request));
        Assert.Contains("Unreachable state due to Domain invariants", ex.Message);
        Assert.IsType<ArgumentOutOfRangeException>(ex.InnerException);
        Assert.Contains("Invalid stop direction", ex.InnerException!.Message);
    }
}
