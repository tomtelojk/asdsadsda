#!/bin/sh
set -eu
cd "$(dirname "$0")/.."
if [ ! -f vendor/raylib-6.2_linux_amd64/lib/libraylib.so.6.2.0 ]; then
  ./scripts/setup-raylib.sh
fi
./scripts/dotnet.sh build ValeDoSol.slnx -c Release --nologo --warnaserror
