# Pre-build .NET NetDaemon core project
FROM mcr.microsoft.com/dotnet/sdk:8.0-bookworm-slim-amd64 as netbuilder
#FROM mcr.microsoft.com/dotnet/sdk:8.0 as netbuilder
ARG TARGETPLATFORM
ARG BUILDPLATFORM

RUN echo "I am running on ${BUILDPLATFORM}"
RUN echo "building for ${TARGETPLATFORM}"
RUN export TARGETPLATFORM="${TARGETPLATFORM}"

# Copy the source to docker container
COPY . /usr

RUN dotnet publish /usr/src/Host/NetDaemon.Host.Default/NetDaemon.Host.Default.csproj -o "/daemon"

# Final stage, create the runtime container
FROM ghcr.io/net-daemon/netdaemon_base:8

# # Install S6 and the Admin site
# COPY ./Docker/rootfs/etc/services.d/NetDaemonAdmin /etc/services.d/NetDaemonAdmin
COPY --chmod=755 ./Docker/rootfs/etc/services.d/netdaemon /etc/s6-overlay/s6-rc.d/netdaemon
COPY --chmod=755 ./Docker/rootfs/etc/s6-overlay/s6-rc.d/usr/netdaemon etc/s6-overlay/s6-rc.d/user/contents.d/netdaemon
# COPY admin
# COPY --from=builder /admin /admin
COPY --from=netbuilder /daemon /daemon

# This is always set to data as default
ENV NetDaemon__ApplicationConfigurationFolder=/data
