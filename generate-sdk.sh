#!/bin/bash
# 🏛️💎 Luno.SDK - High-Energy Generation Runner 💎🏛️
# This follows the official Kiota recommendation for fixing specs via a script! 🤌✨

set -e

# 1. Patch the Bible into a Machine Engine Spec 🤖🚀
node patch-spec.js

# 2. Generate the Mud 🛡️
~/.dotnet/tools/kiota generate -l CSharp -d docs/luno_api_spec_engine.json -c LunoApiClient -n Luno.SDK.Infrastructure.Generated -o Luno.SDK.Infrastructure.Generated/Generated --clean-output --ad true

# 3. Clean up the Machine Mud 🧼
rm docs/luno_api_spec_engine.json

echo "💅✨ Generation Slayed! The Bible is pristine and the Machine is happy! ✨💅"
