using Microsoft.Extensions.DependencyInjection;
using Luno.SDK;
using Luno.SDK.Infrastructure.Market;
using Xunit;

namespace Luno.SDK.Tests.Integration;

public class DependencyInjectionTests
{
    [Fact(DisplayName = "AddLunoClient should register ILunoClient and resolve all required services")]
    public void AddLunoClient_ShouldRegisterServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddLunoClient();
        var provider = services.BuildServiceProvider();
        var client = provider.GetService<ILunoClient>();

        // Assert
        Assert.NotNull(client);
        Assert.IsType<LunoClient>(client);
        
        Assert.NotNull(client.Market);
        Assert.NotNull(client.GetMarketClient());
    }
}
