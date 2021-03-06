# poppler 自炊につきまして

[![Build Status](https://dev.azure.com/HiraokaHyperTools/poppler/_apis/build/status/HiraokaHyperTools.poppler_mirror?branchName=dev-ng)](https://dev.azure.com/HiraokaHyperTools/poppler/_build/latest?definitionId=4&branchName=dev-ng)

Windows でのみ有用です。
MSYS2 を使用します。

## ビルド手順

```sh
mkdir build
cd build
cmake -G "MSYS Makefiles" \
 -D CMAKE_INSTALL_PREFIX=~/poppler-release \
 -D CMAKE_BUILD_TYPE=Release \
 -D BUILD_CPP_TESTS:BOOL=OFF \
 -D ENABLE_QT5:BOOL=OFF \
 -D ENABLE_GLIB:BOOL=OFF \
 -D ENABLE_ZLIB_UNCOMPRESS:BOOL=ON \
 ..
make
```

## 手始めに環境構築から

https://www.msys2.org/ にて `msys2-i686-20200517.exe` をダウンロードしてセットアップします。

### MinGW 環境構築

```sh
pacman -S \
 mingw32/mingw-w64-i686-boost \
 mingw32/mingw-w64-i686-cairo \
 mingw32/mingw-w64-i686-cmake \
 mingw32/mingw-w64-i686-freetype \
 mingw32/mingw-w64-i686-gcc \
 mingw32/mingw-w64-i686-jsoncpp \
 mingw32/mingw-w64-i686-libjpeg-turbo \
 mingw32/mingw-w64-i686-make \
 mingw32/mingw-w64-i686-nasm \
 mingw32/mingw-w64-i686-openjpeg2 \
 mingw32/mingw-w64-i686-pkg-config \
 msys/make
```

```sh
pacman -S \
 mingw64/mingw-w64-x86_64-boost \
 mingw64/mingw-w64-x86_64-cairo \
 mingw64/mingw-w64-x86_64-cmake \
 mingw64/mingw-w64-x86_64-freetype \
 mingw64/mingw-w64-x86_64-gcc \
 mingw64/mingw-w64-x86_64-jsoncpp \
 mingw64/mingw-w64-x86_64-libjpeg-turbo \
 mingw64/mingw-w64-x86_64-make \
 mingw64/mingw-w64-x86_64-nasm \
 mingw64/mingw-w64-x86_64-openjpeg2 \
 mingw64/mingw-w64-x86_64-pkg-config \
 msys/make
```

### openjpeg ≠ openjpeg2

CMakeLists.txt にて、あたかも `OpenJPEG` が `openjpeg2` と同一であるかのような仮定がなされているので、その対策です。

問題の箇所:

```txt
if(ENABLE_LIBOPENJPEG STREQUAL "openjpeg2")
  find_package(OpenJPEG)
  set(WITH_OPENJPEG ${OpenJPEG_FOUND})
  if(NOT OpenJPEG_FOUND OR OPENJPEG_MAJOR_VERSION VERSION_LESS 2)
    message(FATAL_ERROR "Install libopenjpeg2 before trying to build poppler. You can also decide to use the internal unmaintained JPX decoder or none at all.")
  endif()
  set(HAVE_JPX_DECODER ON)
```

エラーメッセージ:

```txt
CMake Error at CMakeLists.txt:207 (message):
  Install libopenjpeg2 before trying to build poppler.  You can also decide
  to use the internal unmaintained JPX decoder or none at all.
```

対策:

```sh
cp /mingw32/lib/pkgconfig/libopenjp2.pc /mingw32/lib/pkgconfig/libopenjpeg.pc
```

### `*.notdll.a` 作成

```sh
./notdll.sh /mingw32/lib/*.dll.a
```

参考: [xxx.dll.a ではなく xxx.notdll.a へ](http://dd-kaihatsu-room.blogspot.jp/2018/04/xxxdlla-xxxnotdlla.html)

事前になきものにする:
```
"C:\msys32\mingw32\lib\gcc\i686-w64-mingw32\6.2.0\-libstdc++.dll.a"
"C:\msys32\mingw32\i686-w64-mingw32\lib\-libpthread.dll.a" 
"C:\msys32\mingw32\i686-w64-mingw32\lib\-libwinpthread.dll.a"
```

#### 不毛な `multiple definition of _imp__` の争いの件

着目するべき `CMakeLists.txt` と `utils/CMakeLists.txt` に存在する 2 箇所の問題点:

```CMakeLists.txt
set(poppler_LIBS ${poppler_LIBS} ${LCMS2_LIBRARIES})
```

CMakeLists.txt の修正案は:

```CMakeLists.txt
set(poppler_LIBS ${poppler_LIBS} liblcms2.notdll.a)
```

その理由として CMakeCache.txt を確認:

```CMakeCache.txt
//Path to a library.
LCMS2_LIBRARIES:FILEPATH=C:/msys32/mingw32/lib/liblcms2.dll.a
```

対策案

```CMakeLists.txt
SET(CMAKE_FIND_LIBRARY_SUFFIXES ".a")
```

参考: https://stackoverflow.com/a/24671474

#### _imp__curl_easy_reset の抵抗

結果を視認する事で `__declspec(dllimport)` 適用の嫌疑を持つ:

```
C:/msys32/mingw32/bin/../lib/gcc/i686-w64-mingw32/10.1.0/../../../../i686-w64-mingw32/bin/ld.exe: ../libpoppler-101.a(CurlCachedFile.cc.obj):CurlCachedFile.cc:(.text+0x28c): undefined reference to `_imp__curl_easy_reset'
```

curl.h を確認したい:

```curl.h
/*
 * libcurl external API function linkage decorations.
 */

#ifdef CURL_STATICLIB
#  define CURL_EXTERN
#elif defined(CURL_WIN32) || defined(__SYMBIAN32__) || \
     (__has_declspec_attribute(dllexport) && \
      __has_declspec_attribute(dllimport))
#  if defined(BUILDING_LIBCURL)
#    define CURL_EXTERN  __declspec(dllexport)
#  else
#    define CURL_EXTERN  __declspec(dllimport)
#  endif
#elif defined(BUILDING_LIBCURL) && defined(CURL_HIDDEN_SYMBOLS)
#  define CURL_EXTERN CURL_EXTERN_SYMBOL
#else
#  define CURL_EXTERN
#endif
```

対策:

```CMakeLists.txt
if(MINGW)
  add_definitions(-DCURL_STATICLIB=1)
endif()
```

#### `poppler_LIBS` へ含んで居ても抵抗する件

```
C:/msys32/mingw32/bin/../lib/gcc/i686-w64-mingw32/10.1.0/../../../../i686-w64-mingw32/bin/ld.exe: C:/msys32/mingw32/bin/../lib/gcc/i686-w64-mingw32/10.1.0/../../../../lib\libpsl.a(libpsl_la-psl.o):(.text+0x108a): undefined reference to `_imp__WSAStringToAddressW@20'
```

該当の `libws2_32.a` を `poppler_LIBS` の末尾へ移動します:

```CMakeLists.txt
set(poppler_LIBS ${poppler_LIBS} 
  ...
  libws2_32.a
  # こちらへ
)
```

### `undefined reference to '_imp____acrt_iob_func'` につきまして

<s>https://github.com/HiraokaHyperTools/libacrt_iob_func</s> は不要になりました。

### cygcheck をすると `lib*.dll` につながっています

```txt
  C:\msys32\mingw32\bin\libopenjp2-7.dll
    C:\msys32\mingw32\bin\libgcc_s_dw2-1.dll
      C:\msys32\mingw32\bin\libwinpthread-1.dll
```

変に頑張るとこうなりました ↓

```txt
[ 62%] Linking CXX executable pdfunite.exe
../libpoppler-79.a(JPEG2000Stream.cc.obj): In function `ZN9JPXStream5closeEv':
D:/Git/poppler/poppler/JPEG2000Stream.cc:98: undefined reference to `_imp__opj_image_destroy@4'
D:/Git/poppler/poppler/JPEG2000Stream.cc:98: undefined reference to `_imp__opj_image_destroy@4'
```

対策編です。

#### ${OpenJPEG_LIBRARIES} にします

`utils/CMakeFiles/pdfunite.dir/build.make` を確認したところ、
openjp2 だけダイレクトに `/mingw32/lib/libopenjp2.dll.a` を参照していました。

そこで、

```txt
if (OpenJPEG_FOUND)
  set(poppler_SRCS ${poppler_SRCS}
    poppler/JPEG2000Stream.cc
  )
  set(poppler_LIBS ${poppler_LIBS} openjp2)
```

↑ これを ↓ こうしました。

```txt
if (OpenJPEG_FOUND)
  set(poppler_SRCS ${poppler_SRCS}
    poppler/JPEG2000Stream.cc
  )
  set(poppler_LIBS ${poppler_LIBS} ${OpenJPEG_LIBRARIES})
```

#### section .text にしました

インポートライブラリを比較するのに `objdump -p` では判別できませんでしたが…

`nm` で比較したところ、シンボルタイプの相違に気が付きました。

`__imp__opj_decode@12` のシンボルタイプは `T` text section となっています ↓

```txt
$ nm /mingw32/lib/libopenjp2.dll.a | grep "opj_decode"
00000000 I __imp__opj_decode_tile_data@20
00000000 T _opj_decode_tile_data@20
00000000 I __imp__opj_decode@12
00000000 T _opj_decode@12
```

```txt
$ nm /mingw32/lib/libopenjp2.notdll.a  | grep "opj_decode"
         U __imp__opj_decode@12
         U __imp__opj_decode_tile_data@20
         U _opj_decode
         U _opj_decode_tile_data
```

↑ `section .text` を宣言しません。 `__imp__opj_decode@12` は明らかに `U` undefined です。

↓ `section .text` を宣言します。 `__imp__opj_decode@12` は `T` text section に変化しました。

```txt
$ nm /mingw32/lib/libopenjp2.notdll.a  | grep "opj_decode"
000000b4 T __imp__opj_decode@12
000000b0 T __imp__opj_decode_tile_data@20
         U _opj_decode
         U _opj_decode_tile_data
```

シンボルの型: [nm(1) manページ](https://nxmnpg.lemoda.net/ja/1/nm)
