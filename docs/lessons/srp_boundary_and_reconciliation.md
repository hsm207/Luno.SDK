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

## Summary — The Four Diagnostics to Run on Every Infrastructure Method

| Check | Question | If "no" → action |
|-------|----------|-----------------|
| **Dumb pipe test** | Would this method survive a vendor swap? | Move logic to Application |
| **Boundary type test** | Does the parameter/return type have methods? | Create a separate plain DTO |
| **Testability test** | Can this be unit-tested without HTTP/WireMock? | Logic is in the wrong layer |
| **Fail-fast test** | Does every enum switch throw on unrecognized values? | Replace silent catch-all with `LunoMappingException` |
