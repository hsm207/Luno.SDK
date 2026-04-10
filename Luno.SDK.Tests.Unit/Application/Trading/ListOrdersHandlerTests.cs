using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Luno.SDK;
using Luno.SDK.Trading;
using Luno.SDK.Application.Trading;

namespace Luno.SDK.Tests.Unit.Application.Trading;

/// <summary>
/// Unit tests for <see cref="ListOrdersHandler"/>.
/// </summary>
public class ListOrdersHandlerTests
{
    [Theory(DisplayName = "Given an invalid Limit, When handling query, Then throw LunoValidationException.")]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(1001)]
    public async Task HandleAsync_InvalidLimit_ThrowsValidationException(long limit)
    {
        // Arrange
        var tradingClientMock = new Mock<ILunoTradingOperations>();
        var handler = new ListOrdersHandler(tradingClientMock.Object);
        var query = new ListOrdersQuery(Limit: limit);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<LunoValidationException>(() => handler.HandleAsync(query));
        Assert.Contains("Limit must be between 1 and 1000", ex.Message);
    }

    [Theory(DisplayName = "Given a valid Limit, When handling query, Then passes validation and calls client.")]
    [InlineData(null)]
    [InlineData(1L)]
    [InlineData(500L)]
    [InlineData(1000L)]
    public async Task HandleAsync_ValidLimit_CallsClient(long? limit)
    {
        // Arrange
        var tradingClientMock = new Mock<ILunoTradingOperations>();
        tradingClientMock
            .Setup(x => x.FetchListOrdersAsync(It.IsAny<OrderStatus?>(), It.IsAny<string?>(), It.IsAny<long?>(), limit, default))
            .ReturnsAsync(new List<Order>());

        var handler = new ListOrdersHandler(tradingClientMock.Object);
        var query = new ListOrdersQuery(Limit: limit);

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        Assert.NotNull(result);
        tradingClientMock.Verify(
            x => x.FetchListOrdersAsync(It.IsAny<OrderStatus?>(), It.IsAny<string?>(), It.IsAny<long?>(), limit, default),
            Times.Once);
    }
}
