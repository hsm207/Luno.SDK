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

    public async Task<OrderResponse> PostLimitOrderAsync(PostLimitOrderRequest request, CancellationToken ct = default)
    {
        ValidatePreFlight(request);

        try
        {
            var response = await _apiClient.Api.One.Postorder.PostAsync(req =>
            {
                // Mandatory fields
                req.QueryParameters.Pair = request.Pair;
                req.QueryParameters.TypeAsPostTypeQueryParameterType = MapType(request.Type);
                req.QueryParameters.Volume = request.Volume.ToString(System.Globalization.CultureInfo.InvariantCulture);
                req.QueryParameters.Price = request.Price.ToString(System.Globalization.CultureInfo.InvariantCulture);
                
                // Account explicitly mandated by validation
                req.QueryParameters.BaseAccountId = (int?)request.BaseAccountId;
                req.QueryParameters.CounterAccountId = (int?)request.CounterAccountId;
                
                // Optional idempotency / behavior
                req.QueryParameters.ClientOrderId = request.ClientOrderId;
                req.QueryParameters.PostOnly = request.PostOnly;
                req.QueryParameters.StopPrice = request.StopPrice?.ToString(System.Globalization.CultureInfo.InvariantCulture);
                req.QueryParameters.Timestamp = (int?)request.Timestamp;
                req.QueryParameters.Ttl = (int?)request.TTL;

                if (request.StopDirection.HasValue)
                {
                    req.QueryParameters.StopDirectionAsPostStopDirectionQueryParameterType = MapStopDirection(request.StopDirection.Value);
                }

                if (request.TimeInForce.HasValue)
                {
                    req.QueryParameters.TimeInForceAsPostTimeInForceQueryParameterType = MapTimeInForce(request.TimeInForce.Value);
                }

                req.Options.Add(new LunoTelemetryOptions("PostLimitOrder"));
            }, ct);

            if (response?.OrderId == null)
            {
                throw new LunoMappingException("The API response was successful but no OrderId was returned.", "PostLimitOrderResponse");
            }

            return new OrderResponse { OrderId = response.OrderId };
        }
        catch (LunoIdempotencyException) when (!string.IsNullOrWhiteSpace(request.ClientOrderId))
        {
            return await ReconcileDuplicateOrderAsync(request, ct);
        }
    }

    public async Task<bool> StopOrderAsync(string orderId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(orderId))
        {
            throw new LunoValidationException("OrderId cannot be null or whitespace.");
        }

        var response = await _apiClient.Api.One.Stoporder.PostAsync(req =>
        {
            req.QueryParameters.OrderId = orderId;
            req.Options.Add(new LunoTelemetryOptions("StopOrder"));
        }, ct);

        return response?.Success ?? false;
    }

    public async Task<bool> StopOrderByClientOrderIdAsync(string clientOrderId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(clientOrderId))
        {
            throw new LunoValidationException("ClientOrderId cannot be null or whitespace.");
        }

        var existingOrder = await _apiClient.Api.Exchange.Three.Order.GetAsync(req =>
        {
            req.QueryParameters.ClientOrderId = clientOrderId;
            req.Options.Add(new LunoTelemetryOptions("GetOrderByClientOrderId"));
        }, ct);

        if (string.IsNullOrWhiteSpace(existingOrder?.OrderId))
        {
            throw new LunoResourceNotFoundException($"No existing order found with ClientOrderId: {clientOrderId}");
        }

        return await StopOrderAsync(existingOrder.OrderId, ct);
    }

    private void ValidatePreFlight(PostLimitOrderRequest request)
    {
        if (request.BaseAccountId == null || request.CounterAccountId == null)
        {
            throw new LunoValidationException("Explicit Account Mandate violated: BaseAccountId and CounterAccountId must be provided.");
        }

        if (request.PostOnly == true && (request.TimeInForce == TimeInForce.IOC || request.TimeInForce == TimeInForce.FOK))
        {
            throw new LunoValidationException("Pre-flight validation failed: PostOnly cannot be used with IOC or FOK TimeInForce.");
        }
    }

    private async Task<OrderResponse> ReconcileDuplicateOrderAsync(PostLimitOrderRequest request, CancellationToken ct)
    {
        var existingOrder = await _apiClient.Api.Exchange.Three.Order.GetAsync(req =>
        {
            req.QueryParameters.ClientOrderId = request.ClientOrderId;
            req.Options.Add(new LunoTelemetryOptions("ReconcileDuplicateOrder"));
        }, ct);

        if (existingOrder == null)
        {
            throw new LunoResourceNotFoundException($"Idempotency failed: Received 409 but lookup by ClientOrderId '{request.ClientOrderId}' returned empty.");
        }

        bool parametersMatch = true;

        if (existingOrder.LimitPrice != null && decimal.TryParse(existingOrder.LimitPrice, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var existingPrice))
        {
            if (existingPrice != request.Price) parametersMatch = false;
        }

        if (existingOrder.LimitVolume != null && decimal.TryParse(existingOrder.LimitVolume, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var existingVolume))
        {
            if (existingVolume != request.Volume) parametersMatch = false;
        }

        if (existingOrder.Side.HasValue)
        {
            var requiredSide = request.Type == OrderType.Bid ? "BUY" : "SELL";
            if (!existingOrder.Side.Value.ToString().Equals(requiredSide, StringComparison.OrdinalIgnoreCase)) 
            {
                parametersMatch = false;
            }
        }

        if (!parametersMatch)
        {
            throw new LunoIdempotencyException("Idempotency failed: A previous order exists with the same ClientOrderId but the request parameters differ.");
        }

        return new OrderResponse { OrderId = existingOrder.OrderId ?? string.Empty };
    }

    private PostTypeQueryParameterType MapType(OrderType type)
    {
        return type switch
        {
            OrderType.Bid => PostTypeQueryParameterType.BID,
            OrderType.Ask => PostTypeQueryParameterType.ASK,
            _ => throw new ArgumentOutOfRangeException(nameof(type), "Invalid order type.")
        };
    }

    private PostStop_directionQueryParameterType MapStopDirection(StopDirection direction)
    {
        return direction switch
        {
            StopDirection.RelativeLastTrade => PostStop_directionQueryParameterType.RELATIVE_LAST_TRADE,
            StopDirection.Above => PostStop_directionQueryParameterType.ABOVE,
            StopDirection.Below => PostStop_directionQueryParameterType.BELOW,
            _ => throw new ArgumentOutOfRangeException(nameof(direction), "Invalid stop direction.")
        };
    }

    private PostTime_in_forceQueryParameterType MapTimeInForce(TimeInForce tif)
    {
        return tif switch
        {
            TimeInForce.GTC => PostTime_in_forceQueryParameterType.GTC,
            TimeInForce.IOC => PostTime_in_forceQueryParameterType.IOC,
            TimeInForce.FOK => PostTime_in_forceQueryParameterType.FOK,
            _ => throw new ArgumentOutOfRangeException(nameof(tif), "Invalid time in force.")
        };
    }
}
