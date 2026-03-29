# Lessons Learned: SRP Boundary Violations in Clean Architecture

> **Context**: Phase 11 of RFC 006 corrected two architectural violations that survived multiple design reviews.
> This document exists because they were not caught at implementation time and required a third-party SRP audit to surface.

---

## Violation 1 — Domain Value Objects Must Not Cross Architectural Boundaries

### What Was Wrong
`ILunoTradingClient.PostLimitOrderAsync` accepted `LimitOrderParameters` — a Domain Value Object with a `Validate()` method.
A live method on an interface parameter means Infrastructure is coupled to Domain behavior, not just Domain data.
Any change to validation rules (Domain actor's concern) forces Infrastructure to recompile for zero reason.

### The Rule
> **Boundary seams must be crossed with behavior-free POCOs only.**
> If the type has a method — including `Validate()`, `ToString()` overrides with logic, or computed properties — it is not a DTO.
> Domain Value Objects enforce invariants; that is their job. But they must stay *inside* the Application layer.
> The Application layer creates the DVO, calls `Validate()`, and *then* maps the plain data to a boundary DTO before crossing out.

### Diagnostic Question
*"Does this type have any method other than a C# record's auto-generated equals/hash?"*
If yes → it is not a boundary DTO. Create a separate plain record.

---

## Violation 2 — Business-Equivalence Logic Is Application Policy, Not Infrastructure Plumbing

### What Was Wrong
`LunoTradingClient.ReconcileDuplicateOrderAsync` caught a `LunoIdempotencyException`, made a second API call, then compared `Price`, `Volume`, and `Side` to decide whether the duplicate was acceptable.
The comparison logic ("is this the same order?") is a **business equivalence rule** — not an HTTP protocol detail.
It was buried inside Infrastructure, making it:
- Untestable without WireMock
- Invisible to Application-layer readers
- Coupled to Kiota response field shapes

### The Rule
> **Infrastructure's job is to be a dumb pipe — translate domain requests to external API calls and translate responses back to domain types. Nothing more.**
> The "dumb pipe" test: *"If I swapped the external API vendor, would any of this code survive unchanged?"*
> If a method contains branching on domain values (price comparisons, side checks, policy decisions), it fails the dumb pipe test and belongs in Application.

### The Actor Test — Necessary But Not Sufficient
The "one actor" heuristic ("there's only one stakeholder so SRP is satisfied") is a necessary condition, not a sufficient one.
Ask instead: *"Which architectural layer owns the knowledge this code uses?"*
A business equivalence rule uses domain knowledge (what constitutes the same order) → Application owns it.
An HTTP retry/mapping rule uses API protocol knowledge (what a 409 means) → Infrastructure owns it.

---

## Violation 3 — Nested If-Chains on a Mutable Flag Is a Code Smell

### What Was Wrong
```csharp
bool parametersMatch = true;
if (x != null && decimal.TryParse(x, ..., out var parsed)) {
    if (parsed != expected) parametersMatch = false;
}
// ... repeated twice more ...
if (!parametersMatch) throw ...;
```
This pattern delays information, hides intent, requires reading the entire method to understand what fails, and groups all mismatches into a single generic error.

### The Rule
> **Use guard clauses with early throws.** Each condition should express its own failure immediately.
> No mutable flags. No deferred boolean accumulation. No parsing inside business-logic methods.
> Parse at the boundary (in the Infrastructure mapper), compare domain types in Application.

### Correct Form
```csharp
private static void EnsureParametersMatch(Order existing, LimitOrderParameters expected)
{
    if (existing.LimitPrice.HasValue && existing.LimitPrice != expected.Price)
        throw new LunoIdempotencyException($"Price mismatch: {existing.LimitPrice} vs {expected.Price}.");

    if (existing.LimitVolume.HasValue && existing.LimitVolume != expected.Volume)
        throw new LunoIdempotencyException($"Volume mismatch: ...");

    if (existing.Side.HasValue && existing.Side != expected.Type)
        throw new LunoIdempotencyException($"Side mismatch: ...");
}
```

---

## Violation 4 — Silent Catch-All Enum Mappings Corrupt Data

### What Was Wrong
```csharp
_ => MarketStatus.Unknown   // silently swallows any new API value
_ => OrderStatus.Awaiting   // lies about the order state
```
Both `MarketMapper.MapStatus` and `LunoTradingClient.MapStatus` used wildcard catch-alls that silently converted unrecognized enum values into a "safe" default.
If the API adds a new status value (e.g., `SUSPENDED`), our code would never crash and never log — it would just quietly return the wrong status.
For a financial SDK, mapping an order to `Awaiting` when it's actually in a state we've never seen is a data integrity violation.

### The Rule
> **Enum switch expressions must explicitly map every known value and throw on the catch-all.**
> The `_` arm is a safety net for impossible states, not a convenience default.
> If the API spec defines an `UNKNOWN` value, map it explicitly. Then throw `LunoMappingException` on `_`.
> The `null` arm is separate and may have a legitimate default (e.g., `null => MarketStatus.Unknown`).

### The Kiota Nuance
Kiota deserializes unrecognized enum strings as `null`, not as an invalid enum integer.
The throwing `_` catch-all is only reachable via corrupted data or force-casted invalid values.
The `null` arm is the practical fallback for unrecognized API strings, and it must have a deliberate, documented mapping — not an accidental one from a lazy wildcard.

### Correct Form
```csharp
status switch
{
    GeneratedStatus.ACTIVE   => MarketStatus.Active,
    GeneratedStatus.POSTONLY => MarketStatus.PostOnly,
    GeneratedStatus.DISABLED => MarketStatus.Disabled,
    GeneratedStatus.UNKNOWN  => MarketStatus.Unknown,  // explicit API value
    null                     => MarketStatus.Unknown,  // Kiota couldn't parse
    _ => throw new LunoMappingException(...),          // safety net
};
```

---

---

## Violation 5 — Over-Promising in Interface Contracts (LSP Violation)

### What Was Wrong
The `ILunoTradingClient.GetOrderAsync` interface documented that it threw `LunoValidationException` if neither ID was provided. However, the Infrastructure implementation was a "dumb pipe" that delegated this check to the API. 
The documentation promised behavior that the implementation did not fulfill, creating a trap for callers who expected the implementation to adhere to the documented abstraction contract.

### The Rule
> **Subtypes must adhere strictly to the behavioral contract of their parent/abstraction.**
> If the interface documents an exception, the implementation must either throw it or be guaranteed that a lower layer will throw an identical domain exception.
> Do not document behavior that is actually enforced by an external orchestrator (like a Handler) as part of the interface contract unless the implementation itself enforces it.

### Correct Form
Remove the inapplicable exception documentation from the interface. Keep validation in the Application layer where it belongs.

---

## Violation 6 — Leaking Naked Framework Exceptions (Exception Normalization)

### What Was Wrong
`LunoMarketClient.GetTickersAsync` used `response!.Tickers!` (and similar null-forgiving operators) which could leak a raw `System.NullReferenceException` to the consumer if the API response was unexpectedly empty.
An SDK should never leak framework exceptions — it suggests it is unpolished and forces the user to handle non-domain errors.

### The Rule
> **Infrastructure must normalize all errors and unexpected states into domain exceptions.**
> Even for "impossible" states (like a successful API call returning a null body), use `LunoMappingException` or similar domain-specific types.
> Use explicit null checks instead of the `!` operator to provide descriptive error messages and consistent exception types.

### Correct Form
```csharp
var tickers = response?.Tickers
    ?? throw new LunoMappingException("API returned a null tickers collection.", "TickersResponse");
```

---

## Violation 9 — Infrastructure Integrity and the "Guessing" Anti-Pattern

### What Was Wrong
In a misguided attempt at resilience, the infrastructure mappers were "guessing" the domain state when the API response was ambiguous:
- Mapping `null` or unrecognized strings to `OrderStatus.Awaiting` because "it's probably that."
- Returnining `null` for unmapped order sides instead of throwing, allowing `null` to leak into the Application layer.
- Quietly defaulting missing balance fields to `0`.

### The Rule
> **Infrastructure must never "guess" or "default" missing mandatory data.**
> A mapping failure is a hard failure. If the API returns data that violates our domain invariants (e.g., an order without a status), the infrastructure bridge must collapse immediately by throwing a `LunoMappingException`.
> Guessing a default state (like `Awaiting`) is a **data integrity violation** that can lead to catastrophic downstream failures (e.g., trying to cancel an order that is actually already closed).

### The "Fail-Fast" Mandate
1. **No Silent Defaults**: If an enum value is unrecognized, throw.
2. **No Null Leaks**: If a mandatory field is `null`, throw.
3. **No Guessing**: A "safe" default is never safe if it's not strictly true.

### Correct Form
```csharp
// FAIL-FAST MAPPING
status switch
{
    GeneratedStatus.PENDING => OrderStatus.Awaiting,
    GeneratedStatus.COMPLETE => OrderStatus.Filled,
    // ...
    _ => throw new LunoMappingException($"Unmapped status: {status}", nameof(GetOrder2Response))
};
```

---

## Summary — The Ten Diagnostics to Run on Every Infrastructure Method

| Principle | Violation | Description |
|---|---|---|
| [SRP: Domain Value Objects](file:///home/user/Documents/GitHub/Luno.SDK/docs/lessons/srp_boundary_and_reconciliation.md#violation-1--domain-value-objects-must-not-cross-architectural-boundaries) | SRP | Domain Value Objects must not cross architectural boundaries. Use behavior-free DTOs. |
| [SRP: Business Equivalence Logic](file:///home/user/Documents/GitHub/Luno.SDK/docs/lessons/srp_boundary_and_reconciliation.md#violation-2--business-equivalence-logic-is-application-policy-not-infrastructure-plumbing) | SRP | Business equivalence logic is Application policy, not Infrastructure plumbing. Infrastructure is a dumb pipe. |
| [SRP: Nested If-Chains](file:///home/user/Documents/GitHub/Luno.SDK/docs/lessons/srp_boundary_and_reconciliation.md#violation-3--nested-if-chains-on-a-mutable-flag-is-a-code-smell) | SRP | Nested if-chains on a mutable flag is a code smell. Use guard clauses with early throws. |
| [SRP: Silent Enum Mappings](file:///home/user/Documents/GitHub/Luno.SDK/docs/lessons/srp_boundary_and_reconciliation.md#violation-4--silent-catch-all-enum-mappings-corrupt-data) | SRP | Silent catch-all enum mappings corrupt data. Explicitly map all known values and throw on `_`. |
| [LSP: Documentation and Exceptions](file:///home/user/Documents/GitHub/Luno.SDK/docs/lessons/srp_boundary_and_reconciliation.md#lsp-documentation--exception-normalization) | LSP | Use `/// <exception>` to define contracts. Never leak framework exceptions. |
| [ISP: Client Segregation & Extension Shadowing](file:///home/user/Documents/GitHub/Luno.SDK/docs/lessons/srp_boundary_and_reconciliation.md#isp-client-segregation--naming-shadowing) | ISP | Keep interfaces small. Extension methods can shadow interface methods; use `Fetch`/`Get` distinction to resolve. |
| [DIP: Command Dispatcher & Composition Root](file:///home/user/Documents/GitHub/Luno.SDK/docs/lessons/srp_boundary_and_reconciliation.md#dip-command-dispatcher--composition-root) | DIP | Decouple orchestration from public API. Use a Command Dispatcher to centralize cross-cutting concerns without forcing DI. |
| [Infrastructure Integrity & Fail-Fast](file:///home/user/Documents/GitHub/Luno.SDK/docs/lessons/srp_boundary_and_reconciliation.md#violation-9--infrastructure-integrity-and-the-guessing-anti-pattern) | SRP | Infrastructure must never guess defaults or silent failures. Throw on ambiguous or missing mandatory API data. |
| [File-Level SRP & Focused Abstractions](file:///home/user/Documents/GitHub/Luno.SDK/docs/lessons/srp_boundary_and_reconciliation.md#violation-10--file-level-srp-and-the-overloaded-abstraction-file) | SRP | Avoid catch-all 'Abstractions' files. Each core interface/feature deserves its own home for better navigability and isolation. |

---

> [!NOTE]
> **What's Next?** The architecture further evolved in Phase 17+ with Tiered Interfaces and Pipeline Behaviors. See [Modern DI and Pipeline Patterns](file:///home/user/Documents/GitHub/Luno.SDK/docs/lessons/modern_di_and_pipeline_patterns.md) for the next level of architectural maturity.

## Violation 7 — Interface Segregation (ISP) and the "Baggage" of Extension Methods

### What Was Wrong
Extension methods like `StopOrderAsync` were written against the monolithic `ILunoClient`. This meant a consumer who only needed Market data would still see Trading commands in their IntelliSense, violating ISP.

### The Rule
> **Clients should not be forced to depend on methods they do not use.**
> If using extension methods for a fluent API, ensure they extend the most specific interface possible.

### Correct Form
Refactor extension methods to extend segregated sub-interfaces (`ILunoTradingClient`, etc.). Users now access them via category: `client.Trading.StopOrderAsync(...)`.

---

## Violation 8 — The Shadowing Trap (Instance vs. Extension)

---

## DIP: Command Dispatcher & Composition Root

Dependency Inversion isn't just about using interfaces; it's about owning the abstractions. As the SDK grew, the extension methods (Public API) were instantiating handlers directly, violating DIP and making it impossible to inject cross-cutting concerns like Retry policies (Polly) without modifying every extension.

### The Problem: The "Newing Up" Anti-Pattern
Extension methods are static and cannot be resolved from a DI container easily. By doing `var handler = new PostLimitOrderHandler(client)`, we coupled the Public API directly to a specific implementation.

### The Solution: The Command Dispatcher
We introduced a **Command Dispatcher** that acts as an orchestration hub.

1.  **Inverted Dispatch**: Extensions now call `client.Commands.DispatchAsync<TRequest, TResponse>(command)`. They no longer know *who* handles the command.
2.  **The Central Factory**: The `LunoClient` (acting as the **Composition Root**) wires a `DefaultCommandHandlerFactory` that maps Commands to Handlers.
3.  **Lazy Proxies**: To break the circular dependency (Client needs Dispatcher, Dispatcher needs Clients for handlers), we used `Lazy<T>` and nested **Lazy Proxies** within `LunoClient`.
4.  **Heterogeneous Returns**: The dispatcher was designed to support both `Task<T>` and `IAsyncEnumerable<T>` by removing the explicit `Task` wrapper from the handler interface, allowing it to be part of the generic `TResponse`.

### Lesson: Design for Extensibility
By routing all application logic through the dispatcher, we've created a "Hook" where we can later add:
-   **Logging/Tracing**: Automatically log every command.
-   **Resilience**: Wrap every handler in a Polly policy.
-   **Validation**: Run global guard clauses before dispatching.

This is architectural self-care! 🧘‍♀️

### What Was Wrong
We had `ILunoMarketClient.GetTickersAsync()` returning raw entities (Core), and an extension `GetTickersAsync()` returning DTOs (Application). Because C# prefers instance methods over extensions with the same signature, the "raw" one stole the call, leading to type mismatches.

### The Rule
> **Public APIs should have clear, unambiguous naming conventions across layers.**
> Use consistent verbs to differentiate between low-level infrastructure operations and high-level application operations.

### Correct Form
Adopt a "Fetch/Get" layer separation:
- **Core Gateways** use the verb `Fetch*` (e.g., `FetchTickersAsync`).
- **Application Extensions** use the verb `Get*` (e.g., `GetTickersAsync`).

---

## Violation 10 — File-Level SRP and the Overloaded "Abstraction" File

### What Was Wrong
We had a catch-all file named `ApplicationAbstractions.cs` containing:
- `ILunoCommandDispatcher` (Resolution concern)
- `IPipelineBehavior` (Interception concern)
- `ICommandHandler` (Execution concern)
- Several delegates and secondary interfaces.

While they are all "abstractions," they serve fundamentally different stakeholders and features. Grouping them together violates SRP at the file/component level, leading to "baggage" where a developer only interested in behaviors is forced to load and reason about the dispatcher.

### The Rule
> **File-Level SRP: One file, one primary concern.**
> Avoid "General," "Common," or "Abstractions" files that act as junk drawers for interfaces. 
> Each core interface that defines a distinct architectural feature (Dispatching vs. Interception) deserves its own dedicated file. 

### Correct Form
Split into focused, single-purpose files:
- `ILunoCommandDispatcher.cs`
- `IPipelineBehavior.cs`
- `ICommandHandler.cs`

This improves IntelliSense relevance, project navigability, and prevents the "moral decay" of an architecture that starts allowing unrelated code into a generic container. 🛡️✨
