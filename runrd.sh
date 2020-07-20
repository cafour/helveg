#!/bin/bash
# ./build.sh --install
# dotnet build helveg
NAME="rd-$(date +"%H-%M")"
renderdoccmd capture -c $NAME -w -d . dotnet run -p helveg -- "$@"
