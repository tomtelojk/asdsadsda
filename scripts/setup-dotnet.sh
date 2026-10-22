#!/bin/sh
set -eu
cd "$(dirname "$0")/.."
if [ "$(uname -s)" != Linux ] || [ "$(uname -m)" != x86_64 ]; then
  echo 'Este instalador local requer Linux x86_64. Instale o SDK oficial para sua plataforma.' >&2
  exit 1
fi
if [ -x .tools/dotnet/dotnet ] && [ -d .tools/dotnet/sdk/10.0.402 ]; then exit 0; fi
archive=$(mktemp /tmp/vale-dotnet.XXXXXX)
trap 'rm -f "$archive"' EXIT HUP INT TERM
curl --fail --location --show-error --proto '=https' --tlsv1.2 \
  https://builds.dotnet.microsoft.com/dotnet/Sdk/10.0.402/dotnet-sdk-10.0.402-linux-x64.tar.gz -o "$archive"
printf '%s  %s\n' 1904a98dda2db34ba78ba3c4fb1b224455329af91c341d83547843c1839e112b04ae699ee92fa2449a8f62249196a938723078c0c60b9f65c88539d5687729c2 "$archive" | sha512sum -c -
mkdir -p .tools/dotnet
tar -xzf "$archive" -C .tools/dotnet
