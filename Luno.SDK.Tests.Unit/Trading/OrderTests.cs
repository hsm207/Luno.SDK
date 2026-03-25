using Xunit;
using Luno.SDK.Trading;
using Luno.SDK;

namespace Luno.SDK.Tests.Unit.Trading;

/// <summary>
/// Unit tests for the <see cref="Order"/> domain record and its invariants.
/// These tests verify that the protected constructor correctly enforces business rules.
/// </summary>
public class OrderTests
{
    private const string OrderId = "BX-123";
    private const OrderSide Side = OrderSide.Buy;
    private const OrderStatus Status = OrderStatus.Awaiting;
    private const string Pair = "XBTZAR";
    private const long CreationTimestamp = 1700000000000;

    [Fact(DisplayName = "Given both Account IDs are null, When constructing a LimitOrder, Then throw LunoValidationException.")]
    public void Constructor_BothAccountIdsNull_ThrowsLunoValidationException()
    {
        // Act & Assert
        var ex = Assert.Throws<LunoValidationException>(() => new LimitOrder(
            orderId: OrderId,
            side: Side,
            status: Status,
            pair: Pair,
            creationTimestamp: CreationTimestamp,
            baseAccountId: null,
            counterAccountId: null, // Both null!
            limitPrice: 1000m,
            limitVolume: 1m
        ));

        Assert.Contains("Domain Invariant Violation", ex.Message);
        Assert.Contains("AccountId", ex.Message);
    }

    [Fact(DisplayName = "Given only BaseAccountId is provided, When constructing a LimitOrder, Then succeed.")]
    public void Constructor_OnlyBaseAccountIdProvided_Succeeds()
    {
        // Act
        var order = new LimitOrder(
            orderId: OrderId,
            side: Side,
            status: Status,
            pair: Pair,
            creationTimestamp: CreationTimestamp,
            baseAccountId: 1,
            counterAccountId: null, // Only BaseAccountId
            limitPrice: 1000m,
            limitVolume: 1m
        );

        // Assert
        Assert.Equal(1, order.BaseAccountId);
        Assert.Null(order.CounterAccountId);
    }

    [Fact(DisplayName = "Given only CounterAccountId is provided, When constructing a LimitOrder, Then succeed.")]
    public void Constructor_OnlyCounterAccountIdProvided_Succeeds()
    {
        // Act
        var order = new LimitOrder(
            orderId: OrderId,
            side: Side,
            status: Status,
            pair: Pair,
            creationTimestamp: CreationTimestamp,
            baseAccountId: null, // Only CounterAccountId
            counterAccountId: 2,
            limitPrice: 1000m,
            limitVolume: 1m
        );

        // Assert
        Assert.Null(order.BaseAccountId);
        Assert.Equal(2, order.CounterAccountId);
    }

    [Fact(DisplayName = "Given a terminal status, When checking IsClosed, Then return true.")]
    public void IsClosed_StatusComplete_ReturnsTrue()
    {
        // Arrange
        var order = new LimitOrder(
            orderId: OrderId,
            side: Side,
            status: OrderStatus.Complete, // Terminal state
            pair: Pair,
            creationTimestamp: CreationTimestamp,
            baseAccountId: 1,
            counterAccountId: 2,
            limitPrice: 1000m,
            limitVolume: 1m
        );

        // Assert
        Assert.True(order.IsClosed);
    }

    [Fact(DisplayName = "Given an active status, When checking IsClosed, Then return false.")]
    public void IsClosed_StatusAwaiting_ReturnsFalse()
    {
        // Arrange
        var order = new LimitOrder(
            orderId: OrderId,
            side: Side,
            status: OrderStatus.Awaiting, // Non-terminal state
            pair: Pair,
            creationTimestamp: CreationTimestamp,
            baseAccountId: 1,
            counterAccountId: 2,
            limitPrice: 1000m,
            limitVolume: 1m
        );

        // Assert
        Assert.False(order.IsClosed);
    }
}
