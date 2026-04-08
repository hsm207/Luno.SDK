using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Luno.SDK;
using Luno.SDK.Application.Account;
using System.Linq;

namespace Luno.SDK.Cli.Concepts;

public static class Concept03_AccountBalances
{
    public static async Task RunAsync()
    {
        Console.WriteLine("\n=== Concept 03: Account Balances (Private API) ===");

        // Try to load credentials from User Secrets first
        var config = new ConfigurationBuilder()
            .AddUserSecrets<Program>()
            .Build();

        string? keyId = config["Luno:ReadOnly:ApiKeyId"];
        string? keySecret = config["Luno:ReadOnly:ApiKeySecret"];

        if (string.IsNullOrWhiteSpace(keyId))
        {
            Console.WriteLine("No credentials found in User Secrets.");
            Console.WriteLine("Enter your API Key ID (or press enter to skip and see the fail-fast behavior): ");
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
            Console.WriteLine("\n--- Step 1: Fetching ALL balances (Unfiltered) ---");
            var allBalances = await client.Accounts.GetBalancesAsync(new GetBalancesQuery());

            Console.WriteLine($"Successfully retrieved {allBalances.Count} balances:");
            foreach (var balance in allBalances.Where(b => b.Total > 0))
            {
                Console.WriteLine($"- {balance.Asset}: {balance.Total} (Available: {balance.Available})");
            }

            // 2. Filtered Fetch
            Console.WriteLine("\n--- Step 2: Fetching specific balances (Filtered: XBT, ETH) ---");
            var filteredBalances = await client.Accounts.GetBalancesAsync(new GetBalancesQuery { Assets = new[] { "XBT", "ETH" } });

            Console.WriteLine($"Successfully retrieved {filteredBalances.Count} filtered balances:");
            foreach (var balance in filteredBalances)
            {
                Console.WriteLine($"- {balance.Asset}: {balance.Total} (Available: {balance.Available})");
            }
        }
        catch (LunoAuthenticationException ex)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[Authentication Blocked] {ex.Message}");
            Console.ResetColor();
            Console.WriteLine("-> The SDK successfully prevented the request from going to the network because keys were missing.");
        }
        catch (LunoUnauthorizedException ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[Unauthorized (401)] {ex.Message}");
            Console.ResetColor();
        }
        catch (LunoForbiddenException ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[Forbidden (403)] {ex.Message}");
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An unexpected error occurred: {ex.Message}");
        }
    }
}
