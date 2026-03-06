using Microsoft.Kiota.Abstractions;
using Luno.SDK.Infrastructure.Authentication;
using Xunit;

namespace Luno.SDK.Tests.Unit.Infrastructure.Authentication;

public class LunoAuthenticationProviderTests
{
    [Fact(DisplayName = "Given no credentials and explicit required request, When authenticating, Then throw LunoAuthenticationException")]
    public async Task AuthenticateRequestAsync_GivenNoCredentialsAndExplicitRequired_ThenThrow()
    {
        // Arrange
        var options = new LunoClientOptions();
        var provider = new LunoAuthenticationProvider(options);
        var request = new RequestInformation { URI = new Uri("https://api.luno.com/api/1/public") };
        request.AddRequestOptions(new[] { new LunoAuthenticationOption { RequiresAuthentication = true } });

        // Act & Assert
        await Assert.ThrowsAsync<LunoAuthenticationException>(() => provider.AuthenticateRequestAsync(request));
    }

    [Fact(DisplayName = "Given no credentials and mandatory private endpoint, When authenticating, Then throw LunoAuthenticationException")]
    public async Task AuthenticateRequestAsync_GivenNoCredentialsAndMandatoryPrivate_ThenThrow()
    {
        // Arrange
        var options = new LunoClientOptions();
        var provider = new LunoAuthenticationProvider(options);
        var request = new RequestInformation { URI = new Uri("https://api.luno.com/api/1/balance") };

        // Act & Assert
        await Assert.ThrowsAsync<LunoAuthenticationException>(() => provider.AuthenticateRequestAsync(request));
    }

    [Fact(DisplayName = "Given credentials and explicit required request, When authenticating, Then attach Authorization header")]
    public async Task AuthenticateRequestAsync_GivenCredentialsAndExplicitRequired_ThenAttachHeader()
    {
        // Arrange
        var options = new LunoClientOptions { ApiKeyId = "user", ApiKeySecret = "pass" };
        var provider = new LunoAuthenticationProvider(options);
        var request = new RequestInformation { URI = new Uri("https://api.luno.com/api/1/public") };
        request.AddRequestOptions(new[] { new LunoAuthenticationOption { RequiresAuthentication = true } });

        // Act
        await provider.AuthenticateRequestAsync(request);

        // Assert
        Assert.True(request.Headers.ContainsKey("Authorization"));
        var authHeader = request.Headers["Authorization"].First();
        Assert.Equal("Basic dXNlcjpwYXNz", authHeader);
    }

    [Fact(DisplayName = "Given credentials and explicit opt-out, When authenticating, Then do not attach Authorization header")]
    public async Task AuthenticateRequestAsync_GivenCredentialsAndExplicitOptOut_ThenSkipAuth()
    {
        // Arrange
        var options = new LunoClientOptions { ApiKeyId = "user", ApiKeySecret = "pass" };
        var provider = new LunoAuthenticationProvider(options);
        // Even if it's a mandatory private endpoint, if explicit opt-out is passed, we skip it
        var request = new RequestInformation { URI = new Uri("https://api.luno.com/api/1/balance") };
        request.AddRequestOptions(new[] { new LunoAuthenticationOption { RequiresAuthentication = false } });

        // Act
        await provider.AuthenticateRequestAsync(request);

        // Assert
        Assert.False(request.Headers.ContainsKey("Authorization"));
    }

    [Fact(DisplayName = "Given credentials and public endpoint, When authenticating, Then auto-optimize by attaching Authorization header")]
    public async Task AuthenticateRequestAsync_GivenCredentialsAndPublicEndpoint_ThenAttachHeader()
    {
        // Arrange
        var options = new LunoClientOptions { ApiKeyId = "user", ApiKeySecret = "pass" };
        var provider = new LunoAuthenticationProvider(options);
        var request = new RequestInformation { URI = new Uri("https://api.luno.com/api/1/tickers") }; // No auth option

        // Act
        await provider.AuthenticateRequestAsync(request);

        // Assert
        Assert.True(request.Headers.ContainsKey("Authorization"));
        var authHeader = request.Headers["Authorization"].First();
        Assert.Equal("Basic dXNlcjpwYXNz", authHeader);
    }
}
