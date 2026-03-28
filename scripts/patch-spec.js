const fs = require('fs');
const path = require('path');

// Reference paths relative to the script location
const specPath = path.join(__dirname, '..', 'docs', 'luno_api_spec.json');
const outputPath = path.join(__dirname, '..', 'docs', 'luno_api_spec_engine.json');

console.log(`Reading source specification: ${specPath}`);
const spec = JSON.parse(fs.readFileSync(specPath, 'utf8'));

// 1. Correct Ticker and GetTickerResponse Timestamp types
// Kiota inconsistently maps "format: timestamp" to int or long.
// We force int64 to ensure 13-digit millisecond timestamps never overflow.
const ticker = spec.components.schemas.Ticker;
if (ticker && ticker.properties.timestamp) {
    ticker.properties.timestamp.format = 'int64';
    console.log("Successfully patched 'Ticker.timestamp' to 'int64'.");
}

const getTickerResponse = spec.components.schemas.GetTickerResponse;
if (getTickerResponse && getTickerResponse.properties.timestamp) {
    getTickerResponse.properties.timestamp.format = 'int64';
    console.log("Successfully patched 'GetTickerResponse.timestamp' to 'int64'.");
}

// 2. Fix the account balance 'assets' parameter serialization
// Luno API documentation explicitly mandates passing the parameter multiple times
// (e.g., assets=XBT&assets=ETH) which requires 'explode: true' in OpenAPI 3.0.
const balancePath = spec.paths['/api/1/balance'];
if (balancePath && balancePath.get && balancePath.get.parameters) {
    const assetsParam = balancePath.get.parameters.find(p => p.name === 'assets');
    if (assetsParam) {
        assetsParam.explode = true;
        console.log("Successfully patched 'assets' parameter to 'explode: true'.");
    }
}

// 3. Fix the 'tickers' endpoint 'pair' parameter serialization
// Same as balance, description mandates multiple params (pair=XBTZAR&pair=ETHZAR)
// which requires 'explode: true'.
const tickersPath = spec.paths['/api/1/tickers'];
if (tickersPath && tickersPath.get && tickersPath.get.parameters) {
    const pairParam = tickersPath.get.parameters.find(p => p.name === 'pair');
    if (pairParam) {
        pairParam.explode = true;
        console.log("Successfully patched 'pair' parameter to 'explode: true'.");
    }
}

// 4. Patch /api/1/listorders created_before to support 64-bit integers (Unix ms)
const listOrdersPath = spec.paths['/api/1/listorders'];
if (listOrdersPath && listOrdersPath.get && listOrdersPath.get.parameters) {
  const createdBeforeParam = listOrdersPath.get.parameters.find(p => p.name === 'created_before');
  if (createdBeforeParam && createdBeforeParam.schema) {
    createdBeforeParam.schema.format = 'int64';
    console.log("Successfully patched 'listorders.created_before' parameter to 'int64'.");
  }
}

// 5. Patch Order component timestamps
const orderSchema = spec.components.schemas['Order'];
if (orderSchema && orderSchema.properties) {
  ['completed_timestamp', 'creation_timestamp', 'expiration_timestamp'].forEach(prop => {
    if (orderSchema.properties[prop]) {
      orderSchema.properties[prop].format = 'int64';
      console.log(`Successfully patched 'Order.${prop}' to 'int64'.`);
    }
  });
}

// 7. Patch GetOrder2Response component timestamps
const getOrder2ResponseSchema = spec.components.schemas['GetOrder2Response'];
if (getOrder2ResponseSchema && getOrder2ResponseSchema.properties) {
  ['completed_timestamp', 'creation_timestamp', 'expiration_timestamp'].forEach(prop => {
    if (getOrder2ResponseSchema.properties[prop]) {
      getOrder2ResponseSchema.properties[prop].format = 'int64';
      console.log(`Successfully patched 'GetOrder2Response.${prop}' to 'int64'.`);
    }
  });
}

// 8. Patch OrderV2 component timestamps
// Verification: The fresh spec has 'int64' for Account IDs, but 'timestamp' for these!
const orderV2Schema = spec.components.schemas['OrderV2'];
if (orderV2Schema && orderV2Schema.properties) {
  ['completed_timestamp', 'creation_timestamp', 'expiration_timestamp'].forEach(prop => {
    if (orderV2Schema.properties[prop]) {
      orderV2Schema.properties[prop].format = 'int64';
      console.log(`Successfully patched 'OrderV2.${prop}' to 'int64'.`);
    }
  });
}

// 9. Patch /api/1/postorder query parameters to support 64-bit integers (Unix ms)
const postOrderPath = spec.paths['/api/1/postorder'];
if (postOrderPath && postOrderPath.post && postOrderPath.post.parameters) {
  ['timestamp', 'ttl'].forEach(paramName => {
    const param = postOrderPath.post.parameters.find(p => p.name === paramName);
    if (param && param.schema) {
      param.schema.format = 'int64';
      console.log(`Successfully patched 'postorder.${paramName}' parameter to 'int64'.`);
    }
  });
}

// 10. Fix the '/api/exchange/1/markets' endpoint 'pair' parameter serialization
// Mandates multiple params (pair=XBTZAR&pair=ETHZAR) which requires 'explode: true'.
const marketsPath = spec.paths['/api/exchange/1/markets'];
if (marketsPath && marketsPath.get && marketsPath.get.parameters) {
    const pairParam = marketsPath.get.parameters.find(p => p.name === 'pair');
    if (pairParam) {
        pairParam.explode = true;
        console.log("Successfully patched 'markets.pair' parameter to 'explode: true'.");
    }
}

console.log(`Writing intermediate patched specification: ${outputPath}`);
fs.writeFileSync(outputPath, JSON.stringify(spec, null, 2));

console.log("Specification successfully patched.");
