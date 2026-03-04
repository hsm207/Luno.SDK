using System;
using System.Threading.Tasks;
using Luno.SDK;

namespace Luno.SDK.Cli.Concepts;

public static class Concept03_AccountBalances
{
    public static async Task RunAsync()
    {
        Console.WriteLine("\n=== Concept 03: Account Balances (Private API) ===");

        // Note: For this demonstration to actually hit Luno's private API, you would need
        // valid API keys. We simulate what happens if you provide keys (or leave them blank).

        Console.WriteLine("Enter your API Key ID (or press enter to skip and see the fail-fast behavior): ");
        var keyId = Console.ReadLine();

        string? keySecret = null;
        if (!string.IsNullOrWhiteSpace(keyId))
        {
            Console.WriteLine("Enter your API Key Secret: ");
            keySecret = Console.ReadLine();
        }

        var options = new LunoClientOptions
        {
            ApiKeyId = keyId,
            ApiKeySecret = keySecret
        };

        var client = new LunoClient(options);

        try
        {
            Console.WriteLine("Fetching balances...");
            var balances = await client.Accounts.GetBalancesAsync();

            Console.WriteLine($"Successfully retrieved {balances.Count} balances:");
            foreach (var balance in balances)
            {
                if (balance.Total > 0)
                {
                    Console.WriteLine($"- {balance.Asset}: {balance.Total} (Available: {balance.Available}, Reserved: {balance.Reserved})");
                }
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
