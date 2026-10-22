#!/bin/sh
set -eu
cd "$(dirname "$0")/.."
root=$(pwd)
export DOTNET_CLI_HOME="${DOTNET_CLI_HOME:-$root/.cache/dotnet-cli}"
export NUGET_PACKAGES="${NUGET_PACKAGES:-$root/.cache/nuget}"
export DOTNET_CLI_TELEMETRY_OPTOUT=1
if [ -z "${XDG_RUNTIME_DIR:-}" ]; then
  export XDG_RUNTIME_DIR="$root/.cache/runtime"
  mkdir -p "$XDG_RUNTIME_DIR"
  chmod 700 "$XDG_RUNTIME_DIR"
fi
if command -v dotnet >/dev/null 2>&1; then
  exec dotnet "$@"
elif [ -x "$root/.tools/dotnet/dotnet" ]; then
  exec "$root/.tools/dotnet/dotnet" "$@"
else
  echo 'Instale .NET SDK 10 ou execute ./scripts/setup-dotnet.sh.' >&2
  exit 1
fi
