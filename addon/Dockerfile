
ARG BUILD_FROM=hassioaddons/base-amd64:5.0.3
#ARG BUILD_FROM=alpine

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine as build

# Avoid warnings by switching to noninteractive
#ENV DEBIAN_FRONTEND=noninteractive

# Build arguments
ARG BUILD_ARCH
# Add-on version
ENV ADDON_VERSION 0.0.36

# Configure apt and install packages
RUN \
    apk add --no-cache git \
    && git clone https://github.com/helto4real/netdaemon.git \
    && cd netdaemon \
    && git checkout dev

# Build a self contained executable and trim the executable
RUN \
    if [ "${BUILD_ARCH}" = "armhf" ]; then \
    export RID="linux-arm"; \
    elif [ "${BUILD_ARCH}" = "armv7" ]; then \
    export RID="linux-arm"; \
    elif [ "${BUILD_ARCH}" = "aarch64" ]; then \
    export RID="linux-arm"; \
    elif [ "${BUILD_ARCH}" = "amd64" ]; then \
    export RID="linux-musl-x64"; \
    else \
    echo 'NOT VALID BUILD'; exit 1; \
    fi \
    && \
    cd netdaemon/src/Service/ \
    && dotnet add package Microsoft.Packaging.Tools.Trimming --version 1.1.0-preview1-26619-01 \
    && dotnet publish -c Release -o /netdaemon/bin/publish -r $RID /p:TrimUnusedDependencies=true

# STAGE 2: Include binary in target add-on container
FROM ${BUILD_FROM} AS runtime

# Build arguments
ARG BUILD_ARCH

# Copy binary and the config from build container
COPY --from=build /netdaemon/bin/publish /daemon

COPY install_dotnet_dep.sh /install_dotnet_dep.sh

# Install .net core runtime deps
RUN chmod +x /install_dotnet_dep.sh && /install_dotnet_dep.sh

# Configure web servers to bind to port 80 when present
ENV ASPNETCORE_URLS=http://+:80 \
    # Enable detection of running in a container
    DOTNET_RUNNING_IN_CONTAINER=true \
    # Set the invariant mode since icu_libs isn't included (see https://github.com/dotnet/announcements/issues/20)
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=true

ENV HASS_DAEMONAPPFOLDER /config/netdaemon

CMD [ "/daemon/Service" ]

#
# LABEL target docker image
#
ARG BUILD_DATE
ARG BUILD_REF
ARG BUILD_VERSION

# Labels
LABEL \
    io.hass.name="Netdaemon" \
    io.hass.description="An application daemon for c# and .NET core 3.1" \
    io.hass.arch="${BUILD_ARCH}" \
    io.hass.type="addon" \
    io.hass.version=${BUILD_VERSION} \
    maintainer="Tomas Hellström <tomas.hellstrom@yahoo.se>" \
    org.label-schema.description="Netdaemon, a community add-on written in .NET core 3.1" \
    org.label-schema.build-date=${BUILD_DATE} \
    org.label-schema.name="Netdaemon" \
    org.label-schema.schema-version="1.0" \
    org.label-schema.url="https://addons.community" \
    org.label-schema.usage="https://helto4real.github.io/netdaemon/" \
    org.label-schema.vcs-ref=${BUILD_REF} \
    org.label-schema.vcs-url="https://helto4real.github.io/netdaemon/" \
    org.label-schema.vendor="Community Hass.io Addons"