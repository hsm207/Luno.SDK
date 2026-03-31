using System.Threading.Tasks;
using Moq;
using Xunit;
using Luno.SDK;
using Luno.SDK.Market;
using Luno.SDK.Application.Market;

namespace Luno.SDK.Tests.Unit.Application.Market;

/// <summary>
/// Unit tests for <see cref="GetTickerHandler"/>.
/// </summary>
public class GetTickerHandlerTests
{
    [Theory(DisplayName = "Given an empty or null Pair, When handling query, Then throw LunoValidationException.")]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task HandleAsync_EmptyPair_ThrowsValidationException(string? pair)
    {
        // Arrange
        var marketClientMock = new Mock<ILunoMarketOperations>();
        var handler = new GetTickerHandler(marketClientMock.Object);
        var query = new GetTickerQuery(pair!); // Forcing null for test

        // Act & Assert
        var ex = await Assert.ThrowsAsync<LunoValidationException>(() => handler.HandleAsync(query));
        Assert.Contains("Pair must be provided", ex.Message);
    }
}
