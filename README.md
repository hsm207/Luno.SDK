# Luno.SDK

A modern .NET 10 SDK for the Luno API, built with Clean Architecture principles and the Microsoft Kiota toolchain.

## 🛡️ Security-First Architecture

This SDK employs a **Hardcore Explicit Intent** model. To prevent accidental financial loss or unauthorized state changes, all write operations (creating orders, cancelling orders, etc.) require an explicit per-request opt-in:

- **Mandatory Write Intent**: POST/PUT/DELETE operations will fail pre-flight unless `AuthorizeWriteOperation = true` is set.
- **Privacy-First Public API**: Public endpoints do not send headers unless `AuthenticatePublicEndpoint = true` is explicitly requested (to protect IP anonymity/privacy).
- **Secure Fail-Safe**: A `LunoSecurityException` is thrown *before* request signing or network transmission if intent is missing.

## Quick Start

```csharp
using Luno.SDK;

// Initialize the standalone client
var luno = new LunoClient();

// Stream market tickers asynchronously
await foreach (var ticker in luno.GetTickersAsync())
{
    Console.WriteLine($"{ticker.Pair}: {ticker.Price}");
}

// Safely calculate a Limit Order to spend exactly 100 MYR on Bitcoin
var quote = await luno.Trading.CalculateOrderSizeAsync(new CalculateOrderSizeQuery(
    Pair: "XBTMYR",
    Side: OrderSide.Buy,
    Spend: TradingAmount.InQuote(100m)
));

// Map the strict mathematical quote seamlessly into a command
var command = quote.ToCommand(baseAccountId: 12345, counterAccountId: 67890);

// Explicitly authorize this specific write operation
await luno.Trading.PostLimitOrderAsync(command, opt => opt.AuthorizeWriteOperation = true);
```

## Demonstration Gallery

The project includes a CLI application demonstrating various integration patterns:

```bash
cd Luno.SDK.Cli
dotnet run
```

## Maintenance and Generation

This SDK utilizes an automated generation pipeline to ensure compliance with the Luno OpenAPI specification. If the specification is updated:

1.  **Update Specification**: Download the latest `openapi.json` from the [Luno API Portal](https://www.luno.com/en/developers/api) and replace `docs/luno_api_spec.json`.
2.  **Generate Client**: Execute `./scripts/generate-sdk.sh`. This script automatically applies necessary type corrections before generating the Kiota client.
3.  **Code Preservation**: Do not manually modify files in the `Luno.SDK.Infrastructure.Generated` project.

## License

This project is licensed under the Apache License, Version 2.0. See the [LICENSE](LICENSE) file for details.
