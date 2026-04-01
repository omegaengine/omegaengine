#!/usr/bin/env bash
set -e
cd `dirname $0`

# Build
dotnet msbuild -v:Quiet -t:Restore -t:Build -p:Configuration=Release -p:Version=${1:-1.0.0-pre}
