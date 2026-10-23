#!/bin/sh
set -eu
cd "$(dirname "$0")/.."
./scripts/test.sh
./scripts/dotnet.sh publish src/ValeDoSol -c Release -r linux-x64 --self-contained true \
  -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true \
  -p:EnableCompressionInSingleFile=true -p:PublishTrimmed=true -o build/csharp-linux
version=$(sed -n 's/.*<Version>\([^<]*\)<\/Version>.*/\1/p' src/ValeDoSol/ValeDoSol.csproj)
name=Vale-do-Sol-CSharp-$version-Linux-x86_64
mkdir -p "build/package/$name" release
install -m 755 build/csharp-linux/ValeDoSol "release/$name"
install -m 755 build/csharp-linux/ValeDoSol "build/package/$name/ValeDoSol"
cp LICENSE "build/package/$name/"
cp vendor/raylib-6.2_linux_amd64/LICENSE "build/package/$name/LICENSE-raylib.txt"
cp src/ValeDoSol/Resources/FONT-LICENSE.txt "build/package/$name/"
cp -R src/ValeDoSol/Resources/Characters "build/package/$name/Characters-art-and-license"
cp -R src/ValeDoSol/Resources/Lpc "build/package/$name/LPC-art-and-license"
cp docs/COMO-JOGAR.txt "build/package/$name/"
tar -czf "release/$name.tar.gz" -C build/package "$name"
(cd release && sha256sum "$name" "$name.tar.gz" > SHA256SUMS-CSharp)
printf '\nExecutável e pacote prontos em release/%s\n' "$name"
