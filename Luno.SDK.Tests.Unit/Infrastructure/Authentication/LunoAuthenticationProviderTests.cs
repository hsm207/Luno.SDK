using Microsoft.Kiota.Abstractions;
using Luno.SDK.Infrastructure.Authentication;
using Xunit;

namespace Luno.SDK.Tests.Unit.Infrastructure.Authentication;

public class LunoAuthenticationProviderTests
{
    [Fact(DisplayName = "Given no credentials, When authenticating a required request, Then throw LunoAuthenticationException")]
    public async Task AuthenticateRequestAsync_GivenNoCredentials_WhenRequired_ThenThrow()
    {
        // Arrange
        var options = new LunoClientOptions(); // No ApiKeyId or ApiKeySecret
        var provider = new LunoAuthenticationProvider(options);
        var request = new RequestInformation();
        request.AddRequestOptions(new[] { new LunoAuthenticationOption { RequiresAuthentication = true } });

        // Act & Assert
        await Assert.ThrowsAsync<LunoAuthenticationException>(() => provider.AuthenticateRequestAsync(request));
    }

    [Fact(DisplayName = "Given credentials, When authenticating a required request, Then attach Authorization header")]
    public async Task AuthenticateRequestAsync_GivenCredentials_WhenRequired_ThenAttachHeader()
    {
        // Arrange
        var options = new LunoClientOptions { ApiKeyId = "user", ApiKeySecret = "pass" };
        var provider = new LunoAuthenticationProvider(options);
        var request = new RequestInformation();
        request.AddRequestOptions(new[] { new LunoAuthenticationOption { RequiresAuthentication = true } });

        // Act
        await provider.AuthenticateRequestAsync(request);

        // Assert
        Assert.True(request.Headers.ContainsKey("Authorization"));
        var authHeader = request.Headers["Authorization"].First();
        Assert.Equal("Basic dXNlcjpwYXNz", authHeader); // "user:pass" in Base64
    }

    [Fact(DisplayName = "Given credentials, When authenticating a public request, Then attach Authorization header")]
    public async Task AuthenticateRequestAsync_GivenCredentials_WhenNotRequired_ThenAttachHeader()
    {
        // Arrange
        var options = new LunoClientOptions { ApiKeyId = "user", ApiKeySecret = "pass" };
        var provider = new LunoAuthenticationProvider(options);
        var request = new RequestInformation(); // No auth option added

        // Act
        await provider.AuthenticateRequestAsync(request);

        // Assert
        Assert.True(request.Headers.ContainsKey("Authorization"));
        var authHeader = request.Headers["Authorization"].First();
        Assert.Equal("Basic dXNlcjpwYXNz", authHeader);
    }
}
