#!/bin/sh
set -eu
cd "$(dirname "$0")/.."
./scripts/build.sh
./scripts/dotnet.sh src/ValeDoSol/bin/Release/net10.0/ValeDoSol.dll --self-test
