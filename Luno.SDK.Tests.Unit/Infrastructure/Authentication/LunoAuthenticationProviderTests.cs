using Microsoft.Kiota.Abstractions;
using Luno.SDK;
using Luno.SDK.Infrastructure.Authentication;
using Xunit;

namespace Luno.SDK.Tests.Unit.Infrastructure.Authentication;

public class LunoAuthenticationProviderTests
{
    [Theory(DisplayName = "Private READ endpoints always authenticate (Safe Harbor)")]
    [InlineData("GET", "{+baseurl}/api/1/balance{?assets*}")]
    [InlineData("GET", "{+baseurl}/api/1/listorders{?closed,created_before,pair}")]
    public async Task PrivateReadEndpoint_AlwaysAuthenticates(string method, string urlTemplate)
    {
        var provider = new LunoAuthenticationProvider(new LunoClientOptions().WithCredentials("user", "pass"));
        var request = new RequestInformation { HttpMethod = Enum.Parse<Method>(method), UrlTemplate = urlTemplate };
        using var scope = LunoSecurityContext.Set(new LunoRequestOptions { AuthenticatePublicEndpoint = false });
        await provider.AuthenticateRequestAsync(request);

        Assert.True(request.Headers.ContainsKey("Authorization"));
    }

    [Theory(DisplayName = "Public endpoints do not send API keys by default (Least Privilege)")]
    [InlineData("GET", "{+baseurl}/api/1/tickers{?pair}")]
    [InlineData("GET", "{+baseurl}/api/1/orderbook?pair={pair}")]
    [InlineData("GET", "{+baseurl}/api/exchange/1/markets{?pair}")]
    public async Task PublicEndpoint_DoesNotAuthenticate_ByDefault(string method, string urlTemplate)
    {
        var provider = new LunoAuthenticationProvider(new LunoClientOptions().WithCredentials("user", "pass"));
        var request = new RequestInformation { HttpMethod = Enum.Parse<Method>(method), UrlTemplate = urlTemplate };

        await provider.AuthenticateRequestAsync(request);

        Assert.False(request.Headers.ContainsKey("Authorization"));
    }

    [Fact(DisplayName = "Public endpoints authenticate when user explicitly opts in (e.g., for higher rate limits)")]
    public async Task PublicEndpoint_Authenticates_WhenUserExplicitlyOptsIn()
    {
        var provider = new LunoAuthenticationProvider(new LunoClientOptions().WithCredentials("user", "pass"));
        var request = new RequestInformation { HttpMethod = Method.GET, UrlTemplate = "{+baseurl}/api/1/tickers{?pair}" };
        using var scope = LunoSecurityContext.Set(new LunoRequestOptions { AuthenticatePublicEndpoint = true });
        await provider.AuthenticateRequestAsync(request);

        Assert.True(request.Headers.ContainsKey("Authorization"));
    }

    [Fact(DisplayName = "Private endpoint without API keys throws LunoAuthenticationException")]
    public async Task PrivateEndpoint_WithoutKeys_Throws()
    {
        var provider = new LunoAuthenticationProvider(new LunoClientOptions());
        var request = new RequestInformation { HttpMethod = Method.GET, UrlTemplate = "{+baseurl}/api/1/balance{?assets*}" };

        await Assert.ThrowsAsync<LunoAuthenticationException>(() => provider.AuthenticateRequestAsync(request));
    }

    [Fact(DisplayName = "Existing Authorization header is never overwritten")]
    public async Task ExistingAuthHeader_IsPreserved()
    {
        var provider = new LunoAuthenticationProvider(new LunoClientOptions().WithCredentials("user", "pass"));
        var request = new RequestInformation { HttpMethod = Method.GET, UrlTemplate = "{+baseurl}/api/1/balance{?assets*}" };
        request.Headers.Add("Authorization", "Bearer external_token");

        await provider.AuthenticateRequestAsync(request);

        Assert.Equal("Bearer external_token", request.Headers["Authorization"].First());
    }

    [Fact(DisplayName = "Write operation without explicit intent throws LunoSecurityException (Implicit Deny)")]
    public async Task WriteOperation_WithoutIntent_Throws()
    {
        var provider = new LunoAuthenticationProvider(new LunoClientOptions().WithCredentials("user", "pass"));
        var request = new RequestInformation { HttpMethod = Method.POST, UrlTemplate = "{+baseurl}/api/1/postorder" };

        var ex = await Assert.ThrowsAsync<LunoSecurityException>(() => provider.AuthenticateRequestAsync(request));
        Assert.Contains("requires 'Perm_W_Orders' permission", ex.Message);
        Assert.Contains("set 'AuthorizeWriteOperation = true'", ex.Message);
    }

