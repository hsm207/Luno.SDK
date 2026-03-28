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
/// Unique branching logic (e.g. Volume/Pair/Type mismatches during reconciliation)
/// is preserved here as Tier 1 logic tests.
/// Happy paths and simple Price/Side mismatches are covered by Tier 2 Integration tests.
/// </summary>
public class PostLimitOrderHandlerTests
{
    private static PostLimitOrderCommand BuildValidCommand(
        string? clientOrderId = "cli-001",
        decimal price = 1000m,
        decimal volume = 1m,
        OrderSide side = OrderSide.Buy,
        TimeInForce tif = TimeInForce.GTC) =>
        new()
        {
            Pair             = "XBTZAR",
            Side             = side,
            Volume           = volume,
            Price            = price,
            BaseAccountId    = 1,
            CounterAccountId = 2,
            ClientOrderId    = clientOrderId,
            TimeInForce      = tif
        };

    private static LimitOrder BuildExistingOrder(
        string orderId = "BX-EXISTING",
        string? clientOrderId = "cli-001",
        decimal limitPrice = 1000m,
        decimal limitVolume = 1m,
        OrderSide side = OrderSide.Buy,
        TimeInForce timeInForce = TimeInForce.GTC,
        string pair = "XBTZAR") =>
        new(
            orderId: orderId,
            side: side,
            status: OrderStatus.Awaiting,
            pair: pair,
            creationTimestamp: 1700000000000,
            baseAccountId: 1,
            counterAccountId: 2,
            limitPrice: limitPrice,
            limitVolume: limitVolume,
            timeInForce: timeInForce,
            clientOrderId: clientOrderId);

    private static MarketOrder BuildExistingMarketOrder(
        string orderId = "BX-MARKET",
        string? clientOrderId = "cli-001",
        OrderSide side = OrderSide.Buy) =>
        new(
            orderId: orderId,
            side: side,
            status: OrderStatus.Awaiting,
            pair: "XBTZAR",
            creationTimestamp: 1700000000000,
            baseAccountId: 1,
            counterAccountId: 2,
            clientOrderId: clientOrderId);

    // ── Unique Branching: Type mismatch ──────────────────────────────────────────

