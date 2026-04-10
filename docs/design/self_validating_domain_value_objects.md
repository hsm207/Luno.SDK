# Design Rule: Self-Validating Domain Value Objects (DVOs)

## Context
In the Luno SDK, we prioritize **Fail-Fast** principles and **Data Integrity**. One of the most common points of contention in SOLID audits is whether a data structure should contain a `Validate()` method.

## The Rule
**Domain Value Objects (DVOs) SHOULD be self-validating.**

A DVO is responsible for enforcing its own structural invariants. If a set of parameters is internally contradictory or invalid by definition, that object should not be allowed to exist in a valid state.

### 1. Structural Invariants vs. Business Rules
We distinguish between two types of validation:

| Type | Responsibility | Example | Location |
| :--- | :--- | :--- | :--- |
| **Structural Invariant** | The Data Itself | `PostOnly` cannot be used with `TimeInForce.IOC`. | Inside the DVO (`Validate()`) |
| **Contextual Rule** | The System State | "User must have enough balance to place this order." | Application Handler / Service |

**Structural invariants** belong inside the DVO. An "Order" that is both `PostOnly` and `IOC` is a logical impossibility; the object should know this about itself.

### 2. The Boundary Mandate
While DVOs can (and should) have behavior, they **MUST NOT** cross architectural boundaries (e.g., Infrastructure interfaces).

- **Inside the Boundary**: Use self-validating DVOs to ensure integrity.
- **Across the Boundary**: Map the DVO to a **Behavior-Free DTO** (POCO).

> [!IMPORTANT]
> Infrastructure should be a "dumb pipe." It should not be coupled to the `Validate()` method of a Domain object. Crossing a seam with an object that has methods is a boundary violation.

### 3. Rationale
- **Prevents Temporal Coupling**: If validation is external, a developer might forget to call the validator before using the object. Self-validation ensures an object is always "ready for use."
- **Cohesion**: The rules that define what a "Limit Order" *is* are co-located with the data that represents it.
- **Discoverability**: Validation rules are found exactly where the data is defined, rather than hidden in a separate `Validator` class.

## Example
```csharp
public record LimitOrderParameters
{
    public required decimal Price { get; init; }
    public bool PostOnly { get; init; }
    public TimeInForce TimeInForce { get; init; }

    public void Validate()
    {
        if (PostOnly && TimeInForce == TimeInForce.IOC)
            throw new LunoValidationException("PostOnly is incompatible with IOC.");
    }
}
```

In the Application layer:
1. Receive request.
2. Initialize `LimitOrderParameters`.
3. Call `Validate()`.
4. Map to `LimitOrderRequest` (Dumb DTO) for the Infrastructure call.
