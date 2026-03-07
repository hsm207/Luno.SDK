using System;
using System.Threading.Tasks;
using Luno.SDK;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using Xunit;

namespace Luno.SDK.Tests.Integration;

public class LunoAccountClientTests : IDisposable
{
    private readonly WireMockServer _server;

    public LunoAccountClientTests()
    {
        _server = WireMockServer.Start();
    }

    public void Dispose()
    {
        _server.Stop();
        _server.Dispose();
    }

    private LunoClient CreateClient(string? apiKeyId = null, string? apiKeySecret = null)
    {
        var options = new LunoClientOptions
        {
            BaseUrl = _server.Url!,
            ApiKeyId = apiKeyId,
            ApiKeySecret = apiKeySecret
        };
        return new LunoClient(options);
    }

    [Fact(DisplayName = "Given successful response, When getting balances, Then deserialize with exact decimal precision")]
    public async Task GetBalancesAsync_GivenSuccess_WhenDeserializing_ThenEnsureDecimalPrecision()
    {
        // Arrange
        _server.Given(Request.Create().WithPath("/api/1/balance").UsingGet().WithHeader("Authorization", "Basic dXNlcjpwYXNz"))
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBodyAsJson(new
                {
                    balance = new[]
                    {
                        new
                        {
                            account_id = "123",
                            asset = "XBT",
                            balance = "1.12345678",
                            reserved = "0.00000001",
                            unconfirmed = "0.1",
                            name = "Crypto Wallet"
                        }
                    }
                }));

        var client = CreateClient("user", "pass");

        // Act
        var balances = await client.GetBalancesAsync(); // Uses Application Layer extension!

        // Assert
        Assert.NotNull(balances);
        Assert.Single(balances);
        var b = balances[0];
        Assert.Equal("123", b.AccountId);
        Assert.Equal("XBT", b.Asset);
        Assert.Equal("Crypto Wallet", b.Name);

        // Exact decimal precision assertions
        Assert.Equal(1.12345678m, b.Available);
        Assert.Equal(0.00000001m, b.Reserved);
        Assert.Equal(0.1m, b.Unconfirmed);
        Assert.Equal(1.12345679m, b.Total); // 1.12345678 + 0.00000001
    }

    [Fact(DisplayName = "Given 401 response, When getting balances, Then translate to LunoUnauthorizedException")]
    public async Task GetBalancesAsync_Given401_WhenRequested_ThenThrowLunoUnauthorizedException()
    {
        // Arrange
        _server.Given(Request.Create().WithPath("/api/1/balance").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(401)
                .WithHeader("Content-Type", "application/json")
                .WithBodyAsJson(new { error = "Invalid API credentials" }));

        var client = CreateClient("wrong", "keys");

        // Act & Assert
        await Assert.ThrowsAsync<LunoUnauthorizedException>(() => client.Accounts.GetBalancesAsync());
    }

    [Fact(DisplayName = "Given 403 response, When getting balances, Then translate to LunoForbiddenException")]
    public async Task GetBalancesAsync_Given403_WhenRequested_ThenThrowLunoForbiddenException()
    {
        // Arrange
        _server.Given(Request.Create().WithPath("/api/1/balance").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(403)
                .WithHeader("Content-Type", "application/json")
                .WithBodyAsJson(new { error = "Forbidden resource" }));

        var client = CreateClient("user", "pass");

        // Act & Assert
        await Assert.ThrowsAsync<LunoForbiddenException>(() => client.Accounts.GetBalancesAsync());
    }

    [Fact(DisplayName = "Given no credentials, When getting balances, Then fail fast with LunoAuthenticationException")]
    public async Task GetBalancesAsync_GivenNoCredentials_WhenRequested_ThenFailFast()
    {
        // Arrange
        var client = CreateClient(); // No keys provided

        // Act & Assert
        await Assert.ThrowsAsync<LunoAuthenticationException>(() => client.Accounts.GetBalancesAsync());
    }
}
