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
internal class LunoTradingClient(LunoApiClient api, ILunoRequestDispatcher requests) : ILunoTradingClient, ILunoTradingOperations
{
    private readonly LunoApiClient _apiClient = api;
    public ILunoRequestDispatcher Requests { get; } = requests;

    async Task<OrderReference> ILunoTradingOperations.FetchPostLimitOrderAsync(LimitOrderRequest request, CancellationToken ct)
    {
        var response = await _apiClient.Api.One.Postorder.PostAsync(req =>
        {
            // Mandatory fields
            req.QueryParameters.Pair = request.Pair;
            req.QueryParameters.TypeAsPostTypeQueryParameterType = MapPostSide(request.Side);
            req.QueryParameters.Volume = request.Volume.ToString(CultureInfo.InvariantCulture);
            req.QueryParameters.Price = request.Price.ToString(CultureInfo.InvariantCulture);

            // Account explicitly mandated by application-layer validation
            req.QueryParameters.BaseAccountId = request.BaseAccountId;
            req.QueryParameters.CounterAccountId = request.CounterAccountId;

            // Optional idempotency / behavior fields
            req.QueryParameters.ClientOrderId = request.ClientOrderId;
            req.QueryParameters.PostOnly = request.PostOnly;
            req.QueryParameters.StopPrice = request.StopPrice?.ToString(CultureInfo.InvariantCulture);
            req.QueryParameters.Timestamp = request.Timestamp;
            req.QueryParameters.Ttl = request.TTL;

            if (request.StopDirection != null)
                req.QueryParameters.StopDirectionAsPostStopDirectionQueryParameterType = MapStopDirection(request.StopDirection);

            req.QueryParameters.TimeInForceAsPostTimeInForceQueryParameterType = MapTimeInForce(request.TimeInForce);

            req.Options.Add(new LunoTelemetryOptions("PostLimitOrder"));
        }, ct);

        // Trust the API contract: a 200 OK guarantees OrderId is present.
        return new OrderReference { OrderId = response!.OrderId! };
    }

    async Task<bool> ILunoTradingOperations.FetchStopOrderAsync(string orderId, CancellationToken ct)
    {
        var response = await _apiClient.Api.One.Stoporder.PostAsync(req =>
        {
            req.QueryParameters.OrderId = orderId;
            req.Options.Add(new LunoTelemetryOptions("StopOrder"));
        }, ct);

        return response!.Success!.Value;
    }

    async Task<Order> ILunoTradingOperations.FetchOrderAsync(string? orderId, string? clientOrderId, CancellationToken ct)
    {
        var response = await _apiClient.Api.Exchange.Three.Order.GetAsync(req =>
        {
            if (orderId != null) req.QueryParameters.Id = orderId;
            if (clientOrderId != null) req.QueryParameters.ClientOrderId = clientOrderId;
            req.Options.Add(new LunoTelemetryOptions("GetOrder"));
        }, ct);

        return MapGetOrderResponse(response!);
    }

    async Task<System.Collections.Generic.IReadOnlyList<Order>> ILunoTradingOperations.FetchListOrdersAsync(
        OrderStatus? state,
        string? pair,
        long? createdBefore,
        long? limit,
        CancellationToken ct)
    {
        var response = await _apiClient.Api.Exchange.Two.Listorders.GetAsync(req =>
        {
            if (state.HasValue) req.QueryParameters.Closed = state.Value == OrderStatus.Complete;
            if (pair != null) req.QueryParameters.Pair = pair;
            if (createdBefore.HasValue) req.QueryParameters.CreatedBefore = createdBefore.Value;
            if (limit.HasValue) req.QueryParameters.Limit = limit.Value;

            req.Options.Add(new LunoTelemetryOptions("ListOrders"));
        }, ct);

        return response?.Orders?.Select(apiOrder => MapOrderV2(apiOrder)).ToList()
            ?? new System.Collections.Generic.List<Order>();
    }

    // ── Response → Domain mapping ────────────────────────────────────────────────

