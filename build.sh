#! /bin/bash

SOURCE_DIR="$PWD"
BUILD_DIR="$PWD/build"
ARTIFACTS_DIR="$PWD/artifacts"

windows=0
install=0
pack=0
clean=0
version=$(git describe --tags --abbrev=0)

while [[ $# > 0 ]]; do
    case "$1" in
    --windows)
        windows=1
        ;;

    --install)
        install=1
        ;;

    --pack)
        pack=1
        ;;

    --clean)
        clean=1
        ;;

    --version)
        version=$2
        shift
        ;;
  esac
  shift
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
if [ $windows -eq 1 ]; then
    CONFIGURE_ARGS+=" -A x64"
fi
echo "Configuring vku"
cmake -S "$SOURCE_DIR" -B "$BUILD_DIR" $CONFIGURE_ARGS

BUILD_ARGS="--config Release"
echo "Building vku"
cmake --build "$BUILD_DIR" $BUILD_ARGS

if [ $install -eq 1 ]; then
    INSTALL_ARGS="--config Release"

    echo "Installing vku"
    cmake --install "$BUILD_DIR" $INSTALL_ARGS
fi

if [ $pack -eq 1 ]; then
    echo "Packing helveg"
    dotnet msbuild "$SOURCE_DIR" \
        -target:Restore,Build,Pack \
        -property:PackageOutputPath="$ARTIFACTS_DIR" \
        -property:Configuration=Release \
        -property:Version="${version#"v"}"
fi
