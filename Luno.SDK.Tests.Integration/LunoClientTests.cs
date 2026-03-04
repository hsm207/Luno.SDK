using Luno.SDK;
using Xunit;

namespace Luno.SDK.Tests.Integration;

public class LunoClientTests
{
    [Fact(DisplayName = "LunoClient should initialize with correct configuration")]
    public void Constructor_ShouldInitialize()
    {
        // Arrange
        using var httpClient = new HttpClient();

        // Act
        var client = new LunoClient(httpClient);

        // Assert
        Assert.NotNull(client.Market);
        var marketClient = client.GetMarketClient();
        Assert.NotNull(marketClient);
    }
}
