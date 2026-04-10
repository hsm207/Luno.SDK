# Lesson: Exception Normalization at Architectural Boundaries

## 1. The Probibition of "Naked" Exceptions
An SDK should never leak framework-level or infrastructure-specific exceptions (e.g., `NullReferenceException`, `HttpRequestException`) to the consumer. 

### The Problem: Leaky Abstractions
Using the null-forgiving operator (`!`) on API responses or failing to catch network-level failures results in raw system exceptions escaping the SDK boundary. This forces the consumer to depend on non-domain errors, indicating a lack of polish and violating the Liskov Substitution Principle (as different implementations would leak different "naked" exceptions).

### The Rule
> **Infrastructure must normalize all errors into domain-specific exceptions.**
> Even for "impossible" conditions—such as a successful API call returning an empty body—the infrastructure bridge must catch these states and translate them into specialized types like `LunoMappingException` or `LunoNetworkException`. 

---

## 2. Explicit Validation over Force-Casting
Static analysis tools and operators like `!` should not be used as a substitute for explicit validation of mandatory API data.

### The Rule
> **Use descriptive checks to provide diagnostic clarity.**
> Replace the null-forgiving operator with explicit null checks and descriptive error messages. This ensures that when a failure occurs at the boundary, the resulting exception provides actionable feedback about which specific API response field was missing or malformed.
