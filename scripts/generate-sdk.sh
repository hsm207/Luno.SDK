#!/bin/bash
# Luno.SDK - Automated Client Generation Script

set -e

# Get the directory of the script
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
ROOT_DIR="$( cd "$SCRIPT_DIR/.." && pwd )"

# 1. Patch the source specification with required type corrections
node "$SCRIPT_DIR/patch-spec.js"

# 2. Generate the Kiota client code
# Note: Using relative paths from ROOT_DIR for Kiota output
cd "$ROOT_DIR"
~/.dotnet/tools/kiota generate -l CSharp -d docs/luno_api_spec_engine.json -c LunoApiClient -n Luno.SDK.Infrastructure.Generated -o Luno.SDK.Infrastructure.Generated/Generated --clean-output --ad true

# 3. Cleanup intermediate files
rm docs/luno_api_spec_engine.json

echo "✅ Client generation complete."
