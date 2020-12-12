# Build the NetDaemon Admin with build container
FROM ludeeus/container:frontend as builder

RUN \
    apk add make \
    \
    && git clone https://github.com/net-daemon/admin.git /admin \
    && cd /admin \
    && git checkout tags/1.3.4 \
    && make deploy \
    \
    && rm -fr /var/lib/apt/lists/* \
    && rm -fr /tmp/* /var/{cache,log}/*  

# Pre-build .NET NetDaemon core project
FROM mcr.microsoft.com/dotnet/sdk:5.0.100 as netbuilder

# Copy the source to docker container
COPY ./src /usr/src
RUN dotnet publish /usr/src/Service/Service.csproj -v q -c Release -o "/daemon"

# Final stage, create the runtime container
FROM mcr.microsoft.com/dotnet/sdk:5.0.100
COPY ./Docker/rootfs/etc /etc
COPY ./Docker/s6.sh /s6.sh

RUN chmod 700 /s6.sh
RUN /s6.sh

# COPY admin
COPY --from=builder /admin /admin
COPY --from=netbuilder /daemon /daemon
# Install S6 and the Admin site
RUN apt update && apt install -y \
    nodejs \
    yarn \
    make

# Set default values of NetDaemon env
ENV \
    DOTNET_NOLOGO=true \
    DOTNET_CLI_TELEMETRY_OPTOUT=true \
    HOMEASSISTANT__HOST=localhost \
    HOMEASSISTANT__PORT=8123 \
    HOMEASSISTANT__TOKEN=NOT_SET \
    HASSCLIENT_MSGLOGLEVEL=Default \
    NETDAEMON__SOURCEFOLDER=/data \
    NETDAEMON__ADMIN=true \
    ASPNETCORE_URLS=http://+:5000 \
    HASS_DISABLE_LOCAL_ASM=true

ENTRYPOINT ["/init"] 
