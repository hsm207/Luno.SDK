using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Luno.SDK.Market;
using Luno.SDK.Trading;

namespace Luno.SDK.Application.Trading;

/// <summary>
/// A query to calculate the optimal order size (Volume and Price) for a given spend.
/// </summary>
/// <param name="Pair">The trading pair.</param>
/// <param name="Side">The side of the order (Buy/Sell).</param>
/// <param name="Spend">The amount to spend, explicitly tracking Base or Quote currency.</param>
/// <param name="AtPrice">Optional explicit price to use instead of the current market ticker.</param>
public record CalculateOrderSizeQuery(
    string Pair,
    OrderSide Side,
    TradingAmount Spend,
    TradingPrice? AtPrice = null);

/// <summary>
/// Orchestrates the calculation of an <see cref="OrderQuote"/> by fetching market limitations and spread.
/// </summary>
internal class CalculateOrderSizeHandler : ICommandHandler<CalculateOrderSizeQuery, Task<OrderQuote>>
{
    private readonly ILunoMarketOperations _marketOperations;

    public CalculateOrderSizeHandler(ILunoMarketOperations marketOperations)
    {
        _marketOperations = marketOperations;
    }

    public async Task<OrderQuote> HandleAsync(CalculateOrderSizeQuery query, CancellationToken ct = default)
    {
        var markets = await _marketOperations.FetchMarketsAsync(new[] { query.Pair }, ct).ConfigureAwait(false);
        var marketInfo = markets?.FirstOrDefault();
        
        if (marketInfo == null || (marketInfo.Status != MarketStatus.Active && marketInfo.Status != MarketStatus.PostOnly))
        {
            throw new LunoMarketStateException($"Market {query.Pair} is not active or post-only.");
        }

        decimal price;

        if (query.AtPrice.HasValue)
        {
            price = query.AtPrice.Value.Value;
        }
        else
        {
            var ticker = await _marketOperations.FetchTickerAsync(query.Pair, ct).ConfigureAwait(false);
            if (ticker == null || ticker.Ask <= 0 || ticker.Bid <= 0)
            {
                throw new LunoValidationException("Invalid price: Market has no liquidity or returned dead ticker.");
            }
            price = query.Side == OrderSide.Buy ? ticker.Ask : ticker.Bid;
        }

        if (price <= 0)
        {
            throw new LunoValidationException("Invalid price: Must be strictly greater than 0.");
        }

        if (price > marketInfo.MaxPrice || price < marketInfo.MinPrice)
        {
            throw new LunoValidationException($"Invalid price: Must be between {marketInfo.MinPrice} and {marketInfo.MaxPrice}.");
        }

        price = Math.Round(price, marketInfo.PriceScale,
            query.Side == OrderSide.Buy ? MidpointRounding.ToNegativeInfinity : MidpointRounding.ToPositiveInfinity);

        decimal unroundedVolume = query.Spend.Unit == TradingUnit.Quote
            ? query.Spend.Value / price
            : query.Spend.Value;

        var volume = Math.Round(unroundedVolume, marketInfo.VolumeScale, MidpointRounding.ToZero);

        if (volume < marketInfo.MinVolume || volume > marketInfo.MaxVolume)
        {
            throw new LunoValidationException($"Volume {volume} is out of bounds. Below minimum volume.");
        }

        return new OrderQuote(query.Pair, query.Side, volume, price, marketInfo.CounterCurrency);
    }
}
