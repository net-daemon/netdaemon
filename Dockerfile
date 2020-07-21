# Build the NetDaemon with build container
#mcr.microsoft.com/dotnet/core/sdk:3.1.200
#ludeeus/container:dotnet-base
FROM mcr.microsoft.com/dotnet/core/sdk:3.1.302

# Copy the source to docker container
COPY ./src /usr/src

# COPY Docker/rootfs/etc /etc
COPY ./Docker/rootfs/etc /etc

# Install S6 and the Admin site
RUN \
    wget -q -O /s6 \
        https://raw.githubusercontent.com/ludeeus/container/master/rootfs/s6/install \
    && bash /s6 \
    \
    && curl -sS https://dl.yarnpkg.com/debian/pubkey.gpg | apt-key add - \
    && echo "deb http://dl.yarnpkg.com/debian/ stable main" | tee /etc/apt/sources.list.d/yarn.list \
    \
    && apt update && apt install -y \
        nodejs \
        yarn \
        make \
    \
    && git clone https://github.com/net-daemon/admin.git /admin \
    && cd /admin \
    && make deploy

# Set default values of NetDaemon env
ENV \
    DOTNET_NOLOGO=true \
    DOTNET_CLI_TELEMETRY_OPTOUT=true \
    NETDAEMON__PROJECTFOLDER=/usr/src/Service \
    HOMEASSISTANT__HOST=localhost \
    HOMEASSISTANT__PORT=8123 \
    HOMEASSISTANT__TOKEN=NOT_SET \
    HASSCLIENT_MSGLOGLEVEL=Default \
    NETDAEMON__SOURCEFOLDER=/data


ENTRYPOINT ["/init"]
