# Lessons Learned: Modern DI and Pipeline Patterns

> **Context**: Phase 17-18 of RFC 006 evolved the SDK from a manual "Composition Root" to a first-class Dependency Injection (DI) architecture with a composable Pipeline.

---

## Lesson 1 — The Tiered Interface Pattern (ISP Enforcement)

### What Was Wrong
Previously, sub-clients like `ILunoTradingClient` were monolithic. They exposed both data-fetching methods (`PostLimitOrderAsync`) and the orchestration hub (`Commands`).
When a Handler needed to call a sub-client, it was injected with the full interface. This created a **recursion trap**: a handler could accidentally dispatch a new command, leading to infinite loops or hidden complexity.

### The Solution: Tiered Segregation
We split sub-clients into two interfaces:
1.  **`Operations` Layer** (e.g., `ILunoTradingOperations`): Pure data-fetching. No `Commands` property.
2.  **`Client` Layer** (e.g., `ILunoTradingClient`): Inherits from `Operations` and adds the `Commands` property.

### The Rule
> **Inject only the capabilities you need.**
> Handlers should be injected with `Operations` interfaces. End-users (Consumers) resolve the `Client` interfaces.
> This makes it **physically impossible** (compiler-enforced) for a handler to trigger an orchestration loop.

---

## Lesson 2 — Composable Pipeline Behaviors (LSP over Decorators)

### What Was Wrong
We used a simple `Func<object, object>` decorator in the `LunoClientOptions`. This was:
- **Weakly Typed**: Required casting and reflection.
- **Fragile**: Hard to chain multiple decorators (Logging + Retry + Validation).
- **Inflexible**: Tied to the initialization of the client.

### The Solution: Pipeline Behaviors
We adopted the `IPipelineBehavior<TRequest, TResponse>` pattern.
- Every behavior is a first-class citizen in the DI container.
- They are chained automatically by the `LunoCommandDispatcher`.
- The `RequestHandlerDelegate<TResponse>` provides a clean continuation mechanism (Middleware).

### The Rule
> **Cross-cutting concerns should be composable, not nested.**
> Use pipelines for logic that applies to *all* commands. Use specialized handlers for logic that applies to *one* command.

---

## Lesson 3 — The Assembly-Scanning Registry (OCP Success)

### What Was Wrong
The `DefaultCommandHandlerFactory` was a manual mapping of Commands to Handlers. Every time we added a new command, we had to update the factory. This was a classic **Open-Closed Principle (OCP)** violation.

### The Solution: Automatic Discovery
The `LunoServiceExtensions.AddLunoClient()` now uses Reflection to scan the Application assembly for anything implementing `ICommandHandler<,>`.

### The Rule
> **The system should grow by adding new classes, not by modifying existing ones.**
> Use assembly scanning for standard patterns (Handlers, Behaviors, Mappers). This ensures the "Composition Root" stays thin and maintenance-free.

---

## Lesson 4 — Breaking Circular Dependencies in DI

### The Challenge
`LunoClient` depends on sub-clients, but sub-clients (via Handlers) might eventually depend on the `ILunoCommandDispatcher`, which is owned by `LunoClient`.

### The Solution: Concrete Registration Tiers
Initially, we used `Lazy<T>` to break cycles. In the refined architecture, we:
1.  Register concrete implementation types (e.g., `services.AddTransient<LunoTradingClient>()`).
2.  Register interfaces by resolving the concrete type (e.g., `services.AddTransient<ILunoTradingOperations>(sp => sp.GetRequiredService<LunoTradingClient>())`).
3.  Register the Master Facade (`LunoClient`) to take these already-registered sub-clients.

### The Rule
> **Circularity is usually a sign of interface leakage.**
> By splitting interfaces (Lesson 1), we naturally reduce the need for circular patterns because Handlers no longer need to "see" the same dispatcher that resolved them.

---

## Summary — The "Clean Architecture" Maturity Matrix

| Phase | Orchestration | Discovery | Interception | Boundary Enforcement |
|---|---|---|---|---|
| **Early** | Manual New-ing | Hardcoded | None | Poor (Monolith) |
| **Middle** | Manual Factory | hardcoded | `Func` Decorator | Moderate (Sub-clients) |
| **Current** | DI Dispatcher | **Assembly Scanning** | **Generic Pipeline** | **Tiered ISP (Compiler-Enforced)** |

This table represents the target maturity state for dependency management and orchestration within the SDK.
