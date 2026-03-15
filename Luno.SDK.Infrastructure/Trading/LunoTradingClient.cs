using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Kiota.Abstractions;
using Luno.SDK.Trading;
using Luno.SDK.Infrastructure.Generated;
using Luno.SDK.Infrastructure.Generated.Api.One.Postorder;
using Luno.SDK.Infrastructure.Telemetry;

namespace Luno.SDK.Infrastructure.Trading;

internal class LunoTradingClient(IRequestAdapter requestAdapter) : ILunoTradingClient
{
    private readonly LunoApiClient _apiClient = new(requestAdapter);

    public async Task<OrderReference> PostLimitOrderAsync(LimitOrderParameters parameters, CancellationToken ct = default)
    {
        // Note: Pre-flight validation is now handled by the Application Layer (LimitOrderParameters.Validate())
        // But we still wrap the raw API call and handle Idempotency.

        try
        {
            var response = await _apiClient.Api.One.Postorder.PostAsync(req =>
            {
                // Mandatory fields
                req.QueryParameters.Pair = parameters.Pair;
                req.QueryParameters.TypeAsPostTypeQueryParameterType = MapType(parameters.Type);
                req.QueryParameters.Volume = parameters.Volume.ToString(System.Globalization.CultureInfo.InvariantCulture);
                req.QueryParameters.Price = parameters.Price.ToString(System.Globalization.CultureInfo.InvariantCulture);
                
                // Account explicitly mandated by validation
                req.QueryParameters.BaseAccountId = (int?)parameters.BaseAccountId;
                req.QueryParameters.CounterAccountId = (int?)parameters.CounterAccountId;
                
                // Optional idempotency / behavior
                req.QueryParameters.ClientOrderId = parameters.ClientOrderId;
                req.QueryParameters.PostOnly = parameters.PostOnly;
                req.QueryParameters.StopPrice = parameters.StopPrice?.ToString(System.Globalization.CultureInfo.InvariantCulture);
                req.QueryParameters.Timestamp = (int?)parameters.Timestamp;
                req.QueryParameters.Ttl = (int?)parameters.TTL;

                if (parameters.StopDirection.HasValue)
                {
                    req.QueryParameters.StopDirectionAsPostStopDirectionQueryParameterType = MapStopDirection(parameters.StopDirection.Value);
                }

                req.QueryParameters.TimeInForceAsPostTimeInForceQueryParameterType = MapTimeInForce(parameters.TimeInForce);

            req.Options.Add(new LunoTelemetryOptions("PostLimitOrder"));
        }, ct);

        // We trust the API contract. If it succeeds (200 OK), OrderId is guaranteed to exist.
        return new OrderReference { OrderId = response!.OrderId! };
        }
        catch (LunoIdempotencyException) when (!string.IsNullOrWhiteSpace(parameters.ClientOrderId))
        {
            return await ReconcileDuplicateOrderAsync(parameters, ct);
        }
    }

    public async Task<bool> StopOrderAsync(string orderId, CancellationToken ct = default)
    {
        var response = await _apiClient.Api.One.Stoporder.PostAsync(req =>
        {
            req.QueryParameters.OrderId = orderId;
            req.Options.Add(new LunoTelemetryOptions("StopOrder"));
        }, ct);

        return response?.Success ?? false;
    }

    public async Task<Order> GetOrderAsync(string? orderId = null, string? clientOrderId = null, CancellationToken ct = default)
    {
        var response = await _apiClient.Api.Exchange.Three.Order.GetAsync(req =>
        {
            if (orderId != null) req.QueryParameters.Id = orderId;
            if (clientOrderId != null) req.QueryParameters.ClientOrderId = clientOrderId;
            req.Options.Add(new LunoTelemetryOptions("GetOrder"));
        }, ct);

        // Trusting Application layer validation for inputs and Kiota for the response contract.
        return new Order
        {
            OrderId = response!.OrderId!,
            ClientOrderId = response.ClientOrderId,
            Status = MapStatus(response.Status)
        };
    }

