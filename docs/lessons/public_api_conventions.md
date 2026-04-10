# Lesson: Public API Naming Conventions and the Shadowing Trap

## 1. Differentiating Layer Responsibilities via Verbs
To prevent ambiguity in the public API, consistent naming conventions must be used to distinguish between low-level infrastructure operations and high-level application orchestrations.

### The Problem: The Shadowing Trap (Instance vs. Extension)
In C#, instance methods are preferred over extension methods with the same signature. If an infrastructure interface method (e.g., `ILunoMarketClient.GetTickersAsync`) shares the same name as an application-layer extension method, the "raw" infrastructure method will shadow the extension. This often leads to unexpected behavior and type mismatches for the consumer.

### The Rule
> **Public APIs should use clear, unambiguous naming conventions across layers.**
> Use consistent verbs to differentiate the operational context:
> - **Infrastructure Gateways** (Infrastructure Layer) use the verb `Fetch*` (e.g., `FetchTickersAsync`, `FetchOrderAsync`). These methods represent raw data acquisition.
> - **Application Extensions** (Public SDK Surface) use the verb `Get*` (e.g., `GetTickersAsync`, `GetOrderAsync`). These methods represent orchestrated high-level features.
> 
> This "Fetch/Get" distinction ensures that IntelliSense remains unambiguous and that the public API correctly routes calls to the intended orchestration layer.
