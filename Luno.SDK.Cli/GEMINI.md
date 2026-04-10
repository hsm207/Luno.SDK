# 🧪 Luno.SDK CLI Sanity Checks

This directory contains the demonstration gallery for the Luno SDK. To ensure all implemented endpoints and application-layer handlers are functioning correctly across the entire stack (from Kiota-generated infrastructure to Fluent extensions), use the following "One-Shot" validation command.

## 🚀 The "One-Shot" Validation Loop

Run this bash command from the repository root to sequentially execute all demonstration concepts. This command uses the command-line argument support in the CLI project to bypass the interactive menu, failing immediately if any concept encounters an error.

```bash
count=$(ls Luno.SDK.Cli/Concepts/Concept*.cs | wc -l); for ((i=1; i<=count; i++)); do dotnet run --project Luno.SDK.Cli -- "$i" || exit 1; done
```

### 🔍 What this validates:
1.  **Concept 01**: `GetTickersAsync` (Public API, IAsyncEnumerable streaming, and Auto-Mapping).
2.  **Concept 02**: Dependency Injection integration (`AddLunoClient`) and Service Provider resolution.
3.  **Concept 03**: `GetBalancesAsync` (Private API, Authentication Provider, User Secrets integration, and Complex Response Mapping).
4.  **Concept 04**: `GetTickerAsync` (Targeted Public API and Single-Entity Mapping).
5.  **Concept 05**: `TickerWrapping` (Pipeline Behaviors, Middleware Pattern, and DI Interception).
6.  **Concept 06**: `Limit Order Lifecycle` (Account & Market resolution, Quote calculation, Post Limit Order with Idempotency, and Stop Order).

### 🔑 Prerequisites
- Ensure API credentials (Key ID and Secret) are configured in **User Secrets** for the `Luno.SDK.Cli` project to allow Concept 03 and 06 to run non-interactively.
- A stable internet connection to reach the Luno API production endpoints.

---
