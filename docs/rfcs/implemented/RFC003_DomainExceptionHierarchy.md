
# RFC 003: Domain Exception Hierarchy & High-Fidelity Validation

**Status:** Implemented (March 11, 2026)  
**Date:** 2026-03-04

## 1. Overview
This RFC proposes the implementation of a structured, domain-driven exception hierarchy for the Luno SDK. It introduces a single root `LunoException` and specialized branches for security, data, and mapping errors. Furthermore, it mandates "fail-fast" validation within the Infrastructure mappers to ensure only valid domain entities enter the Application layer.

## 2. Motivation
Currently, the Luno SDK's custom exceptions (`LunoAuthenticationException`, `LunoUnauthorizedException`, `LunoForbiddenException`) derive directly from the base `System.Exception` class. This makes it difficult for users to catch all SDK-specific errors in a single block. 

Additionally, the Luno API specification is often incomplete regarding "required" fields, leading to "low-fidelity" mapping where mandatory domain fields (like `AccountId` or `Asset`) are given empty string defaults. We need a consistent way to handle these data integrity issues.

## 3. Future State
Developers can catch all Luno-related errors using a single `catch (LunoException ex)` block. The hierarchy provides clear, semantic categories (Security vs. Data) to allow for granular error handling. The SDK will proactively reject malformed API responses at the Infrastructure boundary, preventing "garbage data" from propagating into the Application and Domain layers.

## 4. Goals & Non-Goals
- **Goals:**
    - Establish `LunoException` as the root for all custom exceptions.
    - Categorize existing and new exceptions into logical branches.
    - Implement "fail-fast" validation in `AccountMapper`.
    - Adhere to "Pro Library" standards for exception constructors (Method A).
- **Non-Goals:**
    - Refactoring Kiota-generated `ApiException` handling logic (handled by `LunoErrorHandlingAdapter`).
    - Adding validation to non-mandatory metadata fields (e.g., `Account.Name`).

## 5. Proposed Technical Design
### High-Level Architecture
- **Root**: `LunoException` (Abstract)
- **Security Branch**: `LunoSecurityException`
    - `LunoAuthenticationException` (Fail-Fast)
    - `LunoUnauthorizedException` (401)
    - `LunoForbiddenException` (403)
- **Data Branch**: `LunoDataException`
    - `LunoMappingException` (Mapper failures)

### Exception Constructor Standards (Method A)
To adhere to .NET "Pro Library" standards, every custom exception must implement the following three constructors:
1.  **Parameterless**: `public MyException() : base() { }`
2.  **Message**: `public MyException(string message) : base(message) { }`
3.  **Wrapper**: `public MyException(string message, Exception inner) : base(message, inner) { }`

**Method A (Explicit Compliance Testing)**: Instead of excluding boilerplate constructors from coverage reports, we will write explicit, lightweight unit tests for each constructor. These tests must include docstrings clarifying that they exist solely to satisfy architectural compliance and coverage mandates.

### Phased Implementation
- **Phase 1: Core Hierarchy**
    - **Description:** Implement the base and branch exception classes in `Luno.SDK.Core`.
    - **Core Changes:** Create `LunoException`, `LunoSecurityException`, `LunoDataException`, and `LunoMappingException`.
    - **Locations:** `Luno.SDK.Core/Exceptions/`
- **Phase 2: Exception Refactoring & Compliance Testing**
    - **Description:** Update existing exceptions to derive from the new hierarchy and implement all three standard constructors. Write explicit unit tests to ensure 100% coverage.
    - **Core Changes:** Update `LunoAuthenticationException`, `LunoForbiddenException`, and `LunoUnauthorizedException`. 
    - **Locations:** 
        - `Luno.SDK.Core/Exceptions/`
        - `Luno.SDK.Tests.Unit/Core/Exceptions/LunoExceptionComplianceTests.cs`
- **Phase 3: High-Fidelity Mapping**
    - **Description:** Implement "fail-fast" validation in the Infrastructure mappers.
    - **Core Changes:** 
        - Update `AccountMapper.ToDomain` to throw `LunoMappingException` if `AccountId` or `Asset` is null/empty.
        - Update `MarketMapper.MapToEntity` to throw `LunoMappingException` instead of `InvalidOperationException` for missing `Pair` or `Timestamp`.
    - **Locations:** 
        - `Luno.SDK.Infrastructure/Account/AccountMapper.cs`
        - `Luno.SDK.Infrastructure/Market/MarketMapper.cs`

## 6. Behavioral Specifications
### Root Exception Catching
- **Given:** A `LunoClient` is performing any operation.
- **When:** Any SDK-specific error occurs (Auth, 401, 403, or Mapping).
- **Then:** The exception must be catchable via `catch (LunoException)`.

### Mapping Validation Failure
- **Given:** The Luno API returns an `AccountBalance` DTO with a missing `account_id`.
- **When:** `AccountMapper.ToDomain` is called.
- **Then:** It must throw a `LunoMappingException` containing the message "Missing mandatory field: account_id" and the `DtoType` set to "AccountBalance".

## 7. Definition of Done
### Quality Gates
- **100% Test Pass**: All unit and integration tests must pass.
- **Coverage**: The `Core` project exceptions must show 100% coverage (utilizing Method A compliance tests).
- **Architecture**: All custom exceptions must derive from `LunoException`.
- **TDD Mandate:** Verification must favor behavioral outcomes. Tests should prove that `LunoException` acts as a catch-all for all subtypes.

### Verification Strategy
- **Unit Tests**: Add tests in `Luno.SDK.Tests.Unit` verifying the hierarchy and the `AccountMapper` validation logic.
- **Commands**: `dotnet test --filter Luno.SDK.Tests.Unit`

## 8. Alternatives Considered & Trade-offs
- **Alternative A: Use Standard Exceptions**: We could use `InvalidOperationException` or `ArgumentException`, but this prevents users from easily distinguishing between SDK errors and general runtime errors.
- **Trade-offs**: Refactoring the existing exceptions is a "breaking change" for any users already catching them specifically, but given the early stage of the SDK, the long-term architectural benefits outweigh the cost.
