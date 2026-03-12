# RFC 001: Pragmatic Telemetry Architecture
**Status:** Implemented (March 11, 2026)  
**Date:** 2024-03-03

## 1. Overview
This RFC documents the architectural decision to implement a decorator-based telemetry system for the Luno SDK using Kiota's `IRequestAdapter`. The design prioritizes **Developer Experience (DX)**, **Performance**, and **Operational Stability** over strict internal decoupling.

## 2. Motivation
The previous telemetry implementation was tightly coupled with the business logic in `LunoMarketClient`. Moving telemetry to a decorator (`LunoTelemetryAdapter`) allows for a cleaner separation of concerns while maintaining the high performance required for a financial SDK. We have deliberately chosen to avoid certain "pure" SOLID patterns to keep the internal implementation lean and easy to maintain.

## 3. Future State
SDK users will be able to instantiate the client with a single call: `new LunoClient()`. All telemetry, authentication, and resilience will be pre-configured and "just work." Developers adding new API endpoints will follow a standardized pattern for emitting stable telemetry signals that map directly to operational dashboards.

## 4. Goals & Non-Goals
- **Goals:**
    - Isolate telemetry logic into a reusable decorator.
    - Provide a "zero-config" entry point for SDK users.
    - Ensure stable telemetry operation names for dashboard consistency.
    - Minimize performance overhead by avoiding unnecessary internal abstractions.
- **Non-Goals:**
    - Achieving "Pure SOLID" compliance for internal-only infrastructure.
    - Creating a generic telemetry framework for external use.

## 5. Proposed Technical Design
### High-Level Architecture
We use the **Decorator Pattern** to wrap Kiota's `IRequestAdapter`. The `LunoTelemetryAdapter` intercepts all outgoing requests, records timing and success/failure signals, and delegates the actual network call to an inner adapter.

### Public API Changes
- **Modified:** `LunoClient` constructor now handles the orchestration of the decorated request adapter.
- **Added:** `ILunoClient.Telemetry` property to expose observability signals to advanced users.

### Phased Implementation
- **Phase 1: Decorator Foundation (Completed)**
    - Implement `LunoTelemetryAdapter` as an `IRequestAdapter` decorator.
    - Refactor `LunoMarketClient` to use the decorated adapter.
- **Phase 2: Orchestration (Completed)**
    - Update `LunoClient` to wire up the decorator chain in its constructor.
- **Phase 3: Stabilization (Completed)**
    - Implement trace enrichment with `luno.operation` and `luno.status` tags.
    - Finalize high-fidelity integration tests using `ActivityListener`.

## 6. Behavioral Specifications
### Successful Request Tracking
- **Given:** A `LunoClient` initialized with defaults.
- **When:** `Market.GetTickersAsync` is called and the API returns 200 OK.
- **Then:** An OpenTelemetry `Activity` named "GetMarketTickers" is emitted.
- **And:** The activity contains tags `luno.operation` ("GetMarketTickers") and `luno.status` ("Success").
- **And:** The request counter is incremented with matching tags.

### Failed Request Tracking
- **Given:** A `LunoClient` initialized with defaults.
- **When:** An API call results in an `ApiException` or `HttpRequestException`.
- **Then:** The `Activity` is marked as "Error" with tags `luno.operation` and `luno.status` ("Error").
- **And:** The error is recorded in the telemetry metrics.

## 7. Definition of Done
### Quality Gates
- 100% pass rate on `Luno.SDK.Tests.Integration`.
- Telemetry signals are verified to be emitted with the correct names and tags.
- No new external dependencies introduced for internal telemetry recording.
- **TDD Mandate:** Verification favors behavioral outcomes using real `ActivityListener` spies over internal state checks or mocks.

### Verification Strategy
- Run `dotnet test Luno.SDK.Tests.Integration` to verify real OpenTelemetry emission and tag accuracy.

## 8. Alternatives Considered & Trade-offs
- **Alternative A: Strict DIP with Internal Interfaces** -> Rejected because the classes are internal and serve a unified purpose within the infrastructure layer. Introducing interfaces for internal coupling adds unnecessary complexity and indirection.
- **Alternative B: Generic Operation Naming** -> Rejected to ensure **Dashboard Stability**. Hardcoding operation names prevents accidental dashboard breakages during code refactoring and avoids the performance penalty of reflection.
- **Alternative C: External Factory for Construction** -> Rejected to prioritize **DX**. SDK users expect a simple `new LunoClient()` experience. Moving construction to a separate factory adds an unnecessary layer of indirection for the primary use case.

### Trade-offs:
- **Pragmatism vs. Purity:** We accept minor internal coupling in exchange for a significantly simpler and more performant implementation.
- **Stability vs. Flexibility:** We favor hardcoded telemetry names to ensure our operational monitoring remains reliable and consistent.
