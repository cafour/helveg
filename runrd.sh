#!/bin/bash

LD_LIBRARY_PATH=~/dev/fi/helveg/build/vku LD_PRELOAD=~/dev/vk-loader/build/loader/libvulkan.so renderdoccmd capture -c rd -w -d . ./build/helveg/helveg world
