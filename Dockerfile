# Build the NetDaemon with build container
FROM ludeeus/container:dotnet-base-s6

# Copy the source to docker container
COPY ./src /usr/src

COPY Docker/rootfs/etc /etc

# Set default values of NetDaemon env
ENV \
    DOTNET_NOLOGO=1\
    DOTNET_CLI_TELEMETRY_OPTOUT=1 \
    HASS_RUN_PROJECT_FOLDER=/usr/src/Service \
    HASS_HOST=localhost \
    HASS_PORT=8123 \
    HASS_TOKEN=NOT_SET \
    HASS_DAEMONAPPFOLDER=/data


ENTRYPOINT ["/init"]
