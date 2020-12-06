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
    && rm -fr /tmp/* /var/{cache,log}/* \
    && rm -R /admin/node_modules

# Build the NetDaemon with build container
FROM ludeeus/container:dotnet5-base-s6

# Copy the source to docker container
COPY ./src /usr/src

# COPY Docker/rootfs/etc /etc
COPY ./Docker/rootfs/etc /etc

# COPY admin
COPY --from=builder /admin /admin

# Install S6 and the Admin site
RUN apt update && apt install -y \
    nodejs \
    yarn \
    make

# Set default values of NetDaemon env
ENV \
    DOTNET_NOLOGO=true \
    DOTNET_CLI_TELEMETRY_OPTOUT=true \
    NETDAEMON__PROJECTFOLDER=/usr/src/Service \
    HOMEASSISTANT__HOST=localhost \
    HOMEASSISTANT__PORT=8123 \
    HOMEASSISTANT__TOKEN=NOT_SET \
    HASSCLIENT_MSGLOGLEVEL=Default \
    NETDAEMON__SOURCEFOLDER=/data \
    NETDAEMON__ADMIN=true \
    ASPNETCORE_URLS=http://+:5000


ENTRYPOINT ["/init"] 