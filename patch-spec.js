const fs = require('fs');
const path = require('path');

const specPath = path.join(__dirname, 'docs', 'luno_api_spec.json');
const outputPath = path.join(__dirname, 'docs', 'luno_api_spec_engine.json');

console.log(`Reading source specification: ${specPath}`);
const spec = JSON.parse(fs.readFileSync(specPath, 'utf8'));

// 1. Correct Ticker property types
const ticker = spec.components.schemas.Ticker;
if (ticker) {
    ticker.properties.timestamp.format = 'int64';
    ticker.properties.ask.format = 'decimal';
    ticker.properties.bid.format = 'decimal';
    ticker.properties.last_trade.format = 'decimal';
    ticker.properties.rolling_24_hour_volume.format = 'decimal';
}

// 2. Additional type corrections
// (Reserved for future type patches)

console.log(`Writing intermediate patched specification: ${outputPath}`);
fs.writeFileSync(outputPath, JSON.stringify(spec, null, 2));

console.log("Specification successfully patched.");
