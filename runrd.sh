#!/bin/bash
./build.sh --install
dotnet build helveg
LD_PRELOAD=~/dev/vk-loader/build/loader/libvulkan.so renderdoccmd capture -c rd -w -d . dotnet run -p helveg -- "$@"
