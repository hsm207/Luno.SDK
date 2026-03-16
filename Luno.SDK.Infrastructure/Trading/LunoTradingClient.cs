using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Kiota.Abstractions;
using Luno.SDK.Trading;
using Luno.SDK.Infrastructure.Generated;
using Luno.SDK.Infrastructure.Generated.Api.One.Postorder;
using Luno.SDK.Infrastructure.Telemetry;

namespace Luno.SDK.Infrastructure.Trading;

/// <summary>
/// Provides a concrete implementation of the trading clients using the generated Kiota client.
/// </summary>
internal class LunoTradingClient(LunoApiClient api, ILunoCommandDispatcher commands) : ILunoTradingClient
{
    private readonly LunoApiClient _apiClient = api;
    public ILunoCommandDispatcher Commands { get; } = commands;

    public async Task<OrderReference> FetchPostLimitOrderAsync(LimitOrderRequest request, CancellationToken ct = default)
    {
        var response = await _apiClient.Api.One.Postorder.PostAsync(req =>
        {
            // Mandatory fields
            req.QueryParameters.Pair = request.Pair;
            req.QueryParameters.TypeAsPostTypeQueryParameterType = MapType(request.Type);
            req.QueryParameters.Volume = request.Volume.ToString(CultureInfo.InvariantCulture);
            req.QueryParameters.Price = request.Price.ToString(CultureInfo.InvariantCulture);

            // Account explicitly mandated by application-layer validation
            req.QueryParameters.BaseAccountId = (int?)request.BaseAccountId;
            req.QueryParameters.CounterAccountId = (int?)request.CounterAccountId;

            // Optional idempotency / behavior fields
            req.QueryParameters.ClientOrderId = request.ClientOrderId;
            req.QueryParameters.PostOnly = request.PostOnly;
            req.QueryParameters.StopPrice = request.StopPrice?.ToString(CultureInfo.InvariantCulture);
            req.QueryParameters.Timestamp = (int?)request.Timestamp;
            req.QueryParameters.Ttl = (int?)request.TTL;

            if (request.StopDirection.HasValue)
                req.QueryParameters.StopDirectionAsPostStopDirectionQueryParameterType = MapStopDirection(request.StopDirection.Value);

            req.QueryParameters.TimeInForceAsPostTimeInForceQueryParameterType = MapTimeInForce(request.TimeInForce);

            req.Options.Add(new LunoTelemetryOptions("PostLimitOrder"));
        }, ct);

        // Trust the API contract: a 200 OK guarantees OrderId is present.
        return new OrderReference { OrderId = response!.OrderId! };
    }

    public async Task<bool> FetchStopOrderAsync(string orderId, CancellationToken ct = default)
    {
        var response = await _apiClient.Api.One.Stoporder.PostAsync(req =>
        {
            req.QueryParameters.OrderId = orderId;
            req.Options.Add(new LunoTelemetryOptions("StopOrder"));
        }, ct);

        return response!.Success!.Value;
    }

    public async Task<Order> FetchOrderAsync(string? orderId = null, string? clientOrderId = null, CancellationToken ct = default)
    {
        var response = await _apiClient.Api.Exchange.Three.Order.GetAsync(req =>
        {
            if (orderId != null) req.QueryParameters.Id = orderId;
            if (clientOrderId != null) req.QueryParameters.ClientOrderId = clientOrderId;
            req.Options.Add(new LunoTelemetryOptions("GetOrder"));
        }, ct);

        // Infrastructure's only job here: map Kiota response fields → domain model.
        // Parsing is Infrastructure's concern; business logic (comparison) belongs to the caller.
        return new Order
        {
            OrderId           = ParseStringMandatory(response!.OrderId, "order_id"),
            ClientOrderId     = response.ClientOrderId,
            Status            = MapStatus(response.Status),
            LimitPrice        = ParseDecimalMandatory(response.LimitPrice, "limit_price"),
            LimitVolume       = ParseDecimalMandatory(response.LimitVolume, "limit_volume"),
            Side              = MapSide(response.Side),
            Pair              = ParseStringMandatory(response.Pair, "pair"),
            CreationTimestamp = ParseLongMandatory(response.CreationTimestamp, "creation_timestamp"),
        };
    }

    public async Task<System.Collections.Generic.IReadOnlyList<Order>> FetchListOrdersAsync(
        OrderStatus? state = null,
        string? pair = null,
        long? createdBefore = null,
        long? limit = null,
        CancellationToken ct = default)
    {
        var response = await _apiClient.Api.One.Listorders.GetAsync(req =>
        {
            if (state.HasValue) req.QueryParameters.StateAsGetStateQueryParameterType = MapListOrdersState(state.Value);
            if (pair != null) req.QueryParameters.Pair = pair;
            if (createdBefore.HasValue) req.QueryParameters.CreatedBefore = (int)createdBefore.Value;
            
            req.Options.Add(new LunoTelemetryOptions("ListOrders"));
        }, ct);

        return response?.Orders?.Select(apiOrder => new Order
        {
            OrderId           = ParseStringMandatory(apiOrder.OrderId, "order_id"),
            ClientOrderId     = null, // List returns no client order ID
            Status            = MapOrderStatus(apiOrder.State),
            LimitPrice        = ParseDecimalMandatory(apiOrder.LimitPrice, "limit_price"),
            LimitVolume       = ParseDecimalMandatory(apiOrder.LimitVolume, "limit_volume"),
            Side              = MapOrderType(apiOrder.Type),
            Pair              = ParseStringMandatory(apiOrder.Pair, "pair"),
            CreationTimestamp = ParseLongMandatory(apiOrder.CreationTimestamp, "creation_timestamp"),
        }).ToList() ?? new System.Collections.Generic.List<Order>();
    }

