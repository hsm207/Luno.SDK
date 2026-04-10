# Lesson: Liskov Substitution Principle (LSP) in Interface Contracts

## 1. Behavioral Integrity in Abstractions
Subclasses and implementations must conform strictly to the behavioral contracts defined by their parent abstractions. 

### The Anti-Pattern: Over-Promising in Documentation
Documenting that an interface method (e.g., `ILunoTradingClient.GetOrderAsync`) throws a specific domain exception (e.g., `LunoValidationException`) when the concrete infrastructure implementation does not actually enforce that check (e.g., delegating it to the API). This creates a trap for consumers who expect the implementation to adhere to the documented abstraction contract.

### The Rule
> **Documentation is part of the contract.**
> Do not document behavior or exceptions on an interface that the concrete implementation cannot or does not fulfill. If an invariant is enforced by an orchestrator (like a Command Handler) rather than the client itself, that validation should not be documented as a behavioral guarantee of the low-level interface. 

---

## 2. Exception Normalization
Providing consistent exception types across all implementations of an interface ensures that the consumer can substitute one implementation for another without changing their error-handling logic.

### The Rule
> **Infrastructure must normalize all boundary failures.**
> Every implementation of an interface must translate external failures (e.g., HTTP errors, null responses, or network timeouts) into the designated domain exception hierarchy. Leaking framework-level exceptions (e.g., `NullReferenceException`) violates the LSP as it forces consumers to depend on the implementation details of a specific subtype.
