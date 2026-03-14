using System;
using System.Threading.Tasks;
using Microsoft.Kiota.Abstractions;
using Moq;
using Xunit;
using Luno.SDK;
using Luno.SDK.Trading;
using Luno.SDK.Infrastructure.Trading;

namespace Luno.SDK.Tests.Unit.Trading;

public class LunoTradingClientTests
{
    private readonly Mock<IRequestAdapter> _requestAdapterMock;
    private readonly ILunoTradingClient _tradingClient;

    public LunoTradingClientTests()
    {
        _requestAdapterMock = new Mock<IRequestAdapter>();
        _requestAdapterMock.Setup(x => x.BaseUrl).Returns("https://api.luno.com");
        _tradingClient = new LunoTradingClient(_requestAdapterMock.Object);
    }

    [Fact(DisplayName = "Given a request with PostOnly = true and TimeInForce = IOC, When posting limit order, Then throw LunoValidationException.")]
    public async Task PostLimitOrderAsync_PostOnlyWithIOC_ThrowsValidationException()
    {
        // Arrange
        var request = new PostLimitOrderRequest
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
            await _tradingClient.PostLimitOrderAsync(request));

        Assert.Contains("PostOnly cannot be used", ex.Message);
    }

    [Fact(DisplayName = "Given a request with null internal accounts, When posting limit order, Then throw LunoValidationException.")]
    public async Task PostLimitOrderAsync_NullAccounts_ThrowsValidationException()
    {
        // Arrange
        var request = new PostLimitOrderRequest
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
            await _tradingClient.PostLimitOrderAsync(request));

        Assert.Contains("Explicit Account Mandate violated", ex.Message);
    }
}
