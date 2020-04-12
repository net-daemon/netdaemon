# Build the NetDaemon with build container
FROM mcr.microsoft.com/dotnet/core/sdk:3.1.200-alpine as build

ARG TARGETPLATFORM
ARG BUILDPLATFORM


RUN apk add  \
    bash


# Copy the source to docker container
COPY ./src /tmp/src

# Copy the build script for minimal single binary build
COPY addon/rootfs/build/build.sh /tmp/build.sh
RUN chmod +x /tmp/build.sh

# Build the minimal single binary
RUN /bin/bash /tmp/build.sh

# Build the target container
FROM netdaemon/base

# Copy the built binaries and set execute permissions
COPY --from=build /netdaemon/bin/publish /daemon
RUN chmod +x /daemon/Service

# Copy the S6 service scripts
COPY addon/rootfs/etc /etc

# Set default values of NetDaemon env
ENV \
    HASS_HOST=localhost \
    HASS_PORT=8123 \
    HASS_TOKEN=NOT_SET \
    HASS_DAEMONAPPFOLDER=/data

ENTRYPOINT ["/init"]