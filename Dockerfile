# Pre-build .NET NetDaemon core project
FROM mcr.microsoft.com/dotnet/sdk:7.0.101-bullseye-slim-amd64 as netbuilder
ARG TARGETPLATFORM
ARG BUILDPLATFORM

RUN echo "I am running on ${BUILDPLATFORM}" 
RUN echo "building for ${TARGETPLATFORM}" 

RUN export TARGETPLATFORM="${TARGETPLATFORM}"

# Copy the source to docker container
COPY ./src /usr/src
RUN dotnet publish /usr/src/Host/NetDaemon.Host.Default/NetDaemon.Host.Default.csproj -o "/daemon"

# Final stage, create the runtime container
FROM ghcr.io/net-daemon/netdaemon_base7

# # Install S6 and the Admin site
# COPY ./Docker/rootfs/etc/services.d/NetDaemonAdmin /etc/services.d/NetDaemonAdmin
COPY ./Docker/rootfs/etc/services.d/NetDaemonApp /etc/services.d/NetDaemonApp

# COPY admin
# COPY --from=builder /admin /admin
COPY --from=netbuilder /daemon /daemon

# This is always set to data as default
ENV NetDaemon__ApplicationConfigurationFolder=/data
