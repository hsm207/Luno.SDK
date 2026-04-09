using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Luno.SDK;
using Luno.SDK.Trading;
using Luno.SDK.Application.Trading;
using Luno.SDK.Market;
using Luno.SDK.Application.Market;
using Luno.SDK.Account;
using Luno.SDK.Application.Account;

namespace Luno.SDK.Cli.Concepts;

/// <summary>
/// Provides a comprehensive demonstration of the Limit Order API lifecycle.
/// </summary>
public static class Concept06_Orders
{
    public static async Task RunAsync()
    {
        Console.WriteLine("\n=== Concept 06: Limit Order Lifecycle (Private API) ===");

        var options = LoadCredentials();
        var client = new LunoClient(options);

        try
        {
            await DemonstrateLimitOrderLifecycleAsync(client);
        }
        catch (Exception ex)
        {
            HandleException(ex);
        }
    }

    private static LunoClientOptions LoadCredentials()
    {
        var config = new ConfigurationBuilder()
            .AddUserSecrets<Program>()
            .Build();

        string? keyId = config["Luno:ReadOnly:ApiKeyId"];
        string? keySecret = config["Luno:ReadOnly:ApiKeySecret"];

        if (string.IsNullOrWhiteSpace(keyId))
        {
            Console.WriteLine("No credentials found in User Secrets. Falling back to manual input.");
            Console.Write("Enter your API Key ID: ");
            keyId = Console.ReadLine();

            Console.Write("Enter your API Key Secret: ");
            keySecret = Console.ReadLine();
        }
        else
        {
            Console.WriteLine("Loaded API credentials from User Secrets! 💅");
        }

        var options = new LunoClientOptions();
        if (!string.IsNullOrWhiteSpace(keyId) && !string.IsNullOrWhiteSpace(keySecret))
        {
            options.WithCredentials(keyId, keySecret);
        }
        return options;
    }

    private static async Task DemonstrateLimitOrderLifecycleAsync(ILunoClient client)
    {
        const string targetPair = "XBTMYR";
        Console.WriteLine($"\n--- Starting Limit Order Lifecycle Demonstration for {targetPair} ---");

        // 1. Snapshot Initial State
        Console.WriteLine($"\n📡 Snapshot: Fetching current pending orders for {targetPair}...");
        var initialPending = await client.Trading.ListOrdersAsync(new ListOrdersQuery { State = OrderStatus.Pending, Pair = targetPair });
        Console.WriteLine($"[STATE] Current Pending Count: {initialPending.Count}");

        // 2. Resolve Accounts and Market Constraints
        var (baseId, counterId) = await ResolveTradingAccountsAsync(client, "XBT", "MYR");
        var market = await ResolveMarketMetadataAsync(client, targetPair);

        // 3. Calculate Order Size (Optimal Quote)
        Console.WriteLine($"\n📡 Strategy: Calculating optimal quote at MinPrice ({market.MinPrice:N2} {market.CounterCurrency})...");
        var quote = await client.Trading.CalculateOrderSizeAsync(new CalculateOrderSizeQuery(
            Pair: targetPair,
            Side: OrderSide.Buy,
            Spend: TradingAmount.InQuote(100),
            AtPrice: TradingPrice.InQuote(market.MinPrice)
        ));

        // Format according to market constraints for perfect UX 💅
        string volFmt = $"N{market.VolumeScale}";
        string priceFmt = $"N{market.PriceScale}";
        string sideAction = quote.Side == OrderSide.Buy ? "BUY" : "SELL";
        string financialAction = quote.Side == OrderSide.Buy ? "PAY" : "RECEIVE";

        Console.WriteLine($"[QUOTE] ACTION:    {sideAction} {quote.Volume.ToString(volFmt)} {market.BaseCurrency}");
        Console.WriteLine($"[QUOTE] PRICE:     {quote.Price.ToString(priceFmt)} {market.CounterCurrency} per {market.BaseCurrency}");
        Console.WriteLine($"[QUOTE] FINANCIAL: You will {financialAction} {quote.GrossQuoteValue:N2} {quote.QuoteCurrency} (estimated, excl. fees)");

        // 4. Post the Order with Idempotency
        string clientOrderId = Guid.NewGuid().ToString();
        var command = quote.ToCommand(baseId, counterId, clientOrderId);

        Console.WriteLine("\n📡 Action: Placing Limit Order (requires explicit write intent)...");
        var response = await client.Trading.PostLimitOrderAsync(command with { Options = command.Options with { AuthorizeWriteOperation = true } });
        Console.WriteLine($"[POST] Success! OrderId: {response.OrderId}");

        // 5. Verify via Listing
        Console.WriteLine($"\n📡 Verification: Fetching pending orders for {targetPair} after placement...");
        var midPending = await client.Trading.ListOrdersAsync(new ListOrdersQuery { State = OrderStatus.Pending, Pair = targetPair });
        bool isListed = midPending.Any(o => o.OrderId == response.OrderId);
        Console.WriteLine($"[VERIFY] Order {response.OrderId} found in pending list: {isListed}");

        // 6. Test Idempotency Reconciliation
        Console.WriteLine("\n📡 Idempotency: Resending same command (requires explicit write intent)...");
        var duplicateResponse = await client.Trading.PostLimitOrderAsync(command with { Options = command.Options with { AuthorizeWriteOperation = true } });
        Console.WriteLine($"[IDEMPOTENCY] Reconciled to same OrderId: {duplicateResponse.OrderId == response.OrderId}");

        // 7. Cancel the order
        Console.WriteLine("\n📡 Cleanup: Cancelling Order (requires explicit write intent)...");
        await client.Trading.StopOrderAsync(new StopOrderCommand { OrderId = response.OrderId, Options = new LunoRequestOptions { AuthorizeWriteOperation = true } });
        Console.WriteLine("[STOP] Stop request dispatched.");

        // 8. Final Verification
        Console.WriteLine($"\n📡 Final State: Fetching pending orders for {targetPair} after cancellation...");
        var finalPending = await client.Trading.ListOrdersAsync(new ListOrdersQuery { State = OrderStatus.Pending, Pair = targetPair });
        bool isRemoved = finalPending.All(o => o.OrderId != response.OrderId);
        Console.WriteLine($"[VERIFY] Order removed from pending list: {isRemoved}");
    }

