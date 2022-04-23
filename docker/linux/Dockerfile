FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine

ENV LANG=en_US.UTF-8 \
    LANGUAGE=en_US:UTF-8

RUN apk add \
    autoconf \
    automake \
    bash \
    binutils \
    bison \
    bzip2 \
    flex \
    g++ \
    gdk-pixbuf \
    gettext \
    git \
    gperf \
    intltool \
    libtool \
    linux-headers \
    lzip \
    make \
    openssl \
    openssl-dev \
    p7zip \
    patch \
    perl \
    python3 \
    ruby \
    unzip \
    wget \
    xz \
    zlib \
    cmake \
    clang \
    xorg-server-dev \
    libxrandr-dev \
    libxinerama-dev \
    libxcursor-dev \
    libxi-dev

RUN cd /usr/bin && ln -s /usr/bin/python3 python
