#!/bin/sh
# Linux (mono) da yadro testlari. Windows'da build.cmd ishlatiladi.
set -e
cd "$(dirname "$0")"
mkdir -p out
mcs -langversion:5 -codepage:utf8 -target:library -out:out/YangiAlifbo.Core.dll src/Core/*.cs
mcs -langversion:5 -codepage:utf8 -target:exe -r:out/YangiAlifbo.Core.dll -out:out/CoreTests.exe tests/CoreTests.cs
if [ "$1" = "test" ]; then mono out/CoreTests.exe; fi
