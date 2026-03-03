using Luno.SDK;
using Xunit;

namespace Luno.SDK.Tests.Integration;

public class LunoClientTests
{
    [Fact(DisplayName = "LunoClient should initialize with correct default configuration")]
    public void Constructor_ShouldInitializeWithDefaults()
    {
        // Arrange & Act
        using var client = new LunoClient();

        // Assert
        Assert.NotNull(client.Market);
        var marketClient = client.GetMarketClient();
        Assert.NotNull(marketClient);
    }

    [Fact(DisplayName = "LunoClient should dispose internal resources without throwing exceptions")]
    public void Dispose_ShouldNotThrow()
    {
        // Arrange
        var client = new LunoClient();

        // Act & Assert
        client.Dispose(); // Should not throw
    }
}
