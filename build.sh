#!/bin/bash

if [ -z "$1" ]; then echo "Specify variant"; exit 1; fi

set -eux

mkdir -p $BinariesDirectory/poppler-build/$1 && cd "$_"

rm -rf $BinariesDirectory/poppler-release/$1 || true

cmake -G "MSYS Makefiles" \
  -D CMAKE_INSTALL_PREFIX=$BinariesDirectory/poppler-release/$1 \
  -D CMAKE_BUILD_TYPE=Release \
  -D BUILD_CPP_TESTS:BOOL=OFF \
  -D ENABLE_QT5:BOOL=OFF \
  -D ENABLE_GLIB:BOOL=OFF \
  -D ENABLE_ZLIB_UNCOMPRESS:BOOL=ON \
  $SourcesDirectory

make install

find $BinariesDirectory/poppler-release -name "*.exe" -print -exec strip {} \;
