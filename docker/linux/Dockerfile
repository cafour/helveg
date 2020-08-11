FROM mcr.microsoft.com/dotnet/core/sdk:3.1

ARG DOCKER_LANG=en_US
ARG DOCKER_TIMEZONE=Europe/Prague

ENV LANG=$DOCKER_LANG.UTF-8 \
    LANGUAGE=$DOCKER_LANG:UTF-8

RUN apt-get update && DEBIAN_FRONTEND=noninteractive apt-get install -y \
    cmake \
    clang \
    xorg-dev