    [Fact(DisplayName = "Write operation with explicit intent succeeds")]
    public async Task WriteOperation_WithIntent_Authenticates()
    {
        var provider = new LunoAuthenticationProvider(new LunoClientOptions().WithCredentials("user", "pass"));
        var request = new RequestInformation { HttpMethod = Method.POST, UrlTemplate = "{+baseurl}/api/1/postorder" };
        using var scope = LunoSecurityContext.Set(new LunoRequestOptions { AuthorizeWriteOperation = true });
        await provider.AuthenticateRequestAsync(request);

        Assert.True(request.Headers.ContainsKey("Authorization"));
    }

    [Fact(DisplayName = "Write operation with public-only intent is still blocked (Loophole Shield)")]
    public async Task WriteOperation_WithMisalignedIntent_IsBlocked()
    {
        var provider = new LunoAuthenticationProvider(new LunoClientOptions().WithCredentials("user", "pass"));
        var request = new RequestInformation { HttpMethod = Method.POST, UrlTemplate = "{+baseurl}/api/1/postorder" };
        using var scope = LunoSecurityContext.Set(new LunoRequestOptions { AuthenticatePublicEndpoint = true }); // WRONG FLAG
        await Assert.ThrowsAsync<LunoSecurityException>(() => provider.AuthenticateRequestAsync(request));
    }

    [Fact(DisplayName = "Read operation with unnecessary write intent succeeds (Safe Harbor)")]
    public async Task ReadOperation_WithOverrideIntent_Succeeds()
    {
        var provider = new LunoAuthenticationProvider(new LunoClientOptions().WithCredentials("user", "pass"));
        var request = new RequestInformation { HttpMethod = Method.GET, UrlTemplate = "{+baseurl}/api/1/balance{?assets*}" };
        using var scope = LunoSecurityContext.Set(new LunoRequestOptions { AuthorizeWriteOperation = true }); // UNNECESSARY BUT ALLOWED
        await provider.AuthenticateRequestAsync(request);

        Assert.True(request.Headers.ContainsKey("Authorization"));
    }

    [Fact(DisplayName = "Late Materialization: Credentials are NOT requested if endpoint is public")]
    public async Task LateMaterialization_PublicEndpoint_DoesNotInvokeProvider()
    {
        var trackingProvider = new TrackingCredentialProvider();
        var options = new LunoClientOptions { Credentials = trackingProvider };
        var provider = new LunoAuthenticationProvider(options);
        var request = new RequestInformation { HttpMethod = Method.GET, UrlTemplate = "{+baseurl}/api/1/tickers{?pair}" };

        await provider.AuthenticateRequestAsync(request);

        Assert.Equal(0, trackingProvider.InvocationCount);
    }

    [Fact(DisplayName = "Late Materialization: Credentials are built just-in-time for required endpoints")]
    public async Task LateMaterialization_PrivateEndpoint_InvokesProviderJustInTime()
    {
        var trackingProvider = new TrackingCredentialProvider();
        var options = new LunoClientOptions { Credentials = trackingProvider };
        var provider = new LunoAuthenticationProvider(options);
        var request = new RequestInformation { HttpMethod = Method.GET, UrlTemplate = "{+baseurl}/api/1/balance{?assets*}" };

        Assert.Equal(0, trackingProvider.InvocationCount); // Pre-flight
        await provider.AuthenticateRequestAsync(request);
        Assert.Equal(1, trackingProvider.InvocationCount); // Hit exactly once

        Assert.True(request.Headers.ContainsKey("Authorization"));
    }

    [Fact(DisplayName = "Late Materialization: Credentials are built just-in-time for explicitly opted-in public endpoints")]
    public async Task LateMaterialization_PublicEndpoint_WithExplicitIntent_InvokesProviderJustInTime()
    {
        var trackingProvider = new TrackingCredentialProvider();
        var options = new LunoClientOptions { Credentials = trackingProvider };
        var provider = new LunoAuthenticationProvider(options);
        var request = new RequestInformation { HttpMethod = Method.GET, UrlTemplate = "{+baseurl}/api/1/tickers{?pair}" };

        using var scope = LunoSecurityContext.Set(new LunoRequestOptions { AuthenticatePublicEndpoint = true });

        Assert.Equal(0, trackingProvider.InvocationCount); // Pre-flight
        await provider.AuthenticateRequestAsync(request);
        Assert.Equal(1, trackingProvider.InvocationCount); // Hit exactly once

        Assert.True(request.Headers.ContainsKey("Authorization"));
    }

    private class TrackingCredentialProvider : ILunoCredentialProvider
    {
        public int InvocationCount { get; private set; }
        public ValueTask<LunoCredentials> GetCredentialsAsync(CancellationToken cancellationToken = default)
        {
            InvocationCount++;
            return ValueTask.FromResult(new LunoCredentials("lazy-id", "lazy-secret"));
        }
    }
}
