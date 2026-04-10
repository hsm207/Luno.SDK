# Lesson: Defensive Coding Standards and Integrity Guardrails

## 1. Guard Clauses vs. Nested If-Chains
Code should express its failure conditions immediately and clearly to maintain readability and prevent logical drift.

### The Anti-Pattern
Using nested `if` statements with mutable flags (e.g., `bool isValid = true;`) and deferring error handling to the end of a method. This hides intent and forces the reader to track the entire state of the method to understand why a failure occurred.

### The Rule
> **Use guard clauses with early throws.**
> Validate inputs immediately and throw the specific domain exception as soon as a condition is violated. This eliminates unnecessary nesting and ensures that the "Happy Path" remains the primary, unindented flow of the method.

---

## 2. Explicit Enum Mapping
Enum conversions must be exhaustive. Wilcard catch-alls should never be used to mask unmapped or unrecognized values.

### The Anti-Pattern
Using a wildcard/default arm (e.g., `_ => OrderStatus.Unknown`) in a switch expression to silently swallow any new or unrecognized API values. This masks data integrity issues and prevents the system from alerting on schema changes.

### The Rule
> **Throw on the catch-all arm of an enum switch.**
> Every known enum value must be mapped explicitly. The `_` arm should be used as a safety net to throw a `LunoMappingException`. If an API field results in an unrecognized string, Kiota typically deserializes it as `null`. The `null` arm should have a deliberate, documented mapping—not a silent fallback provided by a wildcard.

---

## 3. Infrastructure Integrity (No Guessing)
The Infrastructure layer must never make assumptions or "guess" values for missing or ambiguous API data.

### The Anti-Pattern
Silently defaulting missing balance fields to `0` or mapping `null` statuses to `Awaiting` because it is "likely" correct.

### The Rule
> **Infrastructure must fail fast on ambiguous data.**
> A mapping failure is a hard failure. If the API returns data that violates domain invariants (e.g., an order without a status), the infrastructure bridge must throw a `LunoMappingException` immediately. Providing a "safe" default is never safe if it is not strictly true and can lead to catastrophic downstream logic errors.
