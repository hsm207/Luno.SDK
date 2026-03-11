# RFC 002: Authentication and Private API Access

**Status:** Implemented (March 11, 2026)  
**Date:** 2026-03-04

## 1. Overview
This RFC proposes the implementation of HTTP Basic Authentication within the Luno SDK to enable secure access to private API endpoints. It introduces a custom Kiota `IAuthenticationProvider` and a `LunoAuthenticationOption` to dynamically manage authentication on a per-request basis.

## 2. Motivation
Currently, the Luno SDK is limited to public market data endpoints. To fulfill the SDK's purpose, it must support private operations. Additionally, by authenticating "public" requests, developers can leverage their per-account rate limit buckets.

## 3. Future State
Developers can provide credentials and opt into "Always Authenticate" mode. The SDK will **fail fast** if a request requires authentication but no credentials are provided. Server-side errors (401, 403) will be translated into strongly-typed exceptions via a dedicated error-handling decorator.

```csharp
// Standalone usage
var client = new LunoClient(new LunoClientOptions 
{ 
    ApiKeyId = "key_id",
    ApiKeySecret = "key_secret"
});

// Explicit semantic error handling
try 
{
    var balances = await client.Accounts.GetBalancesAsync();
}
catch (LunoUnauthorizedException)
{
    // Handle invalid keys
}
```

## 4. Goals & Non-Goals
- **Goals:**
    - Support standard HTTP Basic Authentication.
    - Introduce `LunoAuthenticationOption` for fine-grained per-request auth control.
    - **Fail fast** if credentials are missing for a required authenticated request.
    - **Layered Error Handling**: Introduce `LunoErrorHandlingAdapter` to translate 401/403 errors into domain exceptions.
    - **Safe Financial Parsing**: Use `InvariantCulture` for all string-to-decimal conversions.
    - **Accurate Semantics**: Return `IReadOnlyList<T>` for snapshot-based API responses.
- **Non-Goals:**
    - Support for OAuth2 (Deprecated).
    - Support for HMAC-SHA256 signatures.

## 5. Proposed Technical Design

### High-Level Architecture: The Layered Slay
To maintain the **Single Responsibility Principle (SRP)** and ensure high-fidelity telemetry, the SDK will use a nested decorator pattern for the Kiota `IRequestAdapter`.

**The Request Pipeline:**
1.  **`LunoClient`**: Entry point for the developer.
2.  **`LunoTelemetryAdapter`**: Measures total execution time and outcome of the semantic operation.
3.  **`LunoErrorHandlingAdapter` (NEW)**: Intercepts `ApiException` and translates 401/403 codes into `LunoUnauthorizedException` and `LunoForbiddenException`.
4.  **`KiotaRequestAdapter`**: Handles the raw HTTP communication.

### Optimization: Pre-computed Credentials
`LunoAuthenticationProvider` will pre-compute the Base64-encoded `Authorization` header in its constructor to minimize allocations in high-frequency scenarios.

### Domain Entity: Balance
The `Balance` entity in the `Core` layer will provide a high-fidelity representation of an account balance.

| Domain Field | API Source Field | Type | Description |
| :--- | :--- | :--- | :--- |
| `AccountId` | `account_id` | `string` | Unique identifier for the account. |
| `Asset` | `asset` | `string` | Currency code (e.g., "XBT", "ETH"). |
| `Available` | `balance` | `decimal` | Amount available to send or trade. |
| `Reserved` | `reserved` | `decimal` | Amount locked by Luno. |
| `Unconfirmed` | `unconfirmed` | `decimal` | Amount awaiting verification. |
| `Name` | `name` | `string` | User-defined name for the account. |
| `Total` | N/A | `decimal` | Calculated property: `Available + Reserved`. |

**Parsing Safety Mandate**: `AccountMapper` MUST use `decimal.Parse(value, CultureInfo.InvariantCulture)` to ensure cross-locale stability.

### Account Client Semantics
`ILunoAccountClient.GetBalancesAsync()` will return `Task<IReadOnlyList<Balance>>`. This accurately represents that the Luno `/api/1/balance` endpoint returns a single static snapshot of all balances, not a paginated stream.

## 6. Behavioral Specifications

### Scenario: Credentials Missing for Required Auth
- **Given:** A `LunoClient` WITHOUT credentials.
- **When:** A request is tagged with `LunoAuthenticationOption { RequiresAuthentication = true }`.
- **Then:** Throw `LunoAuthenticationException` immediately without making an HTTP call.

### Scenario: Invalid Credentials from Server (401)
- **Given:** A `LunoClient` with incorrect keys.
- **When:** `client.Accounts.GetBalancesAsync()` is called.
- **Then:** The `LunoErrorHandlingAdapter` must throw `LunoUnauthorizedException`, and telemetry must record this semantic exception as the failure reason.

### Scenario: Precision Across Cultures
- **Given:** A system locale using `,` as a decimal separator (e.g., `de-DE`).
- **When:** `AccountMapper` parses "1000.50".
- **Then:** The resulting `decimal` must be exactly `1000.50m`.

## 7. Definition of Done
- **100% Test Pass**: Unit tests for the provider, mapper (with culture variants), and error handling must pass.
- **Architecture Compliance Test**: Verify that all `LunoAccountClient` methods apply the authentication option.
- **No Secrets in Logs**: The `Authorization` header must not be captured by telemetry.
- **TDD Mandate**: Verification must favor behavioral outcomes.