    [Fact(DisplayName = "Given a 409 where existing order is a MarketOrder, When reconciling, Then throw LunoIdempotencyException")]
    public async Task HandleAsync_Idempotency_TypeMismatch_ThrowsLunoIdempotencyException()
    {
        var tradingClientMock = new Mock<ILunoTradingOperations>();
        var handler = new PostLimitOrderHandler(tradingClientMock.Object);
        var command = BuildValidCommand();
        var existingOrder = BuildExistingMarketOrder();

        tradingClientMock
            .Setup(x => x.FetchPostLimitOrderAsync(It.IsAny<LimitOrderRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new LunoIdempotencyException("409"));

        tradingClientMock
            .Setup(x => x.FetchOrderAsync(null, "cli-001", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingOrder);

        var ex = await Assert.ThrowsAsync<LunoIdempotencyException>(() => handler.HandleAsync(command));
        Assert.Contains("Type", ex.Message);
    }

    // ── Unique Branching: Volume mismatch ────────────────────────────────────────

    [Fact(DisplayName = "Given a 409 with a volume mismatch, When reconciling, Then throw LunoIdempotencyException")]
    public async Task HandleAsync_Idempotency_VolumeMismatch_ThrowsLunoIdempotencyException()
    {
        var tradingClientMock = new Mock<ILunoTradingOperations>();
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

    // ── Unique Branching: TimeInForce mismatch ───────────────────────────────────

    [Fact(DisplayName = "Given a 409 with a TimeInForce mismatch, When reconciling, Then throw LunoIdempotencyException")]
    public async Task HandleAsync_Idempotency_TimeInForceMismatch_ThrowsLunoIdempotencyException()
    {
        var tradingClientMock = new Mock<ILunoTradingOperations>();
        var handler = new PostLimitOrderHandler(tradingClientMock.Object);
        var command = BuildValidCommand(tif: TimeInForce.FOK);
        var existingOrder = BuildExistingOrder(timeInForce: TimeInForce.GTC);

        tradingClientMock
            .Setup(x => x.FetchPostLimitOrderAsync(It.IsAny<LimitOrderRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new LunoIdempotencyException("409"));

        tradingClientMock
            .Setup(x => x.FetchOrderAsync(null, "cli-001", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingOrder);

        var ex = await Assert.ThrowsAsync<LunoIdempotencyException>(() => handler.HandleAsync(command));
        Assert.Contains("TimeInForce", ex.Message);
    }

    // ── Unique Branching: Pair mismatch ──────────────────────────────────────────

    [Fact(DisplayName = "Given a 409 with a Pair mismatch, When reconciling, Then throw LunoIdempotencyException")]
    public async Task HandleAsync_Idempotency_PairMismatch_ThrowsLunoIdempotencyException()
    {
        var tradingClientMock = new Mock<ILunoTradingOperations>();
        var handler = new PostLimitOrderHandler(tradingClientMock.Object);
        var command = BuildValidCommand();
        var existingOrder = BuildExistingOrder(pair: "ETHMYR"); // Explicitly set mismatch pair

        tradingClientMock
            .Setup(x => x.FetchPostLimitOrderAsync(It.IsAny<LimitOrderRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new LunoIdempotencyException("409"));

        tradingClientMock
            .Setup(x => x.FetchOrderAsync(null, "cli-001", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingOrder);

        var ex = await Assert.ThrowsAsync<LunoIdempotencyException>(() => handler.HandleAsync(command));
        Assert.Contains("Pair", ex.Message);
    }

    // ── Unique Branching: BaseAccountId mismatch ────────────────────────────────

    [Fact(DisplayName = "Given a 409 with a BaseAccountId mismatch, When reconciling, Then throw LunoIdempotencyException")]
    public async Task HandleAsync_Idempotency_BaseAccountIdMismatch_ThrowsLunoIdempotencyException()
    {
        var tradingClientMock = new Mock<ILunoTradingOperations>();
        var handler = new PostLimitOrderHandler(tradingClientMock.Object);
        var command = BuildValidCommand();
        // Use helper to create a mismatching order
        var mismatchOrder = new LimitOrder(
            orderId: "BX-EXISTING",
            side: OrderSide.Buy,
            status: OrderStatus.Awaiting,
            pair: "XBTZAR",
            creationTimestamp: 1700000000000,
            baseAccountId: 999, // Mismatch!
            counterAccountId: 2,
            limitPrice: 1000m,
            limitVolume: 1m,
            timeInForce: TimeInForce.GTC,
            clientOrderId: "cli-001");

        tradingClientMock
            .Setup(x => x.FetchPostLimitOrderAsync(It.IsAny<LimitOrderRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new LunoIdempotencyException("409"));

        tradingClientMock
            .Setup(x => x.FetchOrderAsync(null, "cli-001", It.IsAny<CancellationToken>()))
            .ReturnsAsync(mismatchOrder);

        var ex = await Assert.ThrowsAsync<LunoIdempotencyException>(() => handler.HandleAsync(command));
        Assert.Contains("BaseAccountId", ex.Message);
    }

    // ── Unique Branching: CounterAccountId mismatch ─────────────────────────────

    [Fact(DisplayName = "Given a 409 with a CounterAccountId mismatch, When reconciling, Then throw LunoIdempotencyException")]
    public async Task HandleAsync_Idempotency_CounterAccountIdMismatch_ThrowsLunoIdempotencyException()
    {
        var tradingClientMock = new Mock<ILunoTradingOperations>();
        var handler = new PostLimitOrderHandler(tradingClientMock.Object);
        var command = BuildValidCommand();
        var mismatchOrder = new LimitOrder(
            orderId: "BX-EXISTING",
            side: OrderSide.Buy,
            status: OrderStatus.Awaiting,
            pair: "XBTZAR",
            creationTimestamp: 1700000000000,
            baseAccountId: 1,
            counterAccountId: 999, // Mismatch!
            limitPrice: 1000m,
            limitVolume: 1m,
            timeInForce: TimeInForce.GTC,
            clientOrderId: "cli-001");

        tradingClientMock
            .Setup(x => x.FetchPostLimitOrderAsync(It.IsAny<LimitOrderRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new LunoIdempotencyException("409"));

        tradingClientMock
            .Setup(x => x.FetchOrderAsync(null, "cli-001", It.IsAny<CancellationToken>()))
            .ReturnsAsync(mismatchOrder);

        var ex = await Assert.ThrowsAsync<LunoIdempotencyException>(() => handler.HandleAsync(command));
        Assert.Contains("CounterAccountId", ex.Message);
    }

    // ── Business Rule Validation ────────────────────────────────────────────────

    [Fact(DisplayName = "Given PostOnly is true and TIF is not GTC, When handling, Then throw LunoValidationException")]
    public async Task HandleAsync_Validation_PostOnlyWithNonGtc_ThrowsLunoValidationException()
    {
        var tradingClientMock = new Mock<ILunoTradingOperations>();
        var handler = new PostLimitOrderHandler(tradingClientMock.Object);
        var command = BuildValidCommand(tif: TimeInForce.IOC) with { PostOnly = true };

        var ex = await Assert.ThrowsAsync<LunoValidationException>(() => handler.HandleAsync(command));
        Assert.Contains("PostOnly cannot be used with a TimeInForce other than GTC", ex.Message);
    }

    [Theory(DisplayName = "Given missing or invalid account IDs, When handling, Then throw LunoValidationException")]
    [InlineData(null, 2L)]
    [InlineData(1L, null)]
    [InlineData(null, null)]
    [InlineData(0L, 2L)]
    [InlineData(1L, 0L)]
    public async Task HandleAsync_Validation_ExplicitAccountMandate_ThrowsLunoValidationException(long? baseId, long? counterId)
    {
        var tradingClientMock = new Mock<ILunoTradingOperations>();
        var handler = new PostLimitOrderHandler(tradingClientMock.Object);
        var command = BuildValidCommand() with { BaseAccountId = baseId, CounterAccountId = counterId };

        var ex = await Assert.ThrowsAsync<LunoValidationException>(() => handler.HandleAsync(command));
        Assert.Contains("Explicit Account Mandate violated", ex.Message);
    }

    // ── Enum Validation ─────────────────────────────────────────────────────────

    [Fact(DisplayName = "Given an invalid OrderSide cast, When handling, Then throw LunoValidationException")]
    public async Task HandleAsync_Validation_InvalidOrderSide_ThrowsLunoValidationException()
    {
        var tradingClientMock = new Mock<ILunoTradingOperations>();
        var handler = new PostLimitOrderHandler(tradingClientMock.Object);
        var command = BuildValidCommand() with { Side = (OrderSide)999 };

        var ex = await Assert.ThrowsAsync<LunoValidationException>(() => handler.HandleAsync(command));
        Assert.Contains("Invalid OrderSide", ex.Message);
    }

    [Fact(DisplayName = "Given an invalid TimeInForce cast, When handling, Then throw LunoValidationException")]
    public async Task HandleAsync_Validation_InvalidTimeInForce_ThrowsLunoValidationException()
    {
        var tradingClientMock = new Mock<ILunoTradingOperations>();
        var handler = new PostLimitOrderHandler(tradingClientMock.Object);
        var command = BuildValidCommand() with { TimeInForce = (TimeInForce)999 };

        var ex = await Assert.ThrowsAsync<LunoValidationException>(() => handler.HandleAsync(command));
        Assert.Contains("Invalid TimeInForce", ex.Message);
    }

    [Fact(DisplayName = "Given an invalid StopDirection cast, When handling, Then throw LunoValidationException")]
    public async Task HandleAsync_Validation_InvalidStopDirection_ThrowsLunoValidationException()
    {
        var tradingClientMock = new Mock<ILunoTradingOperations>();
        var handler = new PostLimitOrderHandler(tradingClientMock.Object);
        var command = BuildValidCommand() with { StopPrice = 1000m, StopDirection = (StopDirection)999 };

        var ex = await Assert.ThrowsAsync<LunoValidationException>(() => handler.HandleAsync(command));
        Assert.Contains("Invalid StopDirection", ex.Message);
    }
}
