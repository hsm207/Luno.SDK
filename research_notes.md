# Luno API Specification Research: Orders

Research conducted to verify "stopping conditions" and parameter requirements for order management.

## 1. Stop Order (Cancellation)
**Endpoint:** `POST /api/1/stoporder`

> "Request to cancel an Order. Once an Order has been completed, it cannot be reversed. The return value from this request will indicate if the Stop request was successful or not."

### Required Parameters
- `order_id` (string): The Order identifier as a string.

**Citations:**
- SDK: `Luno.SDK.Infrastructure.Generated.Api.One.Stoporder.StoporderRequestBuilder`

---

## 2. Post Limit Order (Activation Conditions)
**Endpoint:** `POST /api/1/postorder`

### Stop-Limit activation rules
If `stop_price` is provided, the order is treated as a **Stop-Limit Order** (trigger order). The following rules apply:

- **Stop Price Requirement:** `stop_price` (string) - "Trigger trade price to activate this order as a decimal string."
- **Stop Direction Requirement:** `stop_direction` (string) - "Side of the trigger price to activate the order. **This should be set if `stop_price` is also set.**"

### Values for Stop Direction
- `RELATIVE_LAST_TRADE`: Automatically infer direction based on last trade price.
- `ABOVE`: Activate when price moves above `stop_price`.
- `BELOW`: Activate when price moves below `stop_price`.

**Citations:**
- SDK: `Luno.SDK.Infrastructure.Generated.Api.One.Postorder.PostorderRequestBuilderPostQueryParameters`
- API Reference: [Luno API - Post Limit Order](https://www.luno.com/en/developers/api#operation/postLimitOrder)

---

## 3. Order Lookup (Reconciliation)
**Endpoint:** `GET /api/exchange/3/order`

Used for idempotency reconciliation and lookup by `client_order_id`.

- **Parameters:** Must provide exactly one of `id` or `client_order_id`.

**Citations:**
- SDK: `Luno.SDK.Infrastructure.Generated.Api.Exchange.Three.Order.OrderRequestBuilderGetQueryParameters`