    private static OrderStatus MapStatus(Luno.SDK.Infrastructure.Generated.Models.GetOrder2Response_status? status)
    {
        return status switch
        {
            Luno.SDK.Infrastructure.Generated.Models.GetOrder2Response_status.AWAITING => OrderStatus.Awaiting,
            Luno.SDK.Infrastructure.Generated.Models.GetOrder2Response_status.PENDING => OrderStatus.Pending,
            Luno.SDK.Infrastructure.Generated.Models.GetOrder2Response_status.COMPLETE => OrderStatus.Complete,
            _ => OrderStatus.Awaiting // Default to safest
        };
    }

    // ValidatePreFlight removed, validation happens in Application layer via LimitOrderParameters.Validate()

    private async Task<OrderReference> ReconcileDuplicateOrderAsync(LimitOrderParameters parameters, CancellationToken ct)
    {
        var existingOrder = (await _apiClient.Api.Exchange.Three.Order.GetAsync(req =>
        {
            req.QueryParameters.ClientOrderId = parameters.ClientOrderId;
            req.Options.Add(new LunoTelemetryOptions("ReconcileDuplicateOrder"));
        }, ct))!;

        // Contract assumes existingOrder is not null if we get here without a 404 exception.
        bool parametersMatch = true;

        if (existingOrder.LimitPrice != null && decimal.TryParse(existingOrder.LimitPrice, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var existingPrice))
        {
            if (existingPrice != parameters.Price) parametersMatch = false;
        }

        if (existingOrder.LimitVolume != null && decimal.TryParse(existingOrder.LimitVolume, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var existingVolume))
        {
            if (existingVolume != parameters.Volume) parametersMatch = false;
        }

        if (existingOrder.Side.HasValue)
        {
            var requiredSide = parameters.Type == OrderType.Bid ? "BUY" : "SELL";
            if (!existingOrder.Side.Value.ToString().Equals(requiredSide, StringComparison.OrdinalIgnoreCase)) 
            {
                parametersMatch = false;
            }
        }

        if (!parametersMatch)
        {
            throw new LunoIdempotencyException("Idempotency failed: A previous order exists with the same ClientOrderId but the request parameters differ.");
        }

        return new OrderReference { OrderId = existingOrder.OrderId ?? string.Empty };
    }

    private PostTypeQueryParameterType MapType(OrderType type)
    {
        return type switch
        {
            OrderType.Bid => PostTypeQueryParameterType.BID,
            OrderType.Ask => PostTypeQueryParameterType.ASK,
            _ => throw new InvalidOperationException("Unreachable state due to Domain invariants.", new ArgumentOutOfRangeException(nameof(type), "Invalid order type."))
        };
    }

    private PostStop_directionQueryParameterType MapStopDirection(StopDirection direction)
    {
        return direction switch
        {
            StopDirection.RelativeLastTrade => PostStop_directionQueryParameterType.RELATIVE_LAST_TRADE,
            StopDirection.Above => PostStop_directionQueryParameterType.ABOVE,
            StopDirection.Below => PostStop_directionQueryParameterType.BELOW,
            _ => throw new InvalidOperationException("Unreachable state due to Domain invariants.", new ArgumentOutOfRangeException(nameof(direction), "Invalid stop direction."))
        };
    }

    private PostTime_in_forceQueryParameterType MapTimeInForce(TimeInForce tif)
    {
        return tif switch
        {
            TimeInForce.GTC => PostTime_in_forceQueryParameterType.GTC,
            TimeInForce.IOC => PostTime_in_forceQueryParameterType.IOC,
            TimeInForce.FOK => PostTime_in_forceQueryParameterType.FOK,
            _ => throw new InvalidOperationException("Unreachable state due to Domain invariants.", new ArgumentOutOfRangeException(nameof(tif), "Invalid time in force."))
        };
    }
}
