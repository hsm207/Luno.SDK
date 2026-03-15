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
/// </summary>
/// <param name="api">The generated Kiota API client.</param>
/// <param name="commands">The command dispatcher for the application layer.</param>
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

        return response?.Success ?? false;
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
            OrderId      = response!.OrderId!,
            ClientOrderId = response.ClientOrderId,
            Status       = MapStatus(response.Status),
            LimitPrice   = TryParseDecimal(response.LimitPrice),
            LimitVolume  = TryParseDecimal(response.LimitVolume),
            Side         = MapSide(response.Side),
        };
    }

    // ── Private mappers ─────────────────────────────────────────────────────────

    private static OrderStatus MapStatus(Generated.Models.GetOrder2Response_status? status) =>
        status switch
        {
            Generated.Models.GetOrder2Response_status.AWAITING  => OrderStatus.Awaiting,
            Generated.Models.GetOrder2Response_status.PENDING   => OrderStatus.Pending,
            Generated.Models.GetOrder2Response_status.COMPLETE  => OrderStatus.Complete,
            null                                                 => OrderStatus.Awaiting,
            _ => throw new LunoMappingException($"Unmapped order status '{status}'.", nameof(Generated.Models.GetOrder2Response_status)),
        };


    private static OrderType? MapSide(Generated.Models.GetOrder2Response_side? side) =>
        side switch
        {
            Generated.Models.GetOrder2Response_side.BUY  => OrderType.Bid,
            Generated.Models.GetOrder2Response_side.SELL => OrderType.Ask,
            _                                            => null,
        };

    private static decimal? TryParseDecimal(string? raw) =>
        raw != null && decimal.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out var v) ? v : null;

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
