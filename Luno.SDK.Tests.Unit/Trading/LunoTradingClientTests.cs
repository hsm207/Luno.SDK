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
    private readonly Mock<ILunoTradingOperations> _tradingOpsMock;
    private readonly Mock<ILunoClient> _lunoClientMock;
    private readonly ILunoRequestDispatcher _dispatcher;

    public LunoTradingClientTests()
    {
        _tradingClientMock = new Mock<ILunoTradingClient>();
        _tradingOpsMock = new Mock<ILunoTradingOperations>();
        
        // Setup a mocked resolver that returns our mock handlers
        var resolver = new Mock<Func<Type, object?>>();
        resolver.Setup(x => x(typeof(ICommandHandler<PostLimitOrderCommand, OrderResponse>)))
                .Returns(new PostLimitOrderHandler(_tradingOpsMock.Object));
        resolver.Setup(x => x(typeof(ICommandHandler<StopOrderCommand, OrderResponse>)))
                .Returns(new StopOrderHandler(_tradingOpsMock.Object));

        // Instantiate a real dispatcher
        _dispatcher = new LunoRequestDispatcher(resolver.Object);

        // Wire the dispatcher into the mocked sub-client
        _tradingClientMock.Setup(x => x.Requests).Returns(_dispatcher);

        _lunoClientMock = new Mock<ILunoClient>();
        _lunoClientMock.Setup(c => c.Trading).Returns(_tradingClientMock.Object);
    }

    // Note: Happy paths for PostLimitOrder, StopOrder, and ListOrders are covered by LunoTradingClientTests (Integration).
    // Domain-specific validation (e.g. StopPrice/StopDirection, Account Mandate) is covered by PostLimitOrderHandlerTests (Unit).
}
