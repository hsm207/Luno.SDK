using Luno.SDK;
using Xunit;

namespace Luno.SDK.Tests.Integration;

public class LunoClientTests
{
    [Fact(DisplayName = "LunoClient should initialize with correct configuration using standalone defaults")]
    public void Constructor_ShouldInitializeWithDefaults()
    {
        // Arrange & Act
        var client = new LunoClient();

        // Assert
        Assert.NotNull(client.Market);
        var marketClient = client.GetMarketClient();
        Assert.NotNull(marketClient);
    }

    [Fact(DisplayName = "LunoClient should initialize with correct configuration using an injected HttpClient")]
    public void Constructor_ShouldInitializeWithInjectedClient()
    {
        // Arrange
        using var httpClient = new HttpClient();

        // Act
        var client = new LunoClient(httpClient);

        // Assert
        Assert.NotNull(client.Market);
    }
}
