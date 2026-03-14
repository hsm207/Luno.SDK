using System;
using System.Threading.Tasks;
using Luno.SDK;
using Luno.SDK.Trading;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using Xunit;

namespace Luno.SDK.Tests.Integration.Trading;

public class LunoTradingClientTests : IDisposable
{
    private readonly WireMockServer _server;

    public LunoTradingClientTests()
    {
        _server = WireMockServer.Start();
    }

    public void Dispose()
    {
        _server.Stop();
        _server.Dispose();
    }

    private ILunoClient CreateClient()
    {
        var options = new LunoClientOptions 
        { 
            BaseUrl = _server.Url!,
            ApiKeyId = "dummy_key",
            ApiKeySecret = "dummy_secret"
        };
        return new LunoClient(options);
    }

    [Fact(DisplayName = "Given an Idempotent Request, When API returns 409 Conflict, Then SDK reconciles the duplicate order ID.")]
    public async Task PostLimitOrderAsync_IdempotencyReconciliation_ReturnsExistingOrder()
    {
        // Arrange
        var clientId = "unique-uuid-123";
        var expectedOrderId = "BX123";

        // 1. Post Limit Order returns 409 Conflict (ErrDuplicateClientOrderID)
        _server.Given(Request.Create().WithPath("/api/1/postorder").UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(409)
                .WithHeader("Content-Type", "application/json")
                .WithBodyAsJson(new { error = "Duplicate client order ID", code = "ErrDuplicateClientOrderID" }));

        // 2. Reconciliation lookup (GET /api/exchange/3/order) returns existing order matching params
        _server.Given(Request.Create().WithPath("/api/exchange/3/order").WithParam("client_order_id", clientId).UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBodyAsJson(new
                {
                    order_id = expectedOrderId,
                    client_order_id = clientId,
                    limit_price = "250000",
                    limit_volume = "0.001",
                    side = "BUY",
                    pair = "XBTMYR"
                }));

        var client = CreateClient();
        var request = new PostLimitOrderRequest
        {
            Pair = "XBTMYR",
            Type = OrderType.Bid, // Expected "BUY"
            Volume = 0.001m,
            Price = 250000m,
            BaseAccountId = 1,
            CounterAccountId = 2,
            ClientOrderId = clientId
        };

        // Act
        var response = await client.Trading.PostLimitOrderAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(expectedOrderId, response.OrderId);
    }

    [Fact(DisplayName = "Given an order ID, When stopping order by ClientOrderId, Then SDK looks up exchange ID and stops.")]
    public async Task StopOrderByClientOrderIdAsync_ValidId_PerformsLookupAndStops()
    {
        // Arrange
        var clientId = "unique-uuid-123";
        var expectedOrderId = "BX123";

        // 1. Reconciliation lookup
        _server.Given(Request.Create().WithPath("/api/exchange/3/order").WithParam("client_order_id", clientId).UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBodyAsJson(new { order_id = expectedOrderId, client_order_id = clientId }));

        // 2. Stop order
        _server.Given(Request.Create().WithPath("/api/1/stoporder").UsingPost().WithParam("order_id", expectedOrderId))
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBodyAsJson(new { success = true }));

        var client = CreateClient();

        // Act
        var result = await client.Trading.StopOrderByClientOrderIdAsync(clientId);

        // Assert
        Assert.True(result);
    }

    [Fact(DisplayName = "Given API returns a specific trading error, When posting, Then specific domain exception is thrown.")]
    public async Task PostLimitOrderAsync_HighFidelityErrorMapping_ThrowsSpecificException()
    {
        // Arrange
        _server.Given(Request.Create().WithPath("/api/1/postorder").UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(403) // Forbidden / Insufficient Perms
                .WithHeader("Content-Type", "application/json")
                .WithBodyAsJson(new { error = "Forbidden", code = "ErrInsufficientPerms" }));

        var client = CreateClient();
        var request = new PostLimitOrderRequest
        {
            Pair = "XBTMYR",
            Type = OrderType.Bid,
            Volume = 0.001m,
            Price = 250000m,
            BaseAccountId = 1,
            CounterAccountId = 2,
            ClientOrderId = "uuid"
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<LunoForbiddenException>(async () =>
            await client.Trading.PostLimitOrderAsync(request));

        Assert.Equal(403, ex.StatusCode);
    }
}
