# API Spec Inconsistency: Query Parameter Serialization 🧱🥊

- **API Version**: 1.2.5
- **Endpoint**: `GET /api/1/balance` and `GET /api/1/tickers`
- **Inconsistency**: The parameter descriptions explicitly mandate passing the parameter multiple times (exploded), but the spec defines `explode: false`.

## Details

### 1. Account Balances (`assets` parameter)
- **Description**: "To request balances for multiple currencies, **pass the parameter multiple times**, e.g. `assets=XBT&assets=ETH`."
- **Spec Property**: `"explode": false` (Default for query parameters)
- **Impact**: Kiota generates comma-separated lists (`assets=XBT,ETH`) which results in an HTTP 400 Bad Request from the Luno API.

### 2. Multi-Ticker (`pair` parameter)
- **Description**: "To request tickers for multiple markets, **pass the parameter multiple times**, e.g. `pair=XBTZAR&pair=ETHZAR`."
- **Spec Property**: `"explode": false`
- **Impact**: Same as above, would result in API errors if implemented as per spec.

## Fix Applied
We use our [patch-spec.js](file:///home/user/Documents/GitHub/Luno.SDK/scripts/patch-spec.js) utility to override these properties to `"explode": true` during the SDK generation lifecycle. This keeps our source spec pristine while ensuring the generated client behaves according to the actual API requirements. 🏛️✨💅
