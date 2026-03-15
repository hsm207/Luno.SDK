using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Luno.SDK;
using Luno.SDK.Trading;
using Luno.SDK.Application.Trading;

namespace Luno.SDK.Tests.Unit.Application;

/// <summary>
/// Unit tests for <see cref="PostLimitOrderHandler"/>.
/// The reconciliation logic is now in the Application layer — these tests exercise it
/// with only a mock <see cref="ILunoTradingClient"/>, with no WireMock or HTTP involved.
/// </summary>
public class PostLimitOrderHandlerTests
{
    private static PostLimitOrderCommand BuildValidCommand(
        string? clientOrderId = "cli-001",
        decimal price = 1000m,
        decimal volume = 1m,
        OrderType type = OrderType.Bid) =>
        new()
        {
            Pair             = "XBTZAR",
            Type             = type,
            Volume           = volume,
            Price            = price,
            BaseAccountId    = 1,
            CounterAccountId = 2,
            ClientOrderId    = clientOrderId,
        };

    private static Order BuildExistingOrder(
        string orderId = "BX-EXISTING",
        string? clientOrderId = "cli-001",
        decimal? limitPrice = 1000m,
        decimal? limitVolume = 1m,
        OrderType? side = OrderType.Bid) =>
        new()
        {
            OrderId       = orderId,
            ClientOrderId = clientOrderId,
            Status        = OrderStatus.Awaiting,
            LimitPrice    = limitPrice,
            LimitVolume   = limitVolume,
            Side          = side,
        };

    // ── Happy path reconciliation ────────────────────────────────────────────────

    [Fact(DisplayName = "Given a 409 with matching parameters, When reconciling, Then return existing OrderId")]
    public async Task HandleAsync_Idempotency_MatchingParams_ReturnsExistingOrderId()
    {
        var tradingClientMock = new Mock<ILunoTradingClient>();
        var handler = new PostLimitOrderHandler(tradingClientMock.Object);
        var command = BuildValidCommand();
        var existingOrder = BuildExistingOrder();

        tradingClientMock
            .Setup(x => x.FetchPostLimitOrderAsync(It.IsAny<LimitOrderRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new LunoIdempotencyException("409"));

        tradingClientMock
            .Setup(x => x.FetchOrderAsync(null, "cli-001", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingOrder);

        var result = await handler.HandleAsync(command);

        Assert.Equal("BX-EXISTING", result.OrderId);
    }

    // ── Price mismatch ───────────────────────────────────────────────────────────

    [Fact(DisplayName = "Given a 409 with a price mismatch, When reconciling, Then throw LunoIdempotencyException")]
    public async Task HandleAsync_Idempotency_PriceMismatch_ThrowsLunoIdempotencyException()
    {
        var tradingClientMock = new Mock<ILunoTradingClient>();
        var handler = new PostLimitOrderHandler(tradingClientMock.Object);
        var command = BuildValidCommand(price: 1000m);
        var existingOrder = BuildExistingOrder(limitPrice: 1500m);

        tradingClientMock
            .Setup(x => x.FetchPostLimitOrderAsync(It.IsAny<LimitOrderRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new LunoIdempotencyException("409"));

        tradingClientMock
            .Setup(x => x.FetchOrderAsync(null, "cli-001", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingOrder);

        var ex = await Assert.ThrowsAsync<LunoIdempotencyException>(() => handler.HandleAsync(command));
        Assert.Contains("Price", ex.Message);
    }

    // ── Volume mismatch ──────────────────────────────────────────────────────────

    [Fact(DisplayName = "Given a 409 with a volume mismatch, When reconciling, Then throw LunoIdempotencyException")]
    public async Task HandleAsync_Idempotency_VolumeMismatch_ThrowsLunoIdempotencyException()
    {
        var tradingClientMock = new Mock<ILunoTradingClient>();
        var handler = new PostLimitOrderHandler(tradingClientMock.Object);
        var command = BuildValidCommand(volume: 1m);
        var existingOrder = BuildExistingOrder(limitVolume: 5m);

        tradingClientMock
            .Setup(x => x.FetchPostLimitOrderAsync(It.IsAny<LimitOrderRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new LunoIdempotencyException("409"));

        tradingClientMock
            .Setup(x => x.FetchOrderAsync(null, "cli-001", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingOrder);

        var ex = await Assert.ThrowsAsync<LunoIdempotencyException>(() => handler.HandleAsync(command));
        Assert.Contains("Volume", ex.Message);
    }

    // ── Side mismatch ────────────────────────────────────────────────────────────

    [Fact(DisplayName = "Given a 409 with a side mismatch, When reconciling, Then throw LunoIdempotencyException")]
    public async Task HandleAsync_Idempotency_SideMismatch_ThrowsLunoIdempotencyException()
    {
        var tradingClientMock = new Mock<ILunoTradingClient>();
        var handler = new PostLimitOrderHandler(tradingClientMock.Object);
        var command = BuildValidCommand(type: OrderType.Bid);
        var existingOrder = BuildExistingOrder(side: OrderType.Ask);   // mismatch!

        tradingClientMock
            .Setup(x => x.FetchPostLimitOrderAsync(It.IsAny<LimitOrderRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new LunoIdempotencyException("409"));

        tradingClientMock
            .Setup(x => x.FetchOrderAsync(null, "cli-001", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingOrder);

        var ex = await Assert.ThrowsAsync<LunoIdempotencyException>(() => handler.HandleAsync(command));
        Assert.Contains("Side", ex.Message);
    }

    // ── No ClientOrderId: 409 propagates ────────────────────────────────────────

    [Fact(DisplayName = "Given a 409 with no ClientOrderId, When posting, Then LunoIdempotencyException propagates unhandled")]
    public async Task HandleAsync_Idempotency_NoClientOrderId_PropagatesException()
    {
        var tradingClientMock = new Mock<ILunoTradingClient>();
        var handler = new PostLimitOrderHandler(tradingClientMock.Object);
        var command = BuildValidCommand(clientOrderId: null);  // no clientOrderId, nothing to reconcile

        tradingClientMock
            .Setup(x => x.FetchPostLimitOrderAsync(It.IsAny<LimitOrderRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new LunoIdempotencyException("409"));

        await Assert.ThrowsAsync<LunoIdempotencyException>(() => handler.HandleAsync(command));

        // Verify we never attempted a lookup when there's nothing to reconcile against
        tradingClientMock.Verify(
            x => x.FetchOrderAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // ── Null fields from API (sparse order) ─────────────────────────────────────

    [Fact(DisplayName = "Given a 409 where existing order has null price/volume/side, When reconciling, Then return existing OrderId (no comparison performed)")]
    public async Task HandleAsync_Idempotency_SparseExistingOrder_ReturnsExistingOrderId()
    {
        var tradingClientMock = new Mock<ILunoTradingClient>();
        var handler = new PostLimitOrderHandler(tradingClientMock.Object);
        var command = BuildValidCommand();
        // Simulate an order returned without optional fields (e.g. older API response)
        var existingOrder = BuildExistingOrder(limitPrice: null, limitVolume: null, side: null);

        tradingClientMock
            .Setup(x => x.FetchPostLimitOrderAsync(It.IsAny<LimitOrderRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new LunoIdempotencyException("409"));

        tradingClientMock
            .Setup(x => x.FetchOrderAsync(null, "cli-001", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingOrder);

        var result = await handler.HandleAsync(command);
        Assert.Equal("BX-EXISTING", result.OrderId);
    }
}
