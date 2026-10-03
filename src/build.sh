#!/usr/bin/env bash
set -e
cd `dirname $0`

echo "Build binaries"
dotnet msbuild -v:Quiet -restore -t:Build -p:Configuration=Release -p:Version=${1:-1.0.0-pre}
