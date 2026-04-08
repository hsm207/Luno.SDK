using Microsoft.Kiota.Abstractions;
using Luno.SDK.Infrastructure.Authentication;
using Xunit;

namespace Luno.SDK.Tests.Unit.Infrastructure.Authentication;

public class LunoAuthenticationProviderTests
{
    [Theory(DisplayName = "Private endpoints always authenticate, even if user opts out")]
    [InlineData("GET", "{+baseurl}/api/1/balance{?assets*}")]
    [InlineData("POST", "{+baseurl}/api/1/postorder")]
    [InlineData("GET", "{+baseurl}/api/1/listorders{?closed,created_before,pair}")]
    public async Task PrivateEndpoint_AlwaysAuthenticates_RegardlessOfUserPreference(string method, string urlTemplate)
    {
        var provider = new LunoAuthenticationProvider(new LunoClientOptions { ApiKeyId = "user", ApiKeySecret = "pass" });
        var request = new RequestInformation { HttpMethod = Enum.Parse<Method>(method), UrlTemplate = urlTemplate };
        request.AddRequestOptions(new[] { new LunoAuthenticationOption { AuthenticatePublicEndpoints = false } });

        await provider.AuthenticateRequestAsync(request);

        Assert.True(request.Headers.ContainsKey("Authorization"));
    }

    [Theory(DisplayName = "Public endpoints do not send API keys by default (Least Privilege)")]
    [InlineData("GET", "{+baseurl}/api/1/tickers{?pair}")]
    [InlineData("GET", "{+baseurl}/api/1/orderbook?pair={pair}")]
    [InlineData("GET", "{+baseurl}/api/exchange/1/markets{?pair}")]
    public async Task PublicEndpoint_DoesNotAuthenticate_ByDefault(string method, string urlTemplate)
    {
        var provider = new LunoAuthenticationProvider(new LunoClientOptions { ApiKeyId = "user", ApiKeySecret = "pass" });
        var request = new RequestInformation { HttpMethod = Enum.Parse<Method>(method), UrlTemplate = urlTemplate };

        await provider.AuthenticateRequestAsync(request);

        Assert.False(request.Headers.ContainsKey("Authorization"));
    }

    [Fact(DisplayName = "Public endpoints authenticate when user explicitly opts in (e.g., for higher rate limits)")]
    public async Task PublicEndpoint_Authenticates_WhenUserExplicitlyOptsIn()
    {
        var provider = new LunoAuthenticationProvider(new LunoClientOptions { ApiKeyId = "user", ApiKeySecret = "pass" });
        var request = new RequestInformation { HttpMethod = Method.GET, UrlTemplate = "{+baseurl}/api/1/tickers{?pair}" };
        request.AddRequestOptions(new[] { new LunoAuthenticationOption { AuthenticatePublicEndpoints = true } });

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
        var provider = new LunoAuthenticationProvider(new LunoClientOptions { ApiKeyId = "user", ApiKeySecret = "pass" });
        var request = new RequestInformation { HttpMethod = Method.GET, UrlTemplate = "{+baseurl}/api/1/balance{?assets*}" };
        request.Headers.Add("Authorization", "Bearer external_token");

        await provider.AuthenticateRequestAsync(request);

        Assert.Equal("Bearer external_token", request.Headers["Authorization"].First());
    }
}