    private static Order MapGetOrderResponse(Generated.Models.GetOrder2Response r)
    {
        var orderId           = ParseStringMandatory(r.OrderId, "order_id");
        var side              = MapSide(r.Side);
        var status            = MapStatus(r.Status);
        var pair              = ParseStringMandatory(r.Pair, "pair");
        var creationTimestamp  = ParseLongMandatory(r.CreationTimestamp, "creation_timestamp");
        var baseAccountId     = (long?)r.BaseAccountId;
        var counterAccountId  = (long?)r.CounterAccountId;
        var clientOrderId     = r.ClientOrderId;
        var completedTimestamp = r.CompletedTimestamp;
        var expirationTimestamp = r.ExpirationTimestamp;
        var filledBase        = TryParseDecimal(r.Base);
        var filledCounter     = TryParseDecimal(r.Counter);
        var feeBase           = TryParseDecimal(r.FeeBase);
        var feeCounter        = TryParseDecimal(r.FeeCounter);

        var type = MapOrderType(r.Type);

        return type switch
        {
            OrderType.Limit => new LimitOrder(
                orderId, side, status, pair, creationTimestamp,
                baseAccountId, counterAccountId,
                limitPrice: ParseDecimalMandatory(r.LimitPrice, "limit_price"),
                limitVolume: ParseDecimalMandatory(r.LimitVolume, "limit_volume"),
                timeInForce: MapTimeInForceString(r.TimeInForce),
                clientOrderId: clientOrderId,
                completedTimestamp: completedTimestamp,
                expirationTimestamp: expirationTimestamp,
                filledBase: filledBase, filledCounter: filledCounter,
                feeBase: feeBase, feeCounter: feeCounter),

            OrderType.Market => new MarketOrder(
                orderId, side, status, pair, creationTimestamp,
                baseAccountId, counterAccountId,
                clientOrderId: clientOrderId,
                completedTimestamp: completedTimestamp,
                expirationTimestamp: expirationTimestamp,
                filledBase: filledBase, filledCounter: filledCounter,
                feeBase: feeBase, feeCounter: feeCounter),

            OrderType.StopLimit => new StopLimitOrder(
                orderId, side, status, pair, creationTimestamp,
                baseAccountId, counterAccountId,
                stopPrice: ParseDecimalMandatory(r.StopPrice, "stop_price"),
                stopDirection: MapStopDirectionResponse(r.StopDirection),
                limitPrice: ParseDecimalMandatory(r.LimitPrice, "limit_price"),
                limitVolume: ParseDecimalMandatory(r.LimitVolume, "limit_volume"),
                clientOrderId: clientOrderId,
                completedTimestamp: completedTimestamp,
                expirationTimestamp: expirationTimestamp,
                filledBase: filledBase, filledCounter: filledCounter,
                feeBase: feeBase, feeCounter: feeCounter),

            _ => throw new LunoMappingException($"Unmapped order type '{type}'.", nameof(Generated.Models.GetOrder2Response_type)),
        };
    }

    private static Order MapOrderV2(Generated.Models.OrderV2 r)
    {
        var orderId            = ParseStringMandatory(r.OrderId, "order_id");
        var side               = MapSideV2(r.Side);
        var status             = MapStatusV2(r.Status);
        var pair               = ParseStringMandatory(r.Pair, "pair");
        var creationTimestamp   = (long?)r.CreationTimestamp ?? throw new LunoMappingException("Mandatory field 'creation_timestamp' is missing in API response.");
        var baseAccountId      = (long?)r.BaseAccountId;
        var counterAccountId   = (long?)r.CounterAccountId;
        var clientOrderId      = r.ClientOrderId;
        var completedTimestamp  = (long?)r.CompletedTimestamp;
        var expirationTimestamp = (long?)r.ExpirationTimestamp;
        var filledBase         = TryParseDecimal(r.Base);
        var filledCounter      = TryParseDecimal(r.Counter);
        var feeBase            = TryParseDecimal(r.FeeBase);
        var feeCounter         = TryParseDecimal(r.FeeCounter);

        var type = MapOrderTypeV2(r.Type);

        return type switch
        {
            OrderType.Limit => new LimitOrder(
                orderId, side, status, pair, creationTimestamp,
                baseAccountId, counterAccountId,
                limitPrice: ParseDecimalMandatory(r.LimitPrice, "limit_price"),
                limitVolume: ParseDecimalMandatory(r.LimitVolume, "limit_volume"),
                timeInForce: MapTimeInForceString(r.TimeInForce),
                clientOrderId: clientOrderId,
                completedTimestamp: completedTimestamp,
                expirationTimestamp: expirationTimestamp,
                filledBase: filledBase, filledCounter: filledCounter,
                feeBase: feeBase, feeCounter: feeCounter),

            OrderType.Market => new MarketOrder(
                orderId, side, status, pair, creationTimestamp,
                baseAccountId, counterAccountId,
                clientOrderId: clientOrderId,
                completedTimestamp: completedTimestamp,
                expirationTimestamp: expirationTimestamp,
                filledBase: filledBase, filledCounter: filledCounter,
                feeBase: feeBase, feeCounter: feeCounter),

            OrderType.StopLimit => new StopLimitOrder(
                orderId, side, status, pair, creationTimestamp,
                baseAccountId, counterAccountId,
                stopPrice: ParseDecimalMandatory(r.StopPrice, "stop_price"),
                stopDirection: MapStopDirectionV2(r.StopDirection),
                limitPrice: ParseDecimalMandatory(r.LimitPrice, "limit_price"),
                limitVolume: ParseDecimalMandatory(r.LimitVolume, "limit_volume"),
                clientOrderId: clientOrderId,
                completedTimestamp: completedTimestamp,
                expirationTimestamp: expirationTimestamp,
                filledBase: filledBase, filledCounter: filledCounter,
                feeBase: feeBase, feeCounter: feeCounter),

            _ => throw new LunoMappingException($"Unmapped order type '{type}'.", nameof(Generated.Models.OrderV2_type)),
        };
    }

