#!/bin/bash
set -e
cd `dirname $0`

echo "Downloading references to other documentation..."
curl -sS -o nanobyte-common.tag https://common.nano-byte.net/nanobyte-common.tag

rm -rf ../artifacts/Documentation
mkdir -p ../artifacts/Documentation

VERSION=${1:-1.0-dev}
./_0install.sh run http://repo.roscidus.com/devel/doxygen OmegaEngine.Doxyfile
./_0install.sh run http://repo.roscidus.com/devel/doxygen AlphaFramework.Doxyfile
./_0install.sh run http://repo.roscidus.com/devel/doxygen FrameOfReference.Doxyfile
cp index.html ../artifacts/Documentation/
