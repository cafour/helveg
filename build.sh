#! /bin/bash

SOURCE_DIR="$PWD"
BUILD_DIR="$PWD/build"
ARTIFACTS_DIR="$PWD/artifacts"

windows=0
pack=0
clean=0

while [[ $# > 0 ]]; do
    case "$1" in
    --windows)
        windows=1
        shift 1
        ;;

    --pack)
        pack=1
        shift 1
        ;;

    --clean)
        clean=1
        shift 1
        ;;
  esac
done

if [ $clean -eq 1 ]; then
    echo "Cleaning build directory"
    rm -rf "$BUILD_DIR" 2>/dev/null
    echo "Cleaning artifacts directory"
    rm -rf "$ARTIFACTS_DIR" 2>/dev/null
fi

if [ ! -d "$BUILD_DIR" ]; then
    mkdir "$BUILD_DIR"
fi

if [ ! -d "$ARTIFACTS_DIR" ]; then
    mkdir "$ARTIFACTS_DIR"
fi

CONFIGURE_ARGS="-DCMAKE_INSTALL_PREFIX='$ARTIFACTS_DIR'"
echo "Configuring vku"
cmake -S "$SOURCE_DIR" -B "$BUILD_DIR" $CONFIGURE_ARGS

BUILD_ARGS="--config Release"
if [ $windows -eq 1 ]; then
    BUILD_ARGS+=" -A x64"
fi
echo "Building vku"
cmake --build "$BUILD_DIR" $BUILD_ARGS


if [ $pack -eq 1 ]; then
    INSTALL_ARGS="--config Release"

    echo "Installing vku"
    cmake --install "$BUILD_DIR" $INSTALL_ARGS

    echo "Packing helveg"
    dotnet pack -c Release "$SOURCE_DIR" /p:PackageOutputPath="$ARTIFACTS_DIR"
fi
