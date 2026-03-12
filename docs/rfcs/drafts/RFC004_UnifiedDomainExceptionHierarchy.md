# RFC 004: Unified Domain Exception Hierarchy

**Status:** Draft  
**Date:** 2026-03-11

## 1. Overview
This RFC formalizes the Luno SDK exception hierarchy by reconciling the existing core exceptions with a new, high-fidelity mapping of API error states. We move from a "Transport-Centric" model to a "Behavior-Centric" model.

## 2. Motivation
The current codebase has fragmented exceptions (`LunoDataException`, `LunoSecurityException`). While the user's proposed design added semantic clarity, it introduced unnecessary nesting and naming inconsistencies. We need a hierarchy that is shallow enough to be usable but deep enough to be semantic.

## 3. Future State
Developers can handle errors based on the required action:
```csharp
try {
    await client.Trading.PostLimitOrderAsync(...);
}
catch (LunoInsufficientFundsException) {
    // Action: Trigger deposit
}
catch (LunoRateLimitException ex) {
    // Action: Back-off for ex.RetryAfter
}
catch (LunoSecurityException) {
    // Action: Check API Keys
}
```

## 4. Goals & Non-Goals
- **Goals:**
    - Standardize on `LunoException` as the abstract root.
    - Consolidate all server-side errors under `LunoApiException`.
    - Map actionable business errors (e.g., Insufficient Funds) to surgical domain exceptions.
    - Leverage existing `LunoDataException` and `LunoSecurityException`.
- **Non-Goals:**
    - Creating "Category" exceptions that have no specific consumer action (e.g., `LunoPolicyException`).

## 5. Proposed Technical Design
### High-Level Architecture
```mermaid
classDiagram
    class LunoException { <<abstract>> }
    LunoException <|-- LunoClientException : Pre-flight failures
    LunoException <|-- LunoApiException : Server-side (4xx/5xx)
    LunoException <|-- LunoMappingException : Response parsing failures

    %% Security Family
    LunoApiException <|-- LunoSecurityException
    LunoSecurityException <|-- LunoUnauthorizedException : 401
    LunoSecurityException <|-- LunoForbiddenException : 403

    %% Data/Resource Family
    LunoApiException <|-- LunoDataException
    LunoDataException <|-- LunoNotFoundException : 404
    LunoDataException <|-- LunoValidationException : 400 (Bad Params)

    %% Operational/Actionable Family
    LunoApiException <|-- LunoRateLimitException : 429
    LunoApiException <|-- LunoMarketUnavailableException : 503

    %% Business/Domain Family
    LunoDataException <|-- LunoInsufficientFundsException : 400 (ErrInsufficientFunds)
```

### Public API Changes
- **Updated Base:** `LunoApiException` (replaces `LunoServiceException`).
- **New Surgical Exceptions:**
    - `LunoInsufficientFundsException` (inherits from `LunoDataException`).
    - `LunoRateLimitException` (includes `TimeSpan? RetryAfter`).
    - `LunoMarketUnavailableException` (handles `ErrUnderMaintenance`).

### Phased Implementation
### Phase 1: Exception Consolidation
- **Description:** Update existing exceptions to match the new hierarchy and create the missing surgical types.
- **Core Changes:** 
    - Move `LunoUnauthorizedException` and `LunoForbiddenException` under `LunoSecurityException`.
    - Create `LunoInsufficientFundsException.cs`, `LunoRateLimitException.cs`, `LunoMarketUnavailableException.cs`.
- **Locations:** `Luno.SDK.Core/Exceptions/`

### Phase 2: Centralized Error Mapping
- **Description:** Implement the exhaustive mapping logic in the request adapter decorator.
- **Core Changes:** Update `LunoErrorHandlingAdapter.cs` to use a refined factory pattern.
- **Locations:** `Luno.SDK.Infrastructure/ErrorHandling/LunoErrorHandlingAdapter.cs`

## 6. Behavioral Specifications
### Handling Insufficient Funds
- **Given:** A 400 response with `ErrorCode: "ErrInsufficientFunds"`.
- **When:** Any API call is made.
- **Then:** The SDK throws `LunoInsufficientFundsException`.

### Handling Rate Limits
- **Given:** A 429 response with `Retry-After: 60`.
- **When:** Any API call is made.
- **Then:** The SDK throws `LunoRateLimitException` with `RetryAfter` set to 60 seconds.

## 7. Definition of Done
### Quality Gates
- 100% pass on `LunoExceptionComplianceTests`.
- All new exceptions documented with XML `<remarks>` explaining the Luno error code they map to.
- **TDD Mandate:** Verification must favor behavioral outcomes over internal state. Avoid mocking internal logic; prefer real collaborators unless external/slow I/O is involved.

## 8. Alternatives Considered & Trade-offs
- **Alternative A:** Creating a deep hierarchy like `LunoPolicyException`. -> Rejected as YAGNI; shallow hierarchies are easier for consumers to navigate.
- **Trade-offs:** Inheriting `LunoInsufficientFundsException` from `LunoDataException` acknowledges it is a response-data issue while keeping it distinct for catch blocks.

## 9. Financial Breaking Points
- **Incorrect Mapping:** If we map a recoverable error to a terminal exception, trading bots might stop unnecessarily. Mapping must be high-fidelity.

## 10. Pre-Mortem
- **Failure Scenario:** Luno adds a new 400 error code that is critical for trading.
- **Mitigation:** `LunoApiException` preserves the raw `ErrorCode` string so developers can still perform manual checks while waiting for an SDK update.

## 11. The Kill List
- **Killed:** `LunoServiceException` (too generic).
- **Killed:** Nested category exceptions (YAGNI).
- **Killed:** Ambiguous 400 errors without semantic context.
