#! /bin/bash

SOURCE_DIR="$PWD"
BUILD_DIR="$PWD/build"
ARTIFACTS_DIR="$PWD/artifacts"

windows=false
configure=true
build=true
install=false
pack=false
clean=false
version=""

while [[ $# > 0 ]]; do
    case "$1" in
    --windows)
        windows=true
        ;;

    --no-configure)
        configure=false
        ;;

    --no-build)
        build=false
        ;;

    --install)
        install=true
        ;;

    --pack)
        pack=true
        ;;

    --clean)
        clean=true
        ;;

    --tag-version)
        version=$(git describe --tags --abbrev=0)
        shift
        ;;
  esac
  shift
done

if $clean; then
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
if $windows; then
    CONFIGURE_ARGS+=" -A x64"
fi
if $configure; then
    echo "Configuring vku"
    cmake -S "$SOURCE_DIR" -B "$BUILD_DIR" $CONFIGURE_ARGS || exit 1
fi

BUILD_ARGS="--config Release"
if $build; then
    echo "Building vku"
    cmake --build "$BUILD_DIR" $BUILD_ARGS || exit 1
fi

INSTALL_ARGS="--config Release"
if $install; then
    echo "Installing vku"
    cmake --build "$BUILD_DIR" --target install $INSTALL_ARGS || exit 1
fi

if $pack; then
    PACK_ARGS="-property:PackageOutputPath=\"$ARTIFACTS_DIR\""
    PACK_ARGS+=" -property:Configuration=Release"
    if [ -n "$version" ]; then
        PACK_ARGS+=" -property:Version=\"${version#"v"}\""
    fi
    echo "Packing helveg"
    dotnet msbuild "$SOURCE_DIR" \
        -target:Restore,Build,Pack \
        $PACK_ARGS
fi
