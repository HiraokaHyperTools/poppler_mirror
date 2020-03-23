# poppler �����ɂ��܂���

Windows �ł̂ݗL�p�ł��B
MSYS2 ���g�p���܂��B

## �r���h�菇

```sh
mkdir mingw32
cd mingw32
cmake -G "MSYS Makefiles"  -D BUILD_CPP_TESTS:BOOL=OFF -D BUILD_GTK_TESTS:BOOL=OFF -D BUILD_QT5_TESTS:BOOL=OFF -D ENABLE_QT5:BOOL=OFF -D ENABLE_GLIB:BOOL=OFF  ..
make
```

## ��n�߂Ɋ��\�z����

https://www.msys2.org/ �ɂ� `msys2-i686-20161025.exe` ���_�E�����[�h���ăZ�b�g�A�b�v���܂��B

### MinGW ���\�z

```sh
pacman -S mingw32/mingw-w64-i686-freetype
pacman -S mingw32/mingw-w64-i686-gcc
pacman -S mingw32/mingw-w64-i686-make
pacman -S msys/make
pacman -S mingw32/mingw-w64-i686-libjpeg-turbo
pacman -S mingw32/mingw-w64-i686-openjpeg2
pacman -S mingw32/mingw-w64-i686-cmake
pacman -S mingw32/mingw-w64-i686-pkg-config
pacman -S mingw32/mingw-w64-i686-jsoncpp
```

### openjpeg �� openjpeg2

CMakeLists.txt �ɂāA�������� `OpenJPEG` �� `openjpeg2` �Ɠ���ł��邩�̂悤�ȉ��肪�Ȃ���Ă���̂ŁA���̑΍�ł��B

���̉ӏ�:

```txt
if(ENABLE_LIBOPENJPEG STREQUAL "openjpeg2")
  find_package(OpenJPEG)
  set(WITH_OPENJPEG ${OpenJPEG_FOUND})
  if(NOT OpenJPEG_FOUND OR OPENJPEG_MAJOR_VERSION VERSION_LESS 2)
    message(FATAL_ERROR "Install libopenjpeg2 before trying to build poppler. You can also decide to use the internal unmaintained JPX decoder or none at all.")
  endif()
  set(HAVE_JPX_DECODER ON)
```

�G���[���b�Z�[�W:

```txt
CMake Error at CMakeLists.txt:207 (message):
  Install libopenjpeg2 before trying to build poppler.  You can also decide
  to use the internal unmaintained JPX decoder or none at all.
```

�΍�:

```sh
cp /mingw32/lib/pkgconfig/libopenjp2.pc /mingw32/lib/pkgconfig/libopenjpeg.pc
```

### `*.notdll.a` �쐬

```sh
./notdll.sh /mingw32/lib/*.dll.a
```

�Q�l: [xxx.dll.a �ł͂Ȃ� xxx.notdll.a ��](http://dd-kaihatsu-room.blogspot.jp/2018/04/xxxdlla-xxxnotdlla.html)

���O�ɂȂ����̂ɂ���:
```
"C:\msys32\mingw32\lib\gcc\i686-w64-mingw32\6.2.0\-libstdc++.dll.a"
"C:\msys32\mingw32\i686-w64-mingw32\lib\-libpthread.dll.a" 
"C:\msys32\mingw32\i686-w64-mingw32\lib\-libwinpthread.dll.a"
```

### `undefined reference to '_imp____acrt_iob_func'` �ɂ��܂���

https://github.com/HiraokaHyperTools/libacrt_iob_func

### cygcheck ������� `lib*.dll` �ɂȂ����Ă��܂�

```txt
  C:\msys32\mingw32\bin\libopenjp2-7.dll
    C:\msys32\mingw32\bin\libgcc_s_dw2-1.dll
      C:\msys32\mingw32\bin\libwinpthread-1.dll
```

�ςɊ撣��Ƃ����Ȃ�܂��� ��

```txt
[ 62%] Linking CXX executable pdfunite.exe
../libpoppler-79.a(JPEG2000Stream.cc.obj): In function `ZN9JPXStream5closeEv':
D:/Git/poppler/poppler/JPEG2000Stream.cc:98: undefined reference to `_imp__opj_image_destroy@4'
D:/Git/poppler/poppler/JPEG2000Stream.cc:98: undefined reference to `_imp__opj_image_destroy@4'
```

�΍��҂ł��B

#### ${OpenJPEG_LIBRARIES} �ɂ��܂�

`utils/CMakeFiles/pdfunite.dir/build.make` ���m�F�����Ƃ���A
openjp2 �����_�C���N�g�� `/mingw32/lib/libopenjp2.dll.a` ���Q�Ƃ��Ă��܂����B

�����ŁA

```txt
if (OpenJPEG_FOUND)
  set(poppler_SRCS ${poppler_SRCS}
    poppler/JPEG2000Stream.cc
  )
  set(poppler_LIBS ${poppler_LIBS} openjp2)
```

�� ����� �� �������܂����B

```txt
if (OpenJPEG_FOUND)
  set(poppler_SRCS ${poppler_SRCS}
    poppler/JPEG2000Stream.cc
  )
  set(poppler_LIBS ${poppler_LIBS} ${OpenJPEG_LIBRARIES})
```

#### section .text �ɂ��܂���

�C���|�[�g���C�u�������r����̂� `objdump -p` �ł͔��ʂł��܂���ł������c

`nm` �Ŕ�r�����Ƃ���A�V���{���^�C�v�̑���ɋC���t���܂����B

`__imp__opj_decode@12` �̃V���{���^�C�v�� `T` text section �ƂȂ��Ă��܂� ��

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

�� `section .text` ��錾���܂���B `__imp__opj_decode@12` �͖��炩�� `U` undefined �ł��B

�� `section .text` ��錾���܂��B `__imp__opj_decode@12` �� `T` text section �ɕω����܂����B

```txt
$ nm /mingw32/lib/libopenjp2.notdll.a  | grep "opj_decode"
000000b4 T __imp__opj_decode@12
000000b0 T __imp__opj_decode_tile_data@20
         U _opj_decode
         U _opj_decode_tile_data
```
