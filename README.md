# Luno.SDK

A high-performance .NET 10 SDK for the Luno API, built with Clean Architecture principles and the Microsoft Kiota toolchain.

## Quick Start

```csharp
using Luno.SDK;

// Initialize the standalone client
using var luno = new LunoClient();

// Stream market tickers asynchronously
await foreach (var heartbeat in luno.GetMarketHeartbeatAsync())
{
    Console.WriteLine($"{heartbeat.Pair}: {heartbeat.Price}");
}
```

## Demonstration Gallery

The project includes a CLI application demonstrating various integration patterns:

```bash
cd Luno.SDK.Cli
dotnet run
```

## Maintenance and Generation

This SDK utilizes an automated generation pipeline to ensure compliance with the Luno OpenAPI specification. If the specification is updated:

1.  **Update Specification**: Replace `docs/luno_api_spec.json` with the latest version.
2.  **Generate Client**: Execute `./generate-sdk.sh`. This script uses `patch-spec.js` to automatically apply necessary type corrections before generating the Kiota client.
3.  **Code Preservation**: Do not manually modify files in the `Luno.SDK.Infrastructure.Generated` project.

## License

This project is licensed under the Apache License, Version 2.0. See the [LICENSE](LICENSE) file for details.