    // ── Private mappers (GetOrder V3) ────────────────────────────────────────────

    private static OrderStatus MapStatus(Generated.Models.GetOrder2Response_status? status) =>
        status switch
        {
            Generated.Models.GetOrder2Response_status.AWAITING  => OrderStatus.Awaiting,
            Generated.Models.GetOrder2Response_status.PENDING   => OrderStatus.Pending,
            Generated.Models.GetOrder2Response_status.COMPLETE  => OrderStatus.Complete,
            _ => throw new LunoMappingException($"Unmapped or null order status '{status}'.", nameof(Generated.Models.GetOrder2Response_status)),
        };

    private static OrderSide MapSide(Generated.Models.GetOrder2Response_side? side) =>
        side switch
        {
            Generated.Models.GetOrder2Response_side.BUY  => OrderSide.Buy,
            Generated.Models.GetOrder2Response_side.SELL => OrderSide.Sell,
            _ => throw new LunoMappingException($"Unmapped or null order side '{side}'.", nameof(Generated.Models.GetOrder2Response_side)),
        };

    private static OrderType MapOrderType(Generated.Models.GetOrder2Response_type? type) =>
        type switch
        {
            Generated.Models.GetOrder2Response_type.LIMIT     => OrderType.Limit,
            Generated.Models.GetOrder2Response_type.MARKET    => OrderType.Market,
            Generated.Models.GetOrder2Response_type.STOP_LIMIT => OrderType.StopLimit,
            _ => throw new LunoMappingException($"Unmapped or null order type '{type}'.", nameof(Generated.Models.GetOrder2Response_type)),
        };

    private static StopDirection MapStopDirectionResponse(Generated.Models.GetOrder2Response_stop_direction? dir) =>
        dir switch
        {
            Generated.Models.GetOrder2Response_stop_direction.ABOVE => StopDirection.Above,
            Generated.Models.GetOrder2Response_stop_direction.BELOW => StopDirection.Below,
            _ => throw new LunoMappingException($"Unmapped or null stop direction '{dir}'.", nameof(Generated.Models.GetOrder2Response_stop_direction)),
        };

    // ── Private mappers (ListOrders V2) ──────────────────────────────────────────

    private static OrderStatus MapStatusV2(Generated.Models.OrderV2_status? status) =>
        status switch
        {
            Generated.Models.OrderV2_status.AWAITING  => OrderStatus.Awaiting,
            Generated.Models.OrderV2_status.PENDING   => OrderStatus.Pending,
            Generated.Models.OrderV2_status.COMPLETE  => OrderStatus.Complete,
            _ => throw new LunoMappingException($"Unmapped or null order status '{status}'.", nameof(Generated.Models.OrderV2_status)),
        };

