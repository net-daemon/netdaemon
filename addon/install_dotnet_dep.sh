# Install .net core runtime deps
if [ "$BUILD_ARCH" == 'amd64' ]; then
    apk update &&
        apk add --no-cache \
            ca-certificates \
            krb5-libs \
            libgcc \
            libintl \
            libssl1.1 \
            libstdc++ \
            wget \
            zlib
fi

# arm 32/64 buster slim
if [ "$BUILD_ARCH" == 'armhf' ] || [ "$BUILD_ARCH" == 'armv7' ] || [ "$BUILD_ARCH" == 'aarch64' ]; then
    apt-get update &&
        apt-get install -y --no-install-recommends \
            ca-certificates libc6 \
            libgcc1 \
            libgssapi-krb5-2 \
            libicu63 \
            libssl1.1 \
            libstdc++6 \
            wget \
            zlib1g && rm -rf /var/lib/apt/lists/*
fi
