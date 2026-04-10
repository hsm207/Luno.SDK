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
    private static readonly DemoScenario Scenario = new(
        Pair: "XBTMYR",
        BaseAsset: "XBT",
        CounterAsset: "MYR",
        SpendAmount: 100m,
        Side: OrderSide.Buy
    );

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

    private static async Task DemonstrateLimitOrderLifecycleAsync(ILunoClient client)
    {
        PrintScenarioHeader();

        await PrintInitialPendingOrdersAsync(client);

        var (baseAccountId, counterAccountId) =
            await ResolveTradingAccountsAsync(client, Scenario.BaseAsset, Scenario.CounterAsset);

        var market = await ResolveMarketMetadataAsync(client, Scenario.Pair);
        var quote = await CalculateAndPrintQuoteAsync(client, market);

        var postOrderCommand = BuildPostOrderCommand(quote, baseAccountId, counterAccountId);
        var orderId = await PlaceOrderAsync(client, postOrderCommand);

        await VerifyOrderListedAsync(client, orderId);
        await VerifyIdempotencyAsync(client, postOrderCommand, orderId);
        await CancelOrderAsync(client, orderId);
        await VerifyOrderRemovedAsync(client, orderId);
    }

    private static void PrintScenarioHeader()
    {
        Console.WriteLine(
            $"\n--- Starting Limit Order Lifecycle Demonstration for {Scenario.Pair} ---");
    }

    private static LunoClientOptions LoadCredentials()
    {
        var config = new ConfigurationBuilder()
            .AddUserSecrets<Program>()
            .Build();

        string? keyId = config["Luno:Trading:ApiKeyId"] ?? config["Luno:ReadOnly:ApiKeyId"];
        string? keySecret = config["Luno:Trading:ApiKeySecret"] ?? config["Luno:ReadOnly:ApiKeySecret"];

        if (string.IsNullOrWhiteSpace(keyId) || string.IsNullOrWhiteSpace(keySecret))
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

    private static async Task PrintInitialPendingOrdersAsync(ILunoClient client)
    {
        Console.WriteLine($"\n📡 Initial State: Fetching pending orders for {Scenario.Pair}...");

        var pendingOrders = await ListPendingOrdersAsync(client);

        Console.WriteLine($"[STATE] Current Pending Count: {pendingOrders.Count}");
    }

    private static async Task<IReadOnlyList<OrderDetailsResponse>> ListPendingOrdersAsync(ILunoClient client)
    {
        return await client.Trading.ListOrdersAsync(new ListOrdersQuery
        {
            State = OrderStatus.Pending,
            Pair = Scenario.Pair
        });
    }

    private static async Task<(long baseId, long counterId)> ResolveTradingAccountsAsync(
        ILunoClient client,
        string baseAsset,
        string counterAsset)
    {
        Console.WriteLine($"📡 Resolving Account IDs for {baseAsset} and {counterAsset}...");

        var balances = await client.Accounts.GetBalancesAsync(new GetBalancesQuery());

        var baseAccount = balances.FirstOrDefault(b => b.Asset == baseAsset)
            ?? throw new InvalidOperationException($"No {baseAsset} account found.");

        var counterAccount = balances.FirstOrDefault(b => b.Asset == counterAsset)
            ?? throw new InvalidOperationException($"No {counterAsset} account found.");

        return (long.Parse(baseAccount.AccountId), long.Parse(counterAccount.AccountId));
    }

    private static async Task<MarketInfo> ResolveMarketMetadataAsync(ILunoClient client, string pair)
    {
        Console.WriteLine($"📡 Fetching market constraints for {pair}...");

        var markets = await client.Market.GetMarketsAsync(new GetMarketsQuery
        {
            Pairs = new[] { pair }
        });

        return markets.FirstOrDefault()
            ?? throw new InvalidOperationException($"Market info for {pair} not found.");
    }

    private static async Task<OrderQuote> CalculateAndPrintQuoteAsync(
        ILunoClient client,
        MarketInfo market)
    {
        Console.WriteLine(
            $"\n📡 Strategy: Calculating optimal quote at MinPrice ({market.MinPrice:N2} {market.CounterCurrency})...");

        var quote = await client.Trading.CalculateOrderSizeAsync(
            new CalculateOrderSizeQuery(
                Pair: Scenario.Pair,
                Side: Scenario.Side,
                Spend: TradingAmount.InQuote(Scenario.SpendAmount),
                AtPrice: TradingPrice.InQuote(market.MinPrice)
            ));

        PrintQuote(quote, market);

        return quote;
    }

    private static void PrintQuote(OrderQuote quote, MarketInfo market)
    {
        string volumeFormat = $"N{market.VolumeScale}";
        string priceFormat = $"N{market.PriceScale}";
        string sideAction = quote.Side == OrderSide.Buy ? "BUY" : "SELL";
        string financialAction = quote.Side == OrderSide.Buy ? "PAY" : "RECEIVE";

        Console.WriteLine(
            $"[QUOTE] ACTION:    {sideAction} {quote.Volume.ToString(volumeFormat)} {market.BaseCurrency}");
        Console.WriteLine(
            $"[QUOTE] PRICE:     {quote.Price.ToString(priceFormat)} {market.CounterCurrency} per {market.BaseCurrency}");
        Console.WriteLine(
            $"[QUOTE] FINANCIAL: You will {financialAction} {quote.GrossQuoteValue:N2} {quote.QuoteCurrency} (estimated, excl. fees)");
    }

    private static PostLimitOrderCommand BuildPostOrderCommand(
        OrderQuote quote,
        long baseAccountId,
        long counterAccountId)
    {
        string clientOrderId = Guid.NewGuid().ToString();

        return quote.ToCommand(baseAccountId, counterAccountId, clientOrderId) with
        {
            Options = new LunoRequestOptions
            {
                AuthorizeWriteOperation = true
            }
        };
    }

    private static async Task<string> PlaceOrderAsync(
        ILunoClient client,
        PostLimitOrderCommand command)
    {
        Console.WriteLine("\n📡 Action: Placing Limit Order (requires explicit write intent)...");

        var response = await client.Trading.PostLimitOrderAsync(command);

        Console.WriteLine($"[POST] Success! OrderId: {response.OrderId}");

        return response.OrderId;
    }

    private static async Task VerifyOrderListedAsync(
        ILunoClient client,
        string orderId)
    {
        Console.WriteLine(
            $"\n📡 Verification: Fetching pending orders for {Scenario.Pair} after placement...");

        var pendingOrders = await ListPendingOrdersAsync(client);
        bool isListed = pendingOrders.Any(order => order.OrderId == orderId);

        Console.WriteLine($"[VERIFY] Order {orderId} found in pending list: {isListed}");
    }

    private static async Task VerifyIdempotencyAsync(
        ILunoClient client,
        PostLimitOrderCommand command,
        string expectedOrderId)
    {
        Console.WriteLine(
            "\n📡 Idempotency: Resending same command (requires explicit write intent)...");

        var duplicateResponse = await client.Trading.PostLimitOrderAsync(command);

        Console.WriteLine(
            $"[IDEMPOTENCY] Reconciled to same OrderId: {duplicateResponse.OrderId == expectedOrderId}");
    }

    private static async Task CancelOrderAsync(
        ILunoClient client,
        string orderId)
    {
        Console.WriteLine("\n📡 Cleanup: Cancelling Order (requires explicit write intent)...");

        var stopCommand = new StopOrderCommand
        {
            OrderId = orderId,
            Options = new LunoRequestOptions
            {
                AuthorizeWriteOperation = true
            }
        };

        await client.Trading.StopOrderAsync(stopCommand);

        Console.WriteLine("[STOP] Stop request dispatched.");
    }

    private static async Task VerifyOrderRemovedAsync(
        ILunoClient client,
        string orderId)
    {
        Console.WriteLine(
            $"\n📡 Final State: Fetching pending orders for {Scenario.Pair} after cancellation...");

        var pendingOrders = await ListPendingOrdersAsync(client);
        bool isRemoved = pendingOrders.All(order => order.OrderId != orderId);

        Console.WriteLine($"[VERIFY] Order removed from pending list: {isRemoved}");
    }

    private static void HandleException(Exception ex)
    {
        var (color, label, resolution) = ex switch
        {
            LunoAuthenticationException => (ConsoleColor.Yellow, "[Local Configuration Error]", null),
            LunoUnauthorizedException   => (ConsoleColor.Red,    "[Server Rejection: Invalid Credentials]", null),
            LunoForbiddenException      => (ConsoleColor.Red,    "[Permission Denied]", "Note: Ensure your API key has 'Perm_W_Trade' enabled."),
            LunoSecurityException       => (ConsoleColor.Cyan,   "[Security Boundary Violated]", "Resolution: This is a pre-flight SDK check. You must explicitly set 'AuthorizeWriteOperation = true' for this call."),
            _                           => (Console.ForegroundColor, "[Unexpected Error]", null)
        };

        Console.ForegroundColor = color;
        Console.WriteLine($"\n{label} {ex.Message}");
        if (resolution != null) Console.WriteLine(resolution);
        Console.ResetColor();
    }

    private sealed record DemoScenario(
        string Pair,
        string BaseAsset,
        string CounterAsset,
        decimal SpendAmount,
        OrderSide Side);
}
