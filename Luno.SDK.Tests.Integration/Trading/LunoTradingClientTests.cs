using System;
using System.Threading.Tasks;
using Luno.SDK;
using Luno.SDK.Trading;
using Luno.SDK.Application.Trading;
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
        var command = new PostLimitOrderCommand
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
        var response = await client.PostLimitOrderAsync(command);

        // Assert
        Assert.NotNull(response.OrderId);
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
        var result = await client.StopOrderByClientOrderIdAsync(clientId);

        // Assert
        Assert.NotNull(result.OrderId);
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
        var command = new PostLimitOrderCommand
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
            await client.PostLimitOrderAsync(command));

        Assert.Equal(403, ex.StatusCode);
    }

    [Fact(DisplayName = "Given valid parameters, When posting limit order (Bid/GTC), Then returns OrderReference successfully.")]
    public async Task PostLimitOrderAsync_HappyPath_ReturnsOrderReference()
    {
        // Arrange
        var expectedOrderId = "BX123_SUCCESS";
        _server.Given(Request.Create().WithPath("/api/1/postorder").UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBodyAsJson(new { order_id = expectedOrderId }));

        var client = CreateClient();
        var command = new PostLimitOrderCommand
        {
            Pair = "XBTMYR", Type = OrderType.Bid, Volume = 0.001m, Price = 250000m, BaseAccountId = 1, CounterAccountId = 2, TimeInForce = TimeInForce.GTC
        };

        // Act
        var result = await client.PostLimitOrderAsync(command);

        // Assert
        Assert.Equal(expectedOrderId, result.OrderId);
    }

    [Theory(DisplayName = "Given valid parameters with varying enums, When posting limit order, Then returns successfully.")]
    [InlineData(OrderType.Ask, TimeInForce.IOC, null)]
    [InlineData(OrderType.Bid, TimeInForce.FOK, null)]
    [InlineData(OrderType.Bid, TimeInForce.GTC, StopDirection.Above)]
    [InlineData(OrderType.Bid, TimeInForce.GTC, StopDirection.Below)]
    [InlineData(OrderType.Ask, TimeInForce.GTC, StopDirection.RelativeLastTrade)]
    public async Task PostLimitOrderAsync_EnumVariations_ReturnsOrderReference(OrderType type, TimeInForce tif, StopDirection? stopDir)
    {
        // Arrange
        var expectedOrderId = "BX123_ENUMS";
        _server.Given(Request.Create().WithPath("/api/1/postorder").UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBodyAsJson(new { order_id = expectedOrderId }));

        var client = CreateClient();
        var command = new PostLimitOrderCommand
        {
            Pair = "XBTMYR", Type = type, Volume = 0.001m, Price = 250000m, BaseAccountId = 1, CounterAccountId = 2, TimeInForce = tif, StopPrice = stopDir.HasValue ? 100m : null, StopDirection = stopDir
        };

        // Act
        var result = await client.PostLimitOrderAsync(command);

        // Assert
        Assert.Equal(expectedOrderId, result.OrderId);
    }

    [Fact(DisplayName = "Given a valid OrderId, When stopping order, Then SDK calls delete endpoint directly.")]
    public async Task StopOrderAsync_ValidOrderId_StopsSuccessfully()
    {
        // Arrange
        var orderId = "BX123";
        _server.Given(Request.Create().WithPath("/api/1/stoporder").UsingPost().WithParam("order_id", orderId))
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBodyAsJson(new { success = true }));

        var client = CreateClient();

        // Act
        var result = await client.StopOrderAsync(new StopOrderCommand { OrderId = orderId });

        // Assert
        Assert.Equal(orderId, result.OrderId);
    }

    [Fact(DisplayName = "Given a ClientOrderId of a COMPLETE order, When stopping, Then SDK returns successfully without calling stop endpoint.")]
    public async Task StopOrderAsync_AlreadyComplete_ReturnsImmediately()
    {
        // Arrange
        var clientOrderId = "CL123";
        var orderId = "BX123";
        
        // Mock the GET order call to return status COMPLETE
        _server.Given(Request.Create()
                .WithPath("/api/exchange/3/order")
                .WithParam("client_order_id", clientOrderId)
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBodyAsJson(new 
                { 
                    order_id = orderId, 
                    status = "COMPLETE", 
                    client_order_id = clientOrderId,
                    side = "BUY",
                    pair = "XBTZAR",
                    limit_price = "1000",
                    limit_volume = "1"
                }));

        var client = CreateClient();

        // Act
        var result = await client.StopOrderAsync(new StopOrderCommand { ClientOrderId = clientOrderId });

        // Assert
        Assert.Equal(orderId, result.OrderId);
        Assert.True(result.Success);
        
        // Verify that /api/1/stoporder was NOT called
        var stopRequests = _server.FindLogEntries(Request.Create().WithPath("/api/1/stoporder"));
        Assert.Empty(stopRequests);
    }

    [Fact(DisplayName = "Given neither OrderId nor ClientOrderId, When stopping order, Then throw LunoValidationException.")]
    public async Task StopOrderAsync_NoIdsProvided_ThrowsValidationException()
    {
        // Arrange
        var client = CreateClient();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<LunoValidationException>(async () =>
            await client.StopOrderAsync(new StopOrderCommand()));

        Assert.Contains("Either OrderId or ClientOrderId must be provided", ex.Message);
    }

    [Fact(DisplayName = "Given StopPrice but no StopDirection, When posting limit order, Then throw LunoValidationException.")]
    public async Task PostLimitOrderAsync_PartialStopLimitParams_ThrowsValidationException()
    {
        // Arrange
        var client = CreateClient();
        var command = new PostLimitOrderCommand
        {
            Pair = "XBTZAR",
            Type = OrderType.Bid,
            Volume = 1m,
            Price = 1000m,
            BaseAccountId = 1,
            CounterAccountId = 2,
            StopPrice = 900m
            // Missing StopDirection
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<LunoValidationException>(async () =>
            await client.PostLimitOrderAsync(command));

        Assert.Contains("both StopPrice and StopDirection must be provided", ex.Message);
    }

    [Fact(DisplayName = "Given Idempotency Reconcilation, When exact match not found on lookup, Then throw LunoResourceNotFoundException.")]
    public async Task PostLimitOrderAsync_IdempotencyReconciliation_NotFound_ThrowsException()
    {
        // Arrange
        var clientId = "unique-uuid-123";

        // 1. Post Limit Order returns 409 Conflict
        _server.Given(Request.Create().WithPath("/api/1/postorder").UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(409)
                .WithHeader("Content-Type", "application/json")
                .WithBodyAsJson(new { error = "Duplicate", code = "ErrDuplicateClientOrderID" }));

        // 2. Reconciliation lookup returns generic error or empty order body (not found)
        _server.Given(Request.Create().WithPath("/api/exchange/3/order").WithParam("client_order_id", clientId).UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(404)
                .WithHeader("Content-Type", "application/json")
                .WithBodyAsJson(new { error = "Order not found" }));

        var client = CreateClient();
        var command = new PostLimitOrderCommand
        {
            Pair = "XBTMYR", Type = OrderType.Bid, Volume = 1m, Price = 10m, BaseAccountId = 1, CounterAccountId = 2, ClientOrderId = clientId
        };

        // Act & Assert
        await Assert.ThrowsAsync<LunoResourceNotFoundException>(async () => await client.PostLimitOrderAsync(command));
    }

    [Fact(DisplayName = "Given Idempotency Reconcilation, When parameters mismatch, Then throw LunoIdempotencyException.")]
    public async Task PostLimitOrderAsync_IdempotencyReconciliation_Mismatch_ThrowsException()
    {
        // Arrange
        var clientId = "unique-uuid-123";

        // 1. Post Limit Order returns 409 Conflict
        _server.Given(Request.Create().WithPath("/api/1/postorder").UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(409)
                .WithHeader("Content-Type", "application/json")
                .WithBodyAsJson(new { error = "Duplicate", code = "ErrDuplicateClientOrderID" }));

        // 2. Reconciliation lookup returns a fundamentally different order
        _server.Given(Request.Create().WithPath("/api/exchange/3/order").WithParam("client_order_id", clientId).UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBodyAsJson(new
                {
                    order_id = "BX123",
                    client_order_id = clientId,
                    limit_price = "999999", // Vastly different price
                    limit_volume = "0.001",
                    side = "BUY",
                    pair = "XBTMYR"
                }));

        var client = CreateClient();
        var command = new PostLimitOrderCommand
        {
            Pair = "XBTMYR", Type = OrderType.Bid, Volume = 0.001m, Price = 250000m, BaseAccountId = 1, CounterAccountId = 2, ClientOrderId = clientId
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<LunoIdempotencyException>(async () => await client.PostLimitOrderAsync(command));
        Assert.Contains("Price", ex.Message);
    }

    [Fact(DisplayName = "Given Idempotency Reconcilation, When Side differs, Then throw LunoIdempotencyException.")]
    public async Task PostLimitOrderAsync_IdempotencyReconciliation_SideMismatch_ThrowsException()
    {
        // Arrange
        var clientId = "unique-uuid-123";

        // 1. Post Limit Order returns 409 Conflict
        _server.Given(Request.Create().WithPath("/api/1/postorder").UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(409)
                .WithHeader("Content-Type", "application/json")
                .WithBodyAsJson(new { error = "Duplicate", code = "ErrDuplicateClientOrderID" }));

        // 2. Lookup returns order with different Side (SELL instead of expected BUY from Bid)
        _server.Given(Request.Create().WithPath("/api/exchange/3/order").WithParam("client_order_id", clientId).UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBodyAsJson(new
                {
                    order_id = "BX123",
                    client_order_id = clientId,
                    limit_price = "250000",
                    limit_volume = "0.001",
                    side = "SELL", // Mismatch!
                    pair = "XBTMYR"
                }));

        var client = CreateClient();
        var command = new PostLimitOrderCommand
        {
            Pair = "XBTMYR", Type = OrderType.Bid, Volume = 0.001m, Price = 250000m, BaseAccountId = 1, CounterAccountId = 2, ClientOrderId = clientId
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<LunoIdempotencyException>(async () => await client.PostLimitOrderAsync(command));
        Assert.Contains("Side", ex.Message);
    }

    [Theory(DisplayName = "Given an order ID, When looking up order, Then maps statuses correctly.")]
    [InlineData("AWAITING", OrderStatus.Awaiting)]
    [InlineData("PENDING", OrderStatus.Pending)]
    [InlineData("UNKNOWN_NONSENSE", OrderStatus.Awaiting)] // Unmapped default fallback
    public async Task GetOrderAsync_StatusMappings_MapsCorrectly(string apiStatus, OrderStatus expectedStatus)
    {
        var orderId = "BX123_STATUS";
        
        _server.Given(Request.Create().WithPath("/api/exchange/3/order").WithParam("id", orderId).UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBodyAsJson(new 
                { 
                    order_id = orderId, 
                    status = apiStatus, 
                    side = "BUY",
                    pair = "XBTZAR",
                    limit_price = "1000",
                    limit_volume = "1"
                }));

        var client = CreateClient();
        
        // Expose public method to call the interface lookup (via Trading.GetOrderAsync is not directly exposed as Command, so we cast to test infra)
        var infraClient = (ILunoTradingClient)client.Trading;
        var result = await infraClient.GetOrderAsync(orderId: orderId);

        Assert.Equal(expectedStatus, result.Status);
    }
}
