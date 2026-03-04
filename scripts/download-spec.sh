#!/bin/bash
# Luno.SDK - API Specification Download Script

set -e

SPEC_URL="https://www.luno.com/en/developers/api/spec.json"
OUTPUT_PATH="docs/luno_api_spec.json"

echo "📡 Downloading Luno API Specification..."
curl -s -o "$OUTPUT_PATH" "$SPEC_URL"

echo "✅ Specification successfully downloaded to $OUTPUT_PATH"
