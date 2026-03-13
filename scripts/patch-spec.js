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
if (ticker) {
    ticker.properties.timestamp.format = 'int64';
}

const getTickerResponse = spec.components.schemas.GetTickerResponse;
if (getTickerResponse) {
    getTickerResponse.properties.timestamp.format = 'int64';
}

console.log(`Writing intermediate patched specification: ${outputPath}`);
fs.writeFileSync(outputPath, JSON.stringify(spec, null, 2));

console.log("Specification successfully patched.");
