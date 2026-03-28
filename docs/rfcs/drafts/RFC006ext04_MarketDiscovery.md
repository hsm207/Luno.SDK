# RFC 006 Ext 04: Market Discovery and Metadata

**Status:** Draft 📝  
**Date:** 2026-03-28  
**Author(s):** Gemini CLI  
**Base RFC:** [RFC 006: Trading Client and Order Lifecycle Management](./RFC006_TradingClientAndLimitOrderPlacement.md)

## 1. Executive Summary: The Vision & The Value
- **The What & The Why:** This RFC proposes the implementation of the `/api/exchange/1/markets` endpoint to provide dynamic discovery of market metadata. Currently, the SDK lacks awareness of market-specific constraints (MinVolume, MinPrice, Scales), forcing developers to hardcode these values or handle avoidable API errors.
- **Business & System ROI:** This unblocks safe production testing (enabling "Low-Ball" orders) and reduces operational risk by allowing client-side validation of order constraints before hitting the network.
- **The Future State:** The SDK becomes "Market-Aware," enabling high-fidelity validation and automated trading strategies that adapt to changing exchange rules without manual code updates.

## 2. The Status Quo & The Timebombs
- **The Urgency (Why Now?):** Safe production testing requires placing orders that are guaranteed not to fill (Low-Balling). Without dynamic discovery of `MinPrice` and `MinVolume`, we are "guessing" the safety boundaries, which could lead to accidental fills or `ErrAmountTooSmall` rejections.
- **The Timebombs (Assumptions):** 
    - **Uniformity Assumption**: Assuming all markets share the same minimums (they don't!).
    - **Hardcoded Scales**: Assuming price and volume scales are static (they can change!).
    - **Data Population**: Assuming Luno always sends non-null data (verified in production, but missing from the OpenAPI spec).

## 3. Goals & The Scope Creep Shield
- **Goals:**
    - Implement `FetchMarketsAsync` in the (internal) `ILunoMarketOperations`.
    - Provide a public `GetMarketsAsync` extension method supporting optional **pairs filtering**.
    - Expose a `MarketInfo` domain record with a **Strict Zero-Null Policy** for all 11 metadata fields.
    - Ensure **Atomic Verification**: The whole list succeeds, or the whole call fails.
- **Non-Goals (The Shield):**
    - This RFC does NOT implement caching. Caching is the responsibility of the consumer to avoid stale metadata issues.
    - This RFC does NOT implement the Order Book streamer.
    - This RFC does NOT implement market-specific fee overrides.
    - This RFC does NOT change the signature of existing trading methods.

## 4. Proposed Technical Design
### 4.1 Architecture & Boundaries
This follows the "Split & Seal" pattern established in RFC 006 Ext 03.

```mermaid
graph TD
    %% @boundary: Public-API | Isolation: Fluent Extensions
    subgraph PublicAPI [Public API Surface]
        Extensions[LunoMarketExtensions]
    end

    %% @boundary: Application-Layer | Isolation: Query Pipeline
    subgraph Application [Application Layer]
        Dispatcher[ILunoCommandDispatcher]
        Handler[GetMarketsHandler]
    end

    %% @boundary: Infrastructure-Layer | Isolation: Encapsulated Operations
    subgraph Infrastructure [Infrastructure Layer]
        %% @contract: ILunoMarketOperations | Responsibility: Raw Metadata Fetching
        OpsInterface[ILunoMarketOperations]
        ConcreteClient[LunoMarketClient]
    end

    Extensions -- "1. Dispatch Query" --> Dispatcher
    Dispatcher -- "2. Execute" --> Handler
    Handler -- "3. Fetch Metadata" --> OpsInterface
    OpsInterface -. "4. Explicit Implementation" .-> ConcreteClient
```

### 4.2 Public Contracts & Schema Mutations
- **MarketInfo (Core)**: A new domain record representing the "Total Population" invariant. To prevent "Partial Data Timebombs," this record mandates that all 11 fields are populated.
    - `Pair` (string)
    - `Status` (MarketStatus)
    - `BaseCurrency` (string)
    - `CounterCurrency` (string)
    - `MinVolume` (decimal)
    - `MaxVolume` (decimal)
    - `VolumeScale` (int) - **Semantic Downcast** from `long?`.
    - `MinPrice` (decimal)
    - `MaxPrice` (decimal)
    - `PriceScale` (int) - **Semantic Downcast** from `long?`.
    - `FeeScale` (int) - **Semantic Downcast** from `long?`.

- **GetMarketsQuery (Application)**: A query to retrieve all or specific market metadata.
    - `Pairs` (string[]?) - Optional filter to limit results and mitigate "Total Blackout" risk if specific market schemas break.

**Domain Invariants**: 
1.  **Zero-Null Policy**: All properties use the `required` keyword.
2.  **Scale Guardrails**: All scale fields (Price, Volume, Fee) MUST be between **0 and 28** (physical limit of the .NET `decimal` type). [1]

**Enforcement Strategy (Fail-Fast)**: 
1.  **Compiler Enforcement**: All properties use the `required` keyword.
2.  **Boundary Guardrail**: The Infrastructure layer (Client) performs a "Full-House Validation" during mapping. If any Kiota-returned field is `null` or `whitespace`, or if any scale is outside the valid range (0-28), the client MUST throw `LunoDataException` immediately.
3.  **No Graceful Degradation**: We prioritize **Integrity over Availability**. A single malformed market pair in the response will fail the entire request to prevent the SDK from operating on "Shit Data." 🛡️⚖️

**Technical Note on Scaling Types**: The choice of `int` for scale fields aligns with .NET standards (`decimal.Scale`).

## 5. Execution, Rollout, & The Sunset
- **Phase 0: Ruthless Mapping (The Cowardice Fix)**
  - **Description**: Refactor `MarketMapper.cs` to remove the `0m` fallback in `ParseDecimal`. All decimal parsing for tickers and market metadata MUST throw `LunoMappingException` on failure to prevent "Zero-Value" logic errors in automated trading.
- **Phase 1: Foundation & Boundary Fortress**
  - **Description:** Define the `MarketInfo` record and implement the "Split & Seal" infrastructure in `LunoMarketClient`.
  - **Merge Gate:** Unit tests verify the "Zero-Null" mapping, scale range validation, and `LunoDataException` guardrails.
- **Phase 2: Application Orchestration**
  - **Description:** Implement `GetMarketsHandler` and the `GetMarketsAsync` public extension returning `Task<IReadOnlyList<MarketInfo>>`.
  - **Merge Gate:** Tier 2 Integration tests verify the end-to-end flow from extension to Kiota.
- **Phase X: The Sunset**
  - **The Kill List:** Remove the temporary `labs/verify_markets_api.cs` script once the feature is verified in the Gallery.
  - **Completed**: Phase 1: Spec Patching (The Explosion Fix) is successfully implemented and committed. 🥂✅

## 6. Behavioral Contracts
### 6.1 Discovery Success (Happy Path)
- **Tier:** Integration
- **Given:** A valid Luno Client and a functioning network.
- **When:** Calling `client.Market.GetMarketsAsync(new[] { "XBTMYR", "ETHMYR" })`.
- **Then:** Returns a collection containing two `MarketInfo` objects for "XBTMYR" and "ETHMYR", both with verified non-zero minimums.
- **Verification:** **Existing Integration Tests** (WireMock) verify the `/api/exchange/1/markets?pair=XBTMYR&pair=ETHMYR` GET request.🛡️🌊⚖️

### 6.2 Discovery Integrity (Chaos Path)
- **Tier:** Unit
- **Given:** A list of 10 markets where the 10th market has an invalid `PriceScale` (e.g., 99) or a null `MinVolume`.
- **When:** The Client maps the response.
- **Then:** Throws `LunoDataException` before the consumer receives any data.
- **Verification:** **Client Unit Tests** verify the "All-or-Nothing" atomic mapping policy, ensuring no "corrupted" data leaks to the consumer.🛡️⚖️

## 7. Operational Reality
- **Blast Radius:** **Medium**. A schema break by Luno will disable the discovery feature entirely until the SDK is updated.
- **Observability:** Tracked via standard `LunoTelemetry` with the operation name `GetMarkets`.
- **Security & Compliance:** Public API. No PII or credentials involved in the request/response.

## 8. Disaster Recovery & The Panic Button
- **The "Panic Button":** None needed (additive feature). 
- **Data Safety:** Purely read-only discovery. No risk to account funds or market state.

## 9. The Pre-Mortem & Trade-offs
- **Rejected Options:** 
    - **Graceful Degradation**: Rejected to prevent passing malformed or incomplete metadata to consumers.
    - **SDK-Level Caching**: Rejected to avoid stale metadata issues; caching is left to the consumer.
- **The Pre-Mortem:** If this fails, it's because we chose **Integrity** over **Availability**, and a single bad market pair in Luno's response caused a total blackout of the discovery feature for our users.

## 10. Definition of Done
- **Verification Strategy:** Run `labs/list_market_rules.cs` to print the minimums for `XBTMYR` on the production API.
- **TDD Mandate:** 100% test pass on `Luno.SDK.Core`. Total coverage of Behavioral Contracts via their specified Verification Tiers. Zero mocking of internal domain logic.

---
**Citations:**
[1] [Decimal.Scale Property - Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/api/system.decimal.getbits)
