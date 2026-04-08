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