    // ── Private mappers ─────────────────────────────────────────────────────────

    private static OrderStatus MapStatus(Generated.Models.GetOrder2Response_status? status) =>
        status switch
        {
            Generated.Models.GetOrder2Response_status.AWAITING  => OrderStatus.Awaiting,
            Generated.Models.GetOrder2Response_status.PENDING   => OrderStatus.Pending,
            Generated.Models.GetOrder2Response_status.COMPLETE  => OrderStatus.Complete,
            _ => throw new LunoMappingException($"Unmapped or null order status '{status}'.", nameof(Generated.Models.GetOrder2Response_status)),
        };

    private static OrderType MapSide(Generated.Models.GetOrder2Response_side? side) =>
        side switch
        {
            Generated.Models.GetOrder2Response_side.BUY  => OrderType.Bid,
            Generated.Models.GetOrder2Response_side.SELL => OrderType.Ask,
            _ => throw new LunoMappingException($"Unmapped or null order side '{side}'.", nameof(Generated.Models.GetOrder2Response_side)),
        };

    private static OrderStatus MapOrderStatus(Generated.Models.Order_state? state) =>
        state switch
        {
            Generated.Models.Order_state.PENDING  => OrderStatus.Pending,
            Generated.Models.Order_state.COMPLETE => OrderStatus.Complete,
            _ => throw new LunoMappingException($"Unmapped or null list order state '{state}'.", nameof(Generated.Models.Order_state))
        };

    private static OrderType MapOrderType(Generated.Models.Order_type? type) =>
        type switch
        {
            Generated.Models.Order_type.BUY  => OrderType.Bid,
            Generated.Models.Order_type.SELL => OrderType.Ask,
            Generated.Models.Order_type.BID  => OrderType.Bid,
            Generated.Models.Order_type.ASK  => OrderType.Ask,
            _ => throw new LunoMappingException($"Unmapped or null order type '{type}'.", nameof(Generated.Models.Order_type))
        };

    private static Generated.Api.One.Listorders.GetStateQueryParameterType MapListOrdersState(OrderStatus state) =>
        state switch
        {
            OrderStatus.Pending => Generated.Api.One.Listorders.GetStateQueryParameterType.PENDING,
            OrderStatus.Complete => Generated.Api.One.Listorders.GetStateQueryParameterType.COMPLETE,
            _ => throw new InvalidOperationException("Unreachable state due to Domain invariants.", new ArgumentOutOfRangeException(nameof(state), "Invalid list orders state.")),
        };

    private static decimal? TryParseDecimal(string? raw) =>
        raw != null && decimal.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out var v) ? v : null;

    private static decimal ParseDecimalMandatory(string? raw, string fieldName) =>
        TryParseDecimal(raw) ?? throw new LunoMappingException($"Mandatory field '{fieldName}' is missing or invalid in API response.");

    private static string ParseStringMandatory(string? raw, string fieldName) =>
        string.IsNullOrWhiteSpace(raw) ? throw new LunoMappingException($"Mandatory field '{fieldName}' is missing or empty in API response.") : raw;

    private static long ParseLongMandatory(long? raw, string fieldName) =>
        raw ?? throw new LunoMappingException($"Mandatory field '{fieldName}' is missing in API response.");

    private static PostTypeQueryParameterType MapType(OrderType type) =>
        type switch
        {
            OrderType.Bid => PostTypeQueryParameterType.BID,
            OrderType.Ask => PostTypeQueryParameterType.ASK,
            _             => throw new InvalidOperationException("Unreachable state due to Domain invariants.", new ArgumentOutOfRangeException(nameof(type), "Invalid order type.")),
        };

    private static PostStop_directionQueryParameterType MapStopDirection(StopDirection direction) =>
        direction switch
        {
            StopDirection.RelativeLastTrade => PostStop_directionQueryParameterType.RELATIVE_LAST_TRADE,
            StopDirection.Above             => PostStop_directionQueryParameterType.ABOVE,
            StopDirection.Below             => PostStop_directionQueryParameterType.BELOW,
            _                               => throw new InvalidOperationException("Unreachable state due to Domain invariants.", new ArgumentOutOfRangeException(nameof(direction), "Invalid stop direction.")),
        };

    private static PostTime_in_forceQueryParameterType MapTimeInForce(TimeInForce tif) =>
        tif switch
        {
            TimeInForce.GTC => PostTime_in_forceQueryParameterType.GTC,
            TimeInForce.IOC => PostTime_in_forceQueryParameterType.IOC,
            TimeInForce.FOK => PostTime_in_forceQueryParameterType.FOK,
            _               => throw new InvalidOperationException("Unreachable state due to Domain invariants.", new ArgumentOutOfRangeException(nameof(tif), "Invalid time in force.")),
        };
}
