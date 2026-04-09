# Security Architecture: Explicit Intent Model

The Luno SDK follows a "Secure by Design" philosophy, enforcing explicit call-site intent for all sensitive operations. This prevents common errors like accidental order placement or unintended credential leakage on public endpoints.

## Core Principles

1.  **Implicit Deny**: All write operations are blocked by default.
2.  **Explicit Intent**: Developers must opt-in to write operations per-request.
3.  **Privacy First**: Public endpoints do not send authorization headers unless requested (avoiding unnecessary IP/Account tracking by proxies or the Luno edge).
4.  **Pre-Flight Enforcement**: Boundary checks occur locally before any data is sent over the network.

---

## Security Behavior Matrix

The `LunoAuthenticationProvider` (The Sentry) enforces the following rules based on endpoint classification and request options:

| Endpoint Type | Write Intent Flag | Public Auth Flag | Behavior |
| :--- | :--- | :--- | :--- |
| **Write** (`POST/PUT/DELETE`) | `false` (default) | Any | **BLOCKED** (`LunoSecurityException`) |
| **Write** (`POST/PUT/DELETE`) | `true` | Any | **ALLOWED** (Auth Header attached) |
| **Private Read** (`GET`) | Any | Any | **ALLOWED** (Auth Header attached) |
| **Public** (`GET`) | Any | `false` (default) | **ALLOWED** (No Auth Header) |
| **Public** (`GET`) | Any | `true` | **ALLOWED** (Auth Header attached) |

> [!IMPORTANT]
> **Safe Harbor Principle**: Over-authorizing a request (e.g., setting `AuthorizeWriteOperation = true` on a `ListBalances` call) is tolerated and ignored to reduce developer friction.

---

## Key Types

### `LunoRequestOptions`
The DTO used to declare intent at the call site.

- `AuthorizeWriteOperation`: Mandatory `true` for all write calls.
- `AuthenticatePublicEndpoint`: Optional `true` to include credentials for rate-limit boosting on public endpoints.

### `LunoSecurityContext`
Used by the SDK internally to propagate intent from the Application Core to the Infrastructure Layer via `AsyncLocal<T>`.

### `LunoSecurityException`
Thrown when a security boundary is reached without proper intent.
- **Message**: Descriptive help naming the endpoint and missing flag.
- **Resolution**: Update the call site to include the required option.

---

## Example: Authorizing a Trade

```csharp
// 🚨 This will throw LunoSecurityException
await client.Trading.PostLimitOrderAsync(command);

// ✅ This succeeds
await client.Trading.PostLimitOrderAsync(command, opt => 
{
    opt.AuthorizeWriteOperation = true;
});
```

## Example: Authenticating a Public Request

```csharp
// No headers sent (Privacy mode)
var market = await client.Market.GetMarketsAsync(new[] { "XBTMYR" });

// Headers sent for rate limit boosting
var market = await client.Market.GetMarketsAsync(new[] { "XBTMYR" }, opt => 
{
    opt.AuthenticatePublicEndpoint = true;
});
```

---

## In-Memory Credential Custody
Because the Luno API mandates Basic Authentication, the SDK must eventually build the `"Basic dXNlci..."` string and attach it to the HTTP request. This managed string will inevitably enter the .NET Garbage Collector pool (Gen 0).

The SDK explicitly adopts a "minimize exposure" posture rather than claiming absolute memory encryption:
1.  **Dependency Inverted Custody**: The SDK does not enforce how credentials are stored. It relies on the lightweight `ILunoCredentialProvider` interface.
2.  **Late Materialization**: The Authentication pipeline delays fetching credentials and combining them until the absolute last millisecond before the request is issued.
3.  **Explicit Zeroization**: Intermediate memory buffers (`Span<byte>`, `char[]`) used to combine the Key and Secret are rigorously wiped using `CryptographicOperations.ZeroMemory()`.

### Building Secure Providers

If memory dump exposure is a primary threat model for your application, you must implement a robust `ILunoCredentialProvider`.

**Windows Environments**:
We recommend implementing a provider wrapped around Windows DPAPI (`ProtectedMemory`) or the Windows Credential Manager.

**Linux / Container Environments**:
Pass credentials in via Environment Variables or orchestrator secret mounts (e.g., Kubernetes Secrets), loading them on demand within the provider.

> [!WARNING]
> **Avoid Caching Secrets**
> Do not hold the raw `LunoCredentials` struct in a singleton or static field inside your provider. Yield it fresh when `GetCredentialsAsync()` is invoked. 

### Development / Convenience Mode

For ease of setup, you can inject credentials directly into the client options using native .NET configurations:

```csharp
var options = new LunoClientOptions().WithCredentials("Api-Key-Id", "Api-Key-Secret");
```

> [!CAUTION]
> **Least Hardened Posture**
> Using `.WithCredentials()` causes the underlying plain text strings to be held indefinitely on the Large Object Heap (if options are injected as a Singleton). This is convenient for configuration and UI apps, but renders the credentials fully visible in a memory dump.

### Telemetry & Logging Safety

The Luno SDK takes an aggressive "Sober Posture" toward observability. To prevent sensitive authentication material from leaking into infrastructure logs (e.g., Application Insights, Elastic, or console logs):

1. **Header Redaction**: The SDK explicitly redacts the `Authorization` header in the internal `IHttpClientFactory` logging pipeline. Even under `Trace` logging levels, the header value is replaced with `*`.
2. **Behavior Metadata**: Metadata collected by `TelemetryPipelineBehavior` is strictly limited to non-sensitive operation names and latencies.
3. **Internal Error Handlers**: Exception loggers within the SDK are audited to ensure they do not dump raw `RequestInformation` objects that might contain sensitive material.

> [!IMPORTANT]
> **Defense in Depth**
> While modern .NET versions (9.0+) redact headers by default, the Luno SDK implements this explicitly to ensure your secrets remain secure even if framework defaults are overridden or the runtime is downgraded.
