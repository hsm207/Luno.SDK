using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Luno.SDK;
using Luno.SDK.Trading;
using Luno.SDK.Application.Trading;

namespace Luno.SDK.Cli.Concepts;

public static class Concept06_Orders
{
    public static async Task RunAsync()
    {
        Console.WriteLine("\n=== Concept 06: List Orders (Private API) ===");

        // Try to load credentials from User Secrets first
        var config = new ConfigurationBuilder()
            .AddUserSecrets<Program>()
            .Build();

        string? keyId = config["Luno:ApiKeyId"];
        string? keySecret = config["Luno:ApiKeySecret"];

        if (string.IsNullOrWhiteSpace(keyId))
        {
            Console.WriteLine("No credentials found in User Secrets.");
            Console.WriteLine("Enter your API Key ID: ");
            keyId = Console.ReadLine();

            if (!string.IsNullOrWhiteSpace(keyId))
            {
                Console.WriteLine("Enter your API Key Secret: ");
                keySecret = Console.ReadLine();
            }
        }
        else
        {
            Console.WriteLine("Loaded API credentials from User Secrets! 💅");
        }

        var options = new LunoClientOptions
        {
            ApiKeyId = keyId,
            ApiKeySecret = keySecret
        };

        var client = new LunoClient(options);

        try
        {
            // 1. Unfiltered Fetch
            Console.WriteLine("\n--- Step 1: Fetching ALL recent orders (Unfiltered) ---");
            var allOrders = await client.Trading.ListOrdersAsync();

            Console.WriteLine($"Successfully retrieved {allOrders.Count} orders.");
            foreach (var order in allOrders.Take(5))
            {
                Console.WriteLine($"- [{order.State}] {order.OrderId}: {order.Type} {order.LimitVolume} {order.Pair} @ {order.LimitPrice} (Created: {order.CreationTimestamp})");
            }

            // 2. Filtered Fetch (Pending)
            Console.WriteLine("\n--- Step 2: Fetching PENDING orders ---");
            var pendingOrders = await client.Trading.ListOrdersAsync(state: OrderStatus.Pending);

            Console.WriteLine($"Found {pendingOrders.Count} pending orders.");
            foreach (var order in pendingOrders)
            {
                Console.WriteLine($"- {order.OrderId}: {order.Type} {order.LimitVolume} {order.Pair} @ {order.LimitPrice}");
            }

            // 3. Filtered Fetch (Pair)
            Console.WriteLine("\n--- Step 3: Fetching XBTMYR orders ---");
            var xbtOrders = await client.Trading.ListOrdersAsync(pair: "XBTMYR");

            Console.WriteLine($"Found {xbtOrders.Count} XBTMYR orders.");
        }
        catch (LunoAuthenticationException ex)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[Authentication Blocked] {ex.Message}");
            Console.ResetColor();
        }
        catch (LunoUnauthorizedException ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[Unauthorized (401)] {ex.Message}");
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An unexpected error occurred: {ex.Message}");
        }
    }
}
