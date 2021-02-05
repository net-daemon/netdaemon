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
FROM mcr.microsoft.com/dotnet/sdk:5.0.102-1-buster-slim-amd64 as netbuilder

ARG TARGETPLATFORM
ARG BUILDPLATFORM

RUN echo "I am running on $BUILDPLATFORM" 
RUN echo "building for $TARGETPLATFORM" 

RUN export TARGETPLATFORM=$TARGETPLATFORM
# Copy the source to docker container
COPY ./src /usr/src
RUN dotnet publish /usr/src/Service/Service.csproj -o "/daemon"

# Final stage, create the runtime container
FROM mcr.microsoft.com/dotnet/sdk:5.0

# Install S6 and the Admin site
RUN apt update && apt install -y \
    nodejs \
    yarn \
    jq \
    make

COPY ./Docker/rootfs/etc /etc
COPY ./Docker/s6.sh /s6.sh

RUN chmod 700 /s6.sh
RUN /s6.sh

# COPY admin
COPY --from=builder /admin /admin
COPY --from=netbuilder /daemon /daemon

#   NETDAEMON__WARN_IF_CUSTOM_APP_SOURCE=

# Set default values of NetDaemon env
ENV \
    S6_KEEP_ENV=1 \
    DOTNET_NOLOGO=true \
    DOTNET_CLI_TELEMETRY_OPTOUT=true \
    HASSCLIENT_MSGLOGLEVEL=Default \
    HOMEASSISTANT__HOST=localhost \
    HOMEASSISTANT__PORT=8123 \
    HOMEASSISTANT__TOKEN=NOT_SET \
    NETDAEMON__APPSOURCE=/data \
    NETDAEMON__ADMIN=true \
    ASPNETCORE_URLS=http://+:5000 

ENTRYPOINT ["/init"] 
