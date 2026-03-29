# API Inconsistency: Unix Timestamp Overflow

## Overview
- **API Version**: v1
- **Affected Endpoints**: `/api/1/listorders`, `/api/1/tickers`, and other endpoints returning order details.
- **Affected Parameters/Properties**:
    - Query Parameter: `created_before` (listorders)
    - Response Property: `creation_timestamp`, `expiration_timestamp`, `completed_timestamp` (Order object)
    - Response Property: `timestamp` (Ticker object)

## Description
The Luno API uses Unix timestamps in **milliseconds**. In the provided OpenAPI specification, these fields were typed as `integer` with a custom `format: timestamp`.

However, 2.147 billion (the max value for a 32-bit signed integer) corresponds to a Unix timestamp in the year 1970 if interpreted as milliseconds. Modern timestamps (e.g., in the 1.6 trillion range) far exceed the capacity of a 32-bit `int`.

Without an explicit `format: int64`, Kiota and other code generators may default to 32-bit integers, leading to:
1. **Overflow** during request serialization (e.g., when passing `created_before`).
2. **Deserialization failures** (e.g., `System.Text.Json` ThrowFormatException) when receiving responses.

## Resolution
We patched the OpenAPI specification in `scripts/patch-spec.js` to force `format: int64` for all Unix millisecond timestamp fields. This ensures the SDK uses 64-bit `long` types, preventing overflows and allowing for accurate time representation.

```javascript
// Example patch log
// Successfully patched 'listorders.created_before' parameter to 'int64'.
// Successfully patched 'Order.creation_timestamp' to 'int64'.
```
