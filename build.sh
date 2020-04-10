#!/bin/bash

set -eux

rm -rf build
mkdir build
cd build

cmake -G "MSYS Makefiles" \
  -D CMAKE_INSTALL_PREFIX=~/poppler-release \
  -D CMAKE_BUILD_TYPE=Release \
  -D BUILD_CPP_TESTS:BOOL=OFF \
  -D ENABLE_QT5:BOOL=OFF \
  -D ENABLE_GLIB:BOOL=OFF \
  -D ENABLE_ZLIB_UNCOMPRESS:BOOL=ON \
  -DFREETYPE_LIBRARY=/mingw32/lib/libfreetype.a \
  -DFREETYPE_INCLUDE_DIRS=/mingw32/include/freetype2 \
  ..

make

strip utils/*.exe
