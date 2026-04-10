# Lesson: Single Responsibility Principle (SRP) in Clean Architecture

## 1. Boundary Integrity: Domain Value Objects (DVO)
Domain artifacts should never cross architectural boundaries. Seams between layers (e.g., Application to Infrastructure) must be crossed using behavior-free Plain Old CLR Objects (POCOs).

### The Anti-Pattern
Injecting a Domain Value Object (specifically one with a `Validate()` method or business logic) into an infrastructure-layer interface (e.g., `ILunoTradingClient.PostLimitOrderAsync`). This couples infrastructure implementation to domain behavior rather than just domain data. Changes to validation rules—a domain concern—unnecessarily force infrastructure recompilation.

### The Rule
> **Cross boundaries with passive data structures only.**
> If a type contains logic beyond basic data storage (e.g., `Validate()`, computed properties, or non-trivial overrides), it belongs exclusively inside the Application layer. The Application layer orchestrates validation and subsequently maps the data to a passive boundary DTO for external transmission.

---

## 2. Policy Isolation: Business-Equivalence Logic
Determining if two entities represent the same logical unit (e.g., idempotency reconciliation for orders) is an Application-layer policy decision, not an infrastructure concern.

### The Anti-Pattern
Embedding comparison logic (e.g., checking if `Price`, `Volume`, and `Side` match) within an infrastructure client or mapper. This makes business rules untestable in isolation, invisible to the Application layer, and coupled to specific API response shapes.

### The Rule
> **Infrastructure is a translation bridge, not a decision-maker.**
> Transition logic that involves branching on domain values or policy decisions belongs in the Application layer. Infrastructure should remain a "dumb pipe" that translates requests to external protocols and normalizes responses to domain models.

---

## 3. Component Hygiene: File-Level SRP
The Single Responsibility Principle applies at the file and component level as well as the class level.

### The Anti-Pattern
Utilizing "Junk Drawer" files (e.g., `ApplicationAbstractions.cs` or `CommonInterfaces.cs`) to house unrelated architectural concerns such as dispatching, interception, and execution.

### The Rule
> **One file, one primary architectural concern.**
> Discrete features (e.g., `ILunoCommandDispatcher`, `IPipelineBehavior`, `ICommandHandler`) should reside in dedicated files. This improves project navigability, prevents the accumulation of unrelated dependencies, and maintains component-level isolation.
