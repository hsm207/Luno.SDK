# Luno.SDK 🏛️💎

A high-performance .NET 10 SDK for Luno, built with Clean Architecture and Kiota.

## 🚀 Quick Start

```csharp
using Luno.SDK;

using var luno = new LunoClient();

await foreach (var heartbeat in luno.GetMarketHeartbeatAsync())
{
    Console.WriteLine($"{heartbeat.Pair}: {heartbeat.Price}");
}
```

## 📖 Executable Documentation

Run the sample gallery to see the SDK in action:

```bash
cd Luno.SDK.Cli
dotnet run
```

## 🤖 Maintenance & Generation

This SDK uses an automated pipeline to keep the infrastructure spec-compliant without manual maintenance. If the Luno API changes or a new version is released:

1.  **Update the Bible**: Replace `docs/luno_api_spec.json` with the latest version.
2.  **Run the Engine**: Execute `./generate-sdk.sh`. This script uses `patch-spec.js` to automatically fix Luno's quirky types (`int64`, `decimal`) before generating the Kiota client.
3.  **No Manual Mud**: Never edit files in the `Infrastructure.Generated` project directly!

## 📜 License

Apache-2.0
