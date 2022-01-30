# Build the NetDaemon Admin with build container
FROM ghcr.io/ludeeus/alpine/node:stable as builder

RUN \
    apk add --no-cache --virtual .build-deps \
        make \
    \
    && git clone https://github.com/net-daemon/admin.git /admin \
    && cd /admin \
    && git checkout tags/1.3.5 \
    && make deploy \
    \
    && apk del --no-cache .build-deps \
    && rm -rf /var/cache/apk/* 

# Pre-build .NET NetDaemon core project
FROM mcr.microsoft.com/dotnet/sdk:6.0.100-bullseye-slim-amd64 as netbuilder
ARG TARGETPLATFORM
ARG BUILDPLATFORM

RUN echo "I am running on ${BUILDPLATFORM}" 
RUN echo "building for ${TARGETPLATFORM}" 

RUN export TARGETPLATFORM="${TARGETPLATFORM}"

# Copy the source to docker container
COPY ./src /usr/src
RUN dotnet publish /usr/src/Host/NetDaemon.Host.Default/NetDaemon.Host.Default.csproj -o "/daemon"

# Final stage, create the runtime container
FROM ghcr.io/net-daemon/netdaemon_base

# # Install S6 and the Admin site
# COPY ./Docker/rootfs/etc/services.d/NetDaemonAdmin /etc/services.d/NetDaemonAdmin
COPY ./Docker/rootfs/etc/services.d/NetDaemonApp /etc/services.d/NetDaemonApp

# COPY admin
# COPY --from=builder /admin /admin
COPY --from=netbuilder /daemon /daemon

# This is always set to data as default
ENV NetDaemon__ApplicationConfigurationFolder=/data
