#!/bin/sh

set -eux

export DEBIAN_FRONTEND=noninteractive
apt-get update

# common deps
apt-get install -y software-properties-common curl

# .NET 9
add-apt-repository ppa:dotnet/backports -y
apt-get update && apt-get install -y dotnet-sdk-9.0

# Node.JS and pnpm
curl -o- https://raw.githubusercontent.com/nvm-sh/nvm/v0.40.3/install.sh | bash
\. "$HOME/.nvm/nvm.sh"
nvm install 22
export NVM_DIR="$HOME/.nvm"
[ -s "$NVM_DIR/nvm.sh" ] && \. "$NVM_DIR/nvm.sh"
export COREPACK_ENABLE_DOWNLOAD_PROMPT=0
corepack enable pnpm
corepack prepare pnpm@latest --activate

# build the front-end
pnpm install
pnpm build

# build the tool package
dotnet build -c Release -p:AllowGitVersion=false Helveg.sln
dotnet pack --no-build -p:AllowGitVersion=false Helveg.sln -o artifacts -c Release
dotnet tool install -g --source ./artifacts --version 0.0.0-local helveg
export PATH="$PATH:/root/.dotnet/tools"

# run the tool to produce the basic sample
helveg --preset Dev --verbose -p TargetFramework=net9.0 --out-file index.html
