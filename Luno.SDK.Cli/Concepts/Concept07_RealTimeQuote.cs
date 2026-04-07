using System;
using System.Linq;
using System.Threading.Tasks;
using Luno.SDK;
using Luno.SDK.Trading;
using Luno.SDK.Application.Trading;
using Luno.SDK.Market;

namespace Luno.SDK.Cli.Concepts;

/// <summary>
/// Provides a demonstration of calculating a limit order quote using live market prices.
/// </summary>
public static class Concept07_RealTimeQuote
{
    public static async Task RunAsync()
    {
        Console.WriteLine("\n=== Concept 07: Real-Time Limit Order Intent ===");

        var client = new LunoClient(); // Public data only for this demo

        try
        {
            const string pair = "XBTMYR";
            const decimal targetSpend = 100.00m;

            Console.WriteLine($"\n📡 Fetching live market context for {pair}...");
            
            // Parallel fetch for speed ⚡
            var tickerTask = client.Market.GetTickerAsync(pair);
            var marketTask = client.Market.GetMarketsAsync(new[] { pair });
            await Task.WhenAll(tickerTask, marketTask);

            var ticker = tickerTask.Result;
            var market = marketTask.Result.First();

            Console.WriteLine($"\n📡 Current Market Price: {ticker.Price:N2} {market.CounterCurrency}");
            Console.WriteLine($"\n📡 Calculating quote for {targetSpend:N2} {market.CounterCurrency} spend at current ASK price...");

            // Calculate the Quote using current ticker price (using Ask since we are Buying)
            var quote = await client.Trading.CalculateOrderSizeAsync(new CalculateOrderSizeQuery(
                Pair: pair,
                Side: OrderSide.Buy,
                Spend: TradingAmount.InQuote(targetSpend),
                AtPrice: TradingPrice.InQuote(ticker.Ask)
            ));

            // Display with our sexy high-fidelity DX 💅
            string volFmt = $"N{market.VolumeScale}";
            string priceFmt = $"N{market.PriceScale}";
            string sideAction = quote.Side == OrderSide.Buy ? "BUY" : "SELL";
            string financialAction = quote.Side == OrderSide.Buy ? "PAY" : "RECEIVE";

            Console.WriteLine("\n--- 📝 PROPOSED ORDER INTENT ---");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"[QUOTE] ACTION:    {sideAction} {quote.Volume.ToString(volFmt)} {market.BaseCurrency}");
            Console.WriteLine($"[QUOTE] PRICE:     {quote.Price.ToString(priceFmt)} {market.CounterCurrency} per {market.BaseCurrency}");
            Console.WriteLine($"[QUOTE] FINANCIAL: You will {financialAction} {quote.GrossQuoteValue:N2} {quote.QuoteCurrency} (estimated, excl. fees)");
            Console.ResetColor();
            Console.WriteLine("--------------------------------\n");

            Console.WriteLine("✨ Intention calculated successfully based on live market data!");
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n[Error] {ex.Message}");
            Console.ResetColor();
        }
    }
}