    private static async Task<(long baseId, long counterId)> ResolveTradingAccountsAsync(ILunoClient client, string baseAsset, string counterAsset)
    {
        Console.WriteLine($"📡 Resolving Account IDs for {baseAsset} and {counterAsset}...");
        var balances = await client.Accounts.GetBalancesAsync(new GetBalancesQuery());

        var baseAcc = balances.FirstOrDefault(b => b.Asset == baseAsset) 
            ?? throw new InvalidOperationException($"No {baseAsset} account found.");
        var counterAcc = balances.FirstOrDefault(b => b.Asset == counterAsset) 
            ?? throw new InvalidOperationException($"No {counterAsset} account found.");

        return (long.Parse(baseAcc.AccountId), long.Parse(counterAcc.AccountId));
    }

    private static async Task<MarketInfo> ResolveMarketMetadataAsync(ILunoClient client, string pair)
    {
        Console.WriteLine($"📡 Fetching market constraints for {pair}...");
        var markets = await client.Market.GetMarketsAsync(new GetMarketsQuery { Pairs = new[] { pair } });
        return markets.FirstOrDefault() ?? throw new InvalidOperationException($"Market info for {pair} not found.");
    }

    private static void HandleException(Exception ex)
    {
        if (ex is LunoAuthenticationException)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\n[Authentication Error] {ex.Message}");
        }
        else if (ex is LunoForbiddenException)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n[Permission Denied] {ex.Message}");
            Console.WriteLine("Note: Ensure your API key has 'Perm_W_Trade' enabled.");
        }
        else if (ex is LunoSecurityException)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"\n[Security Boundary Violated] {ex.Message}");
            Console.WriteLine("Resolution: This is a pre-flight SDK check. You must explicitly set 'AuthorizeWriteOperation = true' for this call.");
        }
        else
        {
            Console.WriteLine($"\n[Unexpected Error] {ex.Message}");
        }
        Console.ResetColor();
    }
}
