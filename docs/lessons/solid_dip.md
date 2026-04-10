# Lesson: Dependency Inversion Principle (DIP) and SDK Orchestration

## 1. The Command Dispatcher Pattern
To decouple high-level application logic from concrete implementations without forcing complex Dependency Injection (DI) requirements on the consumer, the SDK utilizes a centralized Command Dispatcher.

### The Problem: Coupling via "New-ing Up"
Instantiating handlers directly within static extension methods (e.g., `var handler = new PostLimitOrderHandler(client)`) couples the Public API directly to a specific implementation. This makes it impossible to inject cross-cutting concerns (like retry policies or logging) without modifying every entry point.

### The Solution
The extensions call a `DispatchAsync<TRequest, TResponse>(command)` method on an orchestrator. This dispatcher maps the command to its corresponding handler (via a factory or assembly scanning), allowing the system to grow by adding new classes rather than modifying the orchestration logic.

---

## 2. Breaking Circular Dependencies
In a complex SDK, circular dependencies often arise between the Master Facade (which needs sub-clients) and sub-clients/handlers (which might need the dispatcher or other clients).

### The Solution: Registration Tiers
Breaking circularity requires separating interface registration from concrete instantiation:
1.  Register concrete types (e.g., `LunoTradingClient`) first.
2.  Register interfaces by resolving from the concrete type (e.g., `services.AddTransient<ILunoTradingOperations>(sp => sp.GetRequiredService<LunoTradingClient>())`).
3.  Ensure the Master Facade (`LunoClient`) takes the already-registered sub-clients as dependencies.

---

## 3. Automatic Discovery (Open-Closed Principle)
Manual mapping of commands to handlers is a violation of the Open-Closed Principle (OCP), as every new feature requires modifying the factory.

### The Rule
> **The system should grow by adding new classes, not by modifying existing ones.**
> Utilize assembly scanning to automatically discover and register types that implement standard patterns (e.g., `ICommandHandler<,>`, `IPipelineBehavior<,>`). This ensures the "Composition Root" remains maintenance-free as the feature set expands.
