#!/bin/bash
set -e
cd `dirname $0`

rm -rf ../artifacts/Documentation
mkdir -p ../artifacts/Documentation

# Download tag files for external references
curl -o nanobyte-common.tag https://common.nano-byte.net/nanobyte-common.tag

0install run http://repo.roscidus.com/devel/doxygen OmegaEngine.Doxyfile
0install run http://repo.roscidus.com/devel/doxygen AlphaFramework.Doxyfile
0install run http://repo.roscidus.com/devel/doxygen FrameOfReference.Doxyfile

cp index.html .nojekyll CNAME ../artifacts/Documentation/