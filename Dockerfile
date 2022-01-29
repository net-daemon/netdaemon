# Build the NetDaemon Admin with build container
FROM ludeeus/container:frontend as builder

RUN \
    apk add make \
    \
    && git clone https://github.com/net-daemon/admin.git /admin \
    && cd /admin \
    && git checkout tags/1.3.5 \
    && make deploy \
    \
    && rm -fr /var/lib/apt/lists/* \
    && rm -fr /tmp/* /var/{cache,log}/*  

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

ENV \    
    HomeAssistant__Host=localhost \
    HomeAssistant__Port=8123 \
    HomeAssistant__Token=NOT_SET \
    NetDaemon__ApplicationConfigurationFolder=/data 

