# Build the NetDaemon with build container
FROM mcr.microsoft.com/dotnet/core/sdk:3.1.302

# Copy the source to docker container
COPY ./src /usr/src

# COPY Docker/rootfs/etc /etc
COPY ./Docker/rootfs/etc /etc

# Install S6 and the Admin site
RUN wget -qO /s6 \
    https://raw.githubusercontent.com/ludeeus/container/master/rootfs/s6/install \
    && bash /s6 \
    \
    && wget -qO - https://dl.yarnpkg.com/debian/pubkey.gpg | apt-key add - \
    && echo "deb https://dl.yarnpkg.com/debian/ stable main" | tee /etc/apt/sources.list.d/yarn.list \
    \
    && apt update && apt install -y \
    nodejs \
    yarn \
    make \
    \
    && git clone https://github.com/net-daemon/admin.git /admin \
    && cd /admin \
    && git checkout tags/1.3.3 \
    && make deploy \
    \
    && rm -fr /var/lib/apt/lists/* \
    && rm -fr /tmp/* /var/{cache,log}/*

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
