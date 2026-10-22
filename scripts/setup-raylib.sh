#!/bin/sh
set -eu
cd "$(dirname "$0")/.."
if [ "$(uname -s)" != Linux ] || [ "$(uname -m)" != x86_64 ]; then
  echo 'Este pacote de dependências requer Linux x86_64.' >&2
  exit 1
fi
if [ -f vendor/raylib-6.2_linux_amd64/lib/libraylib.so.6.2.0 ]; then exit 0; fi
archive=$(mktemp /tmp/vale-raylib.XXXXXX)
trap 'rm -f "$archive"' EXIT HUP INT TERM
curl --fail --location --show-error --proto '=https' --tlsv1.2 \
  https://github.com/raysan5/raylib/releases/download/6.2/raylib-6.2_linux_amd64.tar.gz -o "$archive"
printf '%s  %s\n' 586bbc32f3596d439382b1c5dbc6c1c573eb7e1d00fe92be7d4deb116883720d "$archive" | sha256sum -c -
mkdir -p vendor
tar -xzf "$archive" -C vendor
