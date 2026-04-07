using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Luno.SDK;
using Luno.SDK.Trading;
using Luno.SDK.Application.Trading;
using Luno.SDK.Market;
using Luno.SDK.Account;
using Luno.SDK.Application.Account;

namespace Luno.SDK.Cli.Concepts;

/// <summary>
/// Provides a comprehensive demonstration of the Trading API lifecycle.
/// </summary>
public static class Concept06_Orders
{
    public static async Task RunAsync()
    {
        Console.WriteLine("\n=== Concept 06: Trading Lifecycle (Private API) ===");

        var options = LoadCredentials();
        var client = new LunoClient(options);

        try
        {
            // PART 1: Read-Only Operations (Listing)
            await DemonstrateOrderListingAsync(client);

            // PART 2: Write Operations (Trading)
            await DemonstrateTradingLifecycleAsync(client);
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

        return new LunoClientOptions
        {
            ApiKeyId = keyId,
            ApiKeySecret = keySecret
        };
    }

    private static async Task DemonstrateOrderListingAsync(ILunoClient client)
    {
        Console.WriteLine("\n--- Part 1: Order Listing (Read-Only) ---");

        // 1. Unfiltered Fetch
        Console.WriteLine("📡 Fetching ALL recent orders...");
        var allOrders = await client.Trading.ListOrdersAsync();
        Console.WriteLine($"[RESULT] Retrieved {allOrders.Count} orders.");

        foreach (var order in allOrders.Take(3))
        {
            Console.WriteLine($"- [{order.State}] {order.OrderId}: {order.Side} {order.LimitVolume} {order.Pair} @ {order.LimitPrice}");
        }

        // 2. Filtered Fetch (Pending)
        Console.WriteLine("\n📡 Fetching PENDING orders...");
        var pendingOrders = await client.Trading.ListOrdersAsync(state: OrderStatus.Pending);
        Console.WriteLine($"[RESULT] Found {pendingOrders.Count} pending orders.");

        // 3. Filtered Fetch (Specific Pair)
        Console.WriteLine("\n📡 Fetching XBTMYR orders...");
        var xbtOrders = await client.Trading.ListOrdersAsync(pair: "XBTMYR");
        Console.WriteLine($"[RESULT] Found {xbtOrders.Count} XBTMYR orders.");
    }

    private static async Task DemonstrateTradingLifecycleAsync(ILunoClient client)
    {
        Console.WriteLine("\n--- Part 2: Trading Lifecycle (Read/Write) ---");

        // A. Resolve Accounts and Market Constraints
        var (baseId, counterId) = await ResolveTradingAccountsAsync(client, "XBT", "MYR");
        var market = await ResolveMarketMetadataAsync(client, "XBTMYR");

        // B. Calculate Order Size (Optimal Quote)
        Console.WriteLine($"\n📡 Calculating optimal quote for XBTMYR at MinPrice ({market.MinPrice})...");
        var quote = await client.Trading.CalculateOrderSizeAsync(new CalculateOrderSizeQuery(
            Pair: "XBTMYR",
            Side: OrderSide.Buy,
            Spend: TradingAmount.InQuote(100),
            AtPrice: TradingPrice.InQuote(market.MinPrice)
        ));
        Console.WriteLine($"[QUOTE] Strategy: {quote.Volume} @ {quote.Price}");

        // C. Post the Order with Idempotency
        string clientOrderId = Guid.NewGuid().ToString();
        var command = quote.ToCommand(baseId, counterId, clientOrderId);

        Console.WriteLine("\n📡 Placing Limit Order...");
        var response = await client.Trading.PostLimitOrderAsync(command);
        Console.WriteLine($"[POST] Success! OrderId: {response.OrderId}");

        // D. Verify Idempotency Reconciliation
        Console.WriteLine("\n📡 Testing Idempotency (resending same command)...");
        var duplicateResponse = await client.Trading.PostLimitOrderAsync(command);
        Console.WriteLine($"[IDEMPOTENCY] Correctly reconciled to same OrderId: {duplicateResponse.OrderId == response.OrderId}");

        // E. Cancel the order
        Console.WriteLine("\n📡 Cancelling Order...");
        await client.Trading.StopOrderAsync(response.OrderId);
        Console.WriteLine("[STOP] Stop request dispatched.");

        // F. Final Verification
        var pending = await client.Trading.ListOrdersAsync(state: OrderStatus.Pending, pair: "XBTMYR");
        bool isRemoved = pending.All(o => o.OrderId != response.OrderId);
        Console.WriteLine($"[VERIFY] Order removed from pending list: {isRemoved}");
    }

    private static async Task<(long baseId, long counterId)> ResolveTradingAccountsAsync(ILunoClient client, string baseAsset, string counterAsset)
    {
        Console.WriteLine($"📡 Resolving Account IDs for {baseAsset} and {counterAsset}...");
        var balances = await client.Accounts.GetBalancesAsync();

        var baseAcc = balances.FirstOrDefault(b => b.Asset == baseAsset) 
            ?? throw new InvalidOperationException($"No {baseAsset} account found.");
        var counterAcc = balances.FirstOrDefault(b => b.Asset == counterAsset) 
            ?? throw new InvalidOperationException($"No {counterAsset} account found.");

        return (long.Parse(baseAcc.AccountId), long.Parse(counterAcc.AccountId));
    }

    private static async Task<MarketInfo> ResolveMarketMetadataAsync(ILunoClient client, string pair)
    {
        Console.WriteLine($"📡 Fetching market constraints for {pair}...");
        var markets = await client.Market.GetMarketsAsync(new[] { pair });
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
        else
        {
            Console.WriteLine($"\n[Unexpected Error] {ex.Message}");
        }
        Console.ResetColor();
    }
}
