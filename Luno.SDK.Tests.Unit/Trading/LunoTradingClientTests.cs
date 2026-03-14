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
}