    private static OrderSide MapSideV2(Generated.Models.OrderV2_side? side) =>
        side switch
        {
            Generated.Models.OrderV2_side.BUY  => OrderSide.Buy,
            Generated.Models.OrderV2_side.SELL => OrderSide.Sell,
            _ => throw new LunoMappingException($"Unmapped or null order side '{side}'.", nameof(Generated.Models.OrderV2_side)),
        };

    private static OrderType MapOrderTypeV2(Generated.Models.OrderV2_type? type) =>
        type switch
        {
            Generated.Models.OrderV2_type.LIMIT     => OrderType.Limit,
            Generated.Models.OrderV2_type.MARKET    => OrderType.Market,
            Generated.Models.OrderV2_type.STOP_LIMIT => OrderType.StopLimit,
            _ => throw new LunoMappingException($"Unmapped or null order type '{type}'.", nameof(Generated.Models.OrderV2_type)),
        };

    private static StopDirection MapStopDirectionV2(Generated.Models.OrderV2_stop_direction? dir) =>
        dir switch
        {
            Generated.Models.OrderV2_stop_direction.ABOVE => StopDirection.Above,
            Generated.Models.OrderV2_stop_direction.BELOW => StopDirection.Below,
            _ => throw new LunoMappingException($"Unmapped or null stop direction '{dir}'.", nameof(Generated.Models.OrderV2_stop_direction)),
        };

    // ── Shared mappers ───────────────────────────────────────────────────────────

    private static TimeInForce MapTimeInForceString(string? tif) =>
        tif?.ToUpperInvariant() switch
        {
            "GTC" => TimeInForce.GTC,
            "IOC" => TimeInForce.IOC,
            "FOK" => TimeInForce.FOK,
            null  => TimeInForce.GTC, // Default for orders that don't specify (e.g. older orders)
            _     => throw new LunoMappingException($"Unmapped time in force '{tif}'."),
        };

    // ── Post mappers (Domain → Kiota) ────────────────────────────────────────────

    private static PostTypeQueryParameterType MapPostSide(OrderSide side) =>
        side switch
        {
            OrderSide.BuySide  => PostTypeQueryParameterType.BID,
            OrderSide.SellSide => PostTypeQueryParameterType.ASK,
            _                  => throw new ArgumentOutOfRangeException(nameof(side), $"Unexpected order side type: {side.GetType().Name}")
        };

    private static PostStop_directionQueryParameterType MapStopDirection(StopDirection direction) =>
        direction switch
        {
            StopDirection.RelativeLastTradeDirection => PostStop_directionQueryParameterType.RELATIVE_LAST_TRADE,
            StopDirection.AboveDirection             => PostStop_directionQueryParameterType.ABOVE,
            StopDirection.BelowDirection             => PostStop_directionQueryParameterType.BELOW,
            _                                        => throw new ArgumentOutOfRangeException(nameof(direction), $"Unexpected stop direction type: {direction.GetType().Name}")
        };

    private static PostTime_in_forceQueryParameterType MapTimeInForce(TimeInForce tif) =>
        tif switch
        {
            TimeInForce.GtcType => PostTime_in_forceQueryParameterType.GTC,
            TimeInForce.IocType => PostTime_in_forceQueryParameterType.IOC,
            TimeInForce.FokType => PostTime_in_forceQueryParameterType.FOK,
            _                   => throw new ArgumentOutOfRangeException(nameof(tif), $"Unexpected time-in-force type: {tif.GetType().Name}")
        };

    // ── Parse helpers ────────────────────────────────────────────────────────────

    private static decimal? TryParseDecimal(string? raw) =>
        raw != null && decimal.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out var v) ? v : null;

    private static decimal ParseDecimalMandatory(string? raw, string fieldName) =>
        TryParseDecimal(raw) ?? throw new LunoMappingException($"Mandatory field '{fieldName}' is missing or invalid in API response.");

    private static string ParseStringMandatory(string? raw, string fieldName) =>
        string.IsNullOrWhiteSpace(raw) ? throw new LunoMappingException($"Mandatory field '{fieldName}' is missing or empty in API response.") : raw;

    private static long ParseLongMandatory(long? raw, string fieldName) =>
        raw ?? throw new LunoMappingException($"Mandatory field '{fieldName}' is missing in API response.");
}
