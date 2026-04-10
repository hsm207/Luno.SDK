# Lesson: Interface Segregation Principle (ISP) in SDK Design

## 1. The Tiered Interface Pattern
To prevent circular dependencies and accidental orchestration loops within the Application layer, interfaces should be segregated by capability.

### The Problem: Recursion Traps
In a monolithic sub-client interface (e.g., `ILunoTradingClient`), both data-fetching methods (e.g., `PostLimitOrderAsync`) and orchestration entry points (e.g., the `Commands` property) are exposed. If a Command Handler is injected with this monolithic interface, it gains the capability to dispatch new commands, potentially leading to infinite recursion or hidden architectural complexity.

### The Rule
> **Inject only the capabilities required by the specific consumer.**
> Sub-clients are segregated into two distinct tiers:
> 1.  **Operations Tier** (e.g., `ILunoTradingOperations`): Contains only raw data-fetching methods. Mark as `internal` where appropriate.
> 2.  **Client Tier** (e.g., `ILunoTradingClient`): Inherits from the Operations tier and adds the public Orchestration property (`Commands`).
> 
> Application-layer handlers should depend exclusively on the **Operations** tier. This ensures it is physically impossible for a handler to trigger an orchestration loop.

---

## 2. Extension Method Baggage
Fluent API extensions must be precisely targeted to avoid polluting unrelated client surfaces.

### The Problem: IntelliSense Noise
Writing extension methods against a high-level facade (e.g., `ILunoClient`) forces all consumers to carry those dependencies. A user only interested in market data should not see trading-specific commands in their IntelliSense.

### The Rule
> **Bind extension methods to the most specific interface possible.**
> Refactor extension methods to target specialized sub-interfaces (e.g., `ILunoTradingClient`) rather than the master facade. This ensures that users only see relevant operations when navigating a specific category, maintaining a clean and focused public API surface.
