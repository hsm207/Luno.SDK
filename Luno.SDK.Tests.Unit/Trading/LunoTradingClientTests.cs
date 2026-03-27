using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Luno.SDK;
using Luno.SDK.Trading;
using Luno.SDK.Application;
using Luno.SDK.Application.Trading;

namespace Luno.SDK.Tests.Unit.Trading;

/// <summary>
/// Unit tests for <see cref="ILunoTradingClient"/> command orchestration.
/// Redundant happy-path and validation tests have been removed as they are covered by Tier 2 Integration tests.
/// </summary>
public class LunoTradingClientTests
{
    private readonly Mock<ILunoTradingClient> _tradingClientMock;
    private readonly Mock<ILunoClient> _lunoClientMock;
    private readonly ILunoCommandDispatcher _dispatcher;

    public LunoTradingClientTests()
    {
        _tradingClientMock = new Mock<ILunoTradingClient>();
        
        // Setup a mocked resolver that returns our mock handlers
        var resolver = new Mock<Func<Type, object?>>();
        resolver.Setup(x => x(typeof(ICommandHandler<PostLimitOrderCommand, Task<OrderResponse>>)))
                .Returns(new PostLimitOrderHandler(_tradingClientMock.Object));
        resolver.Setup(x => x(typeof(ICommandHandler<StopOrderCommand, Task<OrderResponse>>)))
                .Returns(new StopOrderHandler(_tradingClientMock.Object));

        // Instantiate a real dispatcher
        _dispatcher = new LunoCommandDispatcher(resolver.Object);

        // Wire the dispatcher into the mocked sub-client
        _tradingClientMock.Setup(x => x.Commands).Returns(_dispatcher);

        _lunoClientMock = new Mock<ILunoClient>();
        _lunoClientMock.Setup(c => c.Trading).Returns(_tradingClientMock.Object);
    }

    // Note: Happy paths for PostLimitOrder, StopOrder, and ListOrders are covered by LunoTradingClientTests (Integration).
    // Domain-specific validation (e.g. StopPrice/StopDirection, Account Mandate) is covered by PostLimitOrderHandlerTests (Unit).
}
