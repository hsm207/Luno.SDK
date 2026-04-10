# Lesson: Composable Pipeline Behaviors in Orchestration

## 1. Pipelines vs. Static Decorators
For global cross-cutting concerns (e.g., logging, validation, resilience), a composable pipeline is superior to a chain of hardcoded decorators.

### The Problem: Weakly Typed Composition
Traditional decorators often rely on `Func<object, object>` or proxy types that require reflection and manual casting. These are difficult to chain (e.g., Logging + Retry + Validation) and create brittle code that is hard to maintain as the system grows.

### The Solution: Composable Behaviors
Utilizing the `IPipelineBehavior<TRequest, TResponse>` pattern allows behaviors to be registered as first-class citizens in a Dependency Injection (DI) container. They are automatically chained by the Command Dispatcher using a middleware-style `RequestHandlerDelegate<TResponse>`.

### The Rule
> **Cross-cutting concerns must be composable, not nested.**
> Use pipeline behaviors for logic that applies to all commands (e.g., telemetry, global invariants). Use specialized handlers for logic that applies to a single command. This ensures that orchestration policy is centralized and can be extended without modifying the execution logic of individual features.

---

## 2. Segregation for Native AOT Support
In a modern architectural environment (such as .NET Native AOT), specific care must be taken to avoid dynamic dispatch within the pipeline.

### The Challenge
A single generic `IPipelineBehavior` that attempts to intercept both scalar `Task<T>` and streaming `IAsyncEnumerable<T>` responses often requires `dynamic` dispatch or complex reflection, which is incompatible with Native AOT.

### The Rule
> **Strictly segregate scalar and streaming interception layers.**
> Use `IPipelineBehavior` for standard request-response flows and a dedicated `IStreamPipelineBehavior` for streaming flows. This preserves architectural purity, improves performance, and ensures the SDK remains compatible with reflection-free compilation targets.
