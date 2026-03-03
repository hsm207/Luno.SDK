#!/bin/bash
# Luno.SDK - Automated Client Generation Script

set -e

# 1. Patch the source specification with required type corrections
node patch-spec.js

# 2. Generate the Kiota client code
~/.dotnet/tools/kiota generate -l CSharp -d docs/luno_api_spec_engine.json -c LunoApiClient -n Luno.SDK.Infrastructure.Generated -o Luno.SDK.Infrastructure.Generated/Generated --clean-output --ad true

# 3. Cleanup intermediate files
rm docs/luno_api_spec_engine.json

echo "Client generation complete."
