// Copyright 2026 Google LLC
// Licensed under the Apache License, Version 2.0

const fs = require('fs');
const path = require('path');

const specPath = path.join(__dirname, 'docs', 'luno_api_spec.json');
const outputPath = path.join(__dirname, 'docs', 'luno_api_spec_engine.json');

console.log(`📖 Reading Bible: ${specPath}`);
const spec = JSON.parse(fs.readFileSync(specPath, 'utf8'));

// 1. Fix Ticker types! 🤌✨
const ticker = spec.components.schemas.Ticker;
if (ticker) {
    ticker.properties.timestamp.format = 'int64';
    ticker.properties.ask.format = 'decimal';
    ticker.properties.bid.format = 'decimal';
    ticker.properties.last_trade.format = 'decimal';
    ticker.properties.rolling_24_hour_volume.format = 'decimal';
}

// 2. Fix other quirky types! 🕵️‍♀️
// (We can add more fixes here as we discover them!)

console.log(`🚀 Writing Machine Engine Spec: ${outputPath}`);
fs.writeFileSync(outputPath, JSON.stringify(spec, null, 2));

console.log("💅✨ Spec Patched! Slay! ✨💅");
