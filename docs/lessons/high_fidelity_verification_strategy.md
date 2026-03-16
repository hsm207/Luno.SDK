# Lesson 06: The "Less is More" High-Fidelity Verification Strategy

## Context
In previous sprints, we frequently fell into the trap of writing redundant, low-fidelity unit tests for every layer of the application (Extensions, Handlers, Core, Infrastructure). This resulted in high maintenance overhead and a "brittle test" problem without significantly increasing confidence in the system's external behavior.

## The Principle
**Aim for 100% behavioral coverage with the minimum number of high-fidelity tests.**

We prioritize verification at the **Architectural Boundary**. A single, high-fidelity integration test that exercises the entire stack is superior to multiple isolated unit tests that pass but fail to catch integration errors (like incorrect parameter mapping to the underlying API client).

## Guidelines

### 1. Verification Tiers
We distinguish between three types of verification, prioritizing Tier 2 for feature validation.

| Tier | Type | Focus | Use Case |
| :--- | :--- | :--- | :--- |
| **Tier 1** | **Unit** | Pure Logic & Invariants | Structural validation, complex algorithms, math. |
| **Tier 2** | **Integration** | **The Boundary Handshake** | Feature verification, error mapping, API plumbing. |
| **Tier 3** | **End-to-End** | Full System Flow | Final smoke tests (rare/expensive). |

### 2. Why Integration (Tier 2) Wins
By testing through the Application Extension methods and mocking the network at the WireMock boundary:
- We verify the **Fluent API** (Extensions).
- We verify the **Command Orchestration** (Handlers).
- We verify the **Business Policy** (Core).
- We verify the **Implementation Detail** (Infrastructure/Kiota Mapping).
- We verify **Error Resilience** (Adapter logic).

### 3. Redundancy is a Smell
If an integration test already covers the successful path and error mapping for a feature, **do not write unit tests** for the handler or extensions unless they contain complex branching logic that is hard to hit via integration.

> [!IMPORTANT]
> A "Green" unit test for a handler is a false signal if the Infrastructure client it's talking to has a mapping bug. Only the network-boundary test (Tier 2) provides high-fidelity truth.

## Case Study: Account Balance Filtering
Instead of:
1. `GetBalancesQueryTests` (Unit)
2. `GetBalancesHandlerTests` (Unit)
3. `LunoAccountClientTests` (Integration)

We write:
1. **`LunoAccountClient_IntegrationTests`**: One test that calls `client.Account.GetBalancesAsync(assets)` and uses WireMock to assert that the `?assets=...` query parameter is actually sent.

## Rationale
- **Development Speed**: Write less code, get more confidence.
- **Maintainability**: Refactoring internals (like changing a handler implementation) doesn't break tests as long as the behavior is preserved.
- **Accuracy**: Tests the system exactly how the user will use it.
