# RFC 005: Single-Pair Ticker Retrieval

**Status:** Implemented  
**Date:** 2026-03-11

## 1. Overview
This RFC proposes adding a surgical `GetTickerAsync(string pair)` method to the `ILunoMarketClient` and exposing it via the fluent `LunoClient` API. This allows consumers to fetch the market state of a single trading pair (e.g., "XBTMYR") without the overhead of retrieving all tickers.

## 2. Motivation
Currently, `ILunoMarketClient` only provides `GetTickersAsync`, which returns a list of all available tickers. For applications focused on a specific pair (like a "price-watch" app or a targeted trading bot), this is inefficient and forces unnecessary data processing on the client side. A targeted method improves **Developer Experience (DX)** and **Performance**.

## 3. Future State
Developers can access a specific ticker with a single, clear call:
```csharp
var ticker = await client.GetTickerAsync("XBTZAR"); 
Console.WriteLine($"Price: {ticker.Price} ({ticker.IsActive})");
```

## 4. Goals & Non-Goals
- **Goals:**
    - Provide a single-pair retrieval method in the Market Client.
    - Leverage the existing, high-fidelity `Ticker` entity.
    - **Pre-flight Validation:** Fail fast by throwing a `LunoValidationException` if the `pair` parameter is null, empty, or whitespace.
    - **Compiler Enforcement:** Leverage Nullable Reference Types (NRT) and `WarningsAsErrors` to catch null assignments at compile-time.
    - **High-Fidelity Error Mapping:** Leverage the existing **RFC 004 Unified Domain Exception Hierarchy** to return semantic errors (`LunoUnauthorizedException`, `LunoRateLimitException`, `LunoValidationException`, etc.).
    - **High-Fidelity Demonstration:** Add a new concept to the `Luno.SDK.Cli` gallery to demonstrate targeted ticker retrieval.
- **Non-Goals:**
    - Implementing client-side ticker normalization or casing logic (Delegated to User).
    - Implementing client-side ticker length or format validation (Delegated to the API).

## 5. Proposed Technical Design
### High-Level Architecture
```mermaid
sequenceDiagram
    %% <!-- MachineTruth: SingleTickerRetrievalPipeline -->
    participant User as SDK User
    participant App as Application Layer
    participant Infra as Infrastructure Layer
    participant Kiota as Kiota Engine
    participant API as Luno API

    User->>App: GetTickerAsync("XBTZAR", ct)
    App->>Infra: GetTickerAsync("XBTZAR", ct)
    Infra->>Kiota: ticker.GetAsync(q => q.QueryParameters.Pair = "XBTZAR", ct)
    Kiota->>API: GET /api/1/ticker?pair=XBTZAR
    alt Success (200)
        API-->>Kiota: 200 OK (Ticker JSON)
        Kiota-->>Infra: GetTickerResponse (Generated)
        Infra->>App: Ticker (Domain Entity)
        App->>User: TickerResponse (Application DTO)
    else Invalid Pair (400)
        API-->>Kiota: 400 Bad Request (ErrInvalidMarketPair)
        Kiota-->>Infra: ApiException
        Infra-->>App: throw LunoValidationException
        App-->>User: throw LunoValidationException
    end
```

### Public API Changes
- **Modified `ILunoMarketClient`**:
    - `Task<Ticker> GetTickerAsync(string pair, CancellationToken ct = default);`
- **Application Layer**:
    - `GetTickerQuery`: A record containing the pair string.
    - `GetTickerHandler`: Orchestrates the retrieval and maps the entity to `TickerResponse`.

### Implementation Realities
#### 1. DTO Discrepancy (Resolved via Patch)
The singular and bulk endpoints return different DTO types (`GetTickerResponse` vs `Ticker`). However, both have been patched in `patch-spec.js` to ensure the `timestamp` field utilizes **`int64`** to prevent overflow. The Infrastructure layer must map these distinct DTOs to the unified **`Luno.SDK.Core.Market.Ticker`** domain entity.

#### 2. Compiler Mandate (Null-Free Lifestyle)
The project strictly enforces Nullable Reference Types and treats warnings as build errors via `.csproj` configuration. This ensures that null assignments to the `pair` parameter are caught during development.

### Phased Implementation
- **Phase 1: Core Interface**
    - **Description:** Update the Market Client interface to support single-pair retrieval.
    - **Core Changes:** Modify `ILunoMarketClient.cs`.
- **Phase 2: Infrastructure Implementation**
    - **Description:** Implement high-fidelity mapping and client logic.
    - **Core Changes:** 
        - Update `MarketMapper.cs` to support mapping from `GetTickerResponse`.
        - Implement `GetTickerAsync` in `LunoMarketClient.cs` using the query parameter pattern.
- **Phase 3: Application Orchestration**
    - **Description:** Implement the handler and fluent extension using high-fidelity mapping patterns.
    - **Core Changes:** 
        - Create `GetTickerHandler.cs` leveraging the extracted `TickerResponse` and `MarketMappingExtensions.ToResponse()`.
        - Add `GetTickerAsync` extension method to `LunoMarketExtensions.cs`.
    - **Locations:** `Luno.SDK.Application/Market/GetTicker.cs`, `Luno.SDK.Application/Market/LunoMarketExtensions.cs`
- **Phase 4: CLI Demonstration**
    - **Description:** Add a new high-fidelity demonstration to the CLI gallery.
    - **Core Changes:** Create `Concept04_SingleTicker.cs` and update `Program.cs`.

## 6. Behavioral Specifications
### Successful Ticker Retrieval
- **Given:**
    - A valid trading pair identifier "XBTZAR".
