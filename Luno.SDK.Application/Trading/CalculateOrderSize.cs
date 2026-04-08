using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Luno.SDK.Market;
using Luno.SDK.Trading;

using System.Diagnostics;
using System.Diagnostics.Metrics;
using Luno.SDK.Telemetry;

namespace Luno.SDK.Application.Trading;

/// <summary>
/// A query to calculate the optimal order size (Volume and Price) for a given spend.
/// </summary>
public record CalculateOrderSizeQuery(
    string Pair,
    OrderSide Side,
    TradingAmount Spend,
    TradingPrice? AtPrice = null) : LunoQueryBase<OrderQuote>;

/// <summary>
/// Orchestrates the calculation of an <see cref="OrderQuote"/> by fetching market limitations and spread.
/// This handler focuses purely on business logic, with cross-cutting concerns handled by the pipeline.
/// </summary>
internal class CalculateOrderSizeHandler : ICommandHandler<CalculateOrderSizeQuery, OrderQuote>
{
    private readonly ILunoMarketOperations _marketOperations;

    public CalculateOrderSizeHandler(ILunoMarketOperations marketOperations)
    {
        ArgumentNullException.ThrowIfNull(marketOperations);
        _marketOperations = marketOperations;
    }

    public async Task<OrderQuote> HandleAsync(CalculateOrderSizeQuery query, CancellationToken ct = default)
    {
        var markets = await _marketOperations.FetchMarketsAsync(new[] { query.Pair }, ct).ConfigureAwait(false);
        var marketInfo = markets?.FirstOrDefault();
        
        if (marketInfo == null || !marketInfo.IsTradable())
        {
            throw new LunoMarketStateException($"Market {query.Pair} is not active or available for trading.");
        }

        decimal price;

        if (query.AtPrice != null)
        {
            price = query.AtPrice.Value;
        }
        else
        {
            var ticker = await _marketOperations.FetchTickerAsync(query.Pair, ct).ConfigureAwait(false);
            if (ticker == null || (query.Side == OrderSide.Buy && ticker.Ask <= 0) || (query.Side == OrderSide.Sell && ticker.Bid <= 0))
            {
                throw new LunoValidationException("Invalid price: Market has no liquidity or returned dead ticker.", "ErrInvalidPrice", null);
            }
            price = query.Side == OrderSide.Buy ? ticker.Ask : ticker.Bid;
        }

        if (price <= 0)
        {
            throw new LunoValidationException("Invalid price: Must be strictly greater than 0.", "ErrInvalidPrice", null);
        }

        // 1. Round to required scale based on side strategies FIRST
        price = Math.Round(price, marketInfo.PriceScale,
            query.Side == OrderSide.Buy ? MidpointRounding.ToNegativeInfinity : MidpointRounding.ToPositiveInfinity);

        // 2. Perform limit checks AFTER rounding
        if (price > marketInfo.MaxPrice || price < marketInfo.MinPrice)
        {
            throw new LunoValidationException($"Invalid price: Must be between {marketInfo.MinPrice} and {marketInfo.MaxPrice}.", "ErrInvalidPrice", null);
        }

        // Use the domain-specific ResolveBaseVolume method to encapsulate unit conversion rules.
        decimal unroundedVolume = query.Spend.ResolveBaseVolume(price);

        var volume = Math.Round(unroundedVolume, marketInfo.VolumeScale, MidpointRounding.ToZero);

        if (volume < marketInfo.MinVolume)
        {
            throw new LunoValidationException($"Volume {volume} is below the minimum allowed volume of {marketInfo.MinVolume}.", "ErrVolumeTooLow", null);
        }

        if (volume > marketInfo.MaxVolume)
        {
            throw new LunoValidationException($"Volume {volume} exceeds the maximum allowed volume of {marketInfo.MaxVolume}.", "ErrVolumeTooHigh", null);
        }

        return new OrderQuote(query.Pair, query.Side, volume, price, marketInfo.CounterCurrency);
    }
}