- **When:**
    - `GetTickerAsync("XBTZAR")` is called.
- **Then:**
    - The SDK returns the `Ticker` record for the requested pair.
    - Telemetry is emitted with the `luno.market.get_ticker` signal.

### Handling Null or Empty Pair
- **Given:**
    - A null, empty, or whitespace string provided as the `pair` parameter.
- **When:**
    - `GetTickerAsync(pair)` is called.
- **Then:**
    - The SDK throws a `LunoValidationException` immediately without making an API request.

### Handling Invalid Pair (400)
- **Given:**
    - An invalid trading pair identifier "NOTAFX".
- **When:**
    - `GetTickerAsync("NOTAFX")` is called.
- **Then:**
    - The SDK throws a `LunoValidationException` (mapped via RFC 004).
    - The original `ApiException` is preserved as the `InnerException`.

### Handling Rate Limits (429) with Retry Info
- **Given:**
    - A user has exceeded the rate limit, and the API returns 429 with `Retry-After: 45`.
- **When:**
    - `GetTickerAsync("XBTMYR")` is called.
- **Then:**
    - The SDK throws a `LunoRateLimitException` where `RetryAfter` is equal to 45 seconds.

### Handling Market Maintenance (503)
- **Given:**
    - The Luno API returns a 503 status code with `ErrUnderMaintenance`.
- **When:**
    - `GetTickerAsync("XBTMYR")` is called.
- **Then:**
    - The SDK throws a `LunoMarketStateException`.

### Handling Permission Denied (403)
- **Given:**
    - A trading pair "XBTNGN" that is not enabled for the authenticated user.
- **When:**
    - `GetTickerAsync("XBTNGN")` is called.
- **Then:**
    - The SDK throws a `LunoForbiddenException` (mapped by the central error handler).

### Handling Authentication Failure (401)
- **Given:**
    - Invalid API credentials provided during client initialization.
- **When:**
    - `GetTickerAsync("XBTMYR")` is called.
- **Then:**
    - The SDK throws a `LunoUnauthorizedException` (mapped by the central error handler).

## 7. Definition of Done
### Quality Gates
- 100% test pass on project-core and project-infrastructure.
- **CLI Victory:** 100% success rate for `Concept04_SingleTicker` in the Demonstration Gallery.
- XML Documentation for the new method and all new exceptions.
- **TDD Mandate:** Verification must favor behavioral outcomes over internal state. Avoid mocking internal logic; prefer real collaborators unless external/slow I/O is involved.

### Verification Strategy
- `dotnet test --filter "Category=Unit&FullyQualifiedName~Market|Category=Unit&FullyQualifiedName~Exceptions"`

## 8. Alternatives Considered & Trade-offs
- **Alternative A:** Implementing client-side ticker length/format validation. -> Rejected because it adds maintenance overhead and risks breaking when the API introduces new pair formats. Delegated to the API as the Source of Truth.
- **Trade-offs:** Minimal trade-offs; adding semantic exceptions is a core tenet of our **Clean Architecture** mandate.

## 9. Financial Breaking Points
- **Rate Limiting:** High-frequency polling of a single ticker may hit Luno's rate limits (**300 calls per minute**). Exposing `RetryAfter` allows for high-fidelity back-off strategies.
- **Data Freshness:** Data is cached for up to **1 second**. High-frequency bots must account for this "stale window" during rapid price swings.

## 10. Pre-Mortem
- **Failure Scenario:** The `Retry-After` header is missing or in an unexpected format.
- **Mitigation:** The `LunoErrorHandlingAdapter` should handle missing or invalid headers gracefully, leaving `RetryAfter` as `null` and allowing the application to use a default back-off.

## 11. The Kill List
- **Killed:** Brittle client-side ticker validation logic.
- **Killed:** The inefficient "Fetch All and Filter" pattern for single-pair applications.
- **Killed:** Guessing how long to wait after a rate limit hit.
- **Killed:** Ambiguous unmapped exceptions (Superceded by **RFC 004 Exception Hierarchy**).

## Appendix: Raw API Response Examples
To ensure high-fidelity mapping in `MarketMapper.cs`, the following raw JSON examples and official **Luno Conventions** should be used as the Source of Truth.

### 1. Official Timestamp Convention
As per the `luno_api_spec.json` Conventions section:
> "Timestamps are always represented as an integer number of milliseconds since the UTC Epoch (a Unix timestamp)."

**Mandate:** The Infrastructure layer must parse raw 13-digit millisecond values using `long` (64-bit) to prevent overflow. These values must then be mapped to the **`DateTimeOffset`** type in the Core domain entity to provide a high-fidelity, type-safe developer experience.

### 2. Singular Ticker (`GET /api/1/ticker`)
```json
{
  "ask": "1000000.00",
  "bid": "999000.00",
  "last_trade": "999500.00",
  "pair": "XBTZAR",
  "rolling_24_hour_volume": "12.34",
  "status": "ACTIVE",
  "timestamp": 1710300000000
}
```
**Note:** The intermediate specification patch in `patch-spec.js` forces Kiota to generate `long? Timestamp` for this DTO, ensuring high-fidelity mapping without overflow risk.

### 3. Bulk Tickers (`GET /api/1/tickers`)
```json
{
  "tickers": [
    {
      "ask": "1000000.00",
      "bid": "999000.00",
      "last_trade": "999500.00",
      "pair": "XBTZAR",
      "rolling_24_hour_volume": "12.34",
      "status": "ACTIVE",
      "timestamp": 1710300000000
    }
  ]
}
```
**Note:** Each item in the array is generated as a `Ticker` DTO with `long? Timestamp`, providing the correct capacity.
