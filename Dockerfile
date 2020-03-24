# Build the NetDaemon with build container
FROM mcr.microsoft.com/dotnet/core/sdk:3.1.200-alpine as build

COPY . ./temp/

RUN \
    mkdir -p /data \
    \
    && dotnet \
    publish \
    ./temp/src/Service/Service.csproj \
    -c Release \
    -o ./temp/dist \
    \
    && mv ./temp/dist /app \
    && rm -R ./temp

# Build the target container
FROM ludeeus/container:dotnet-base
ARG version=dev

COPY --from=build /app /app

ENV \
    HASS_HOST=localhost \
    HASS_PORT=8123 \
    HASS_TOKEN=NOT_SET \
    HASS_DAEMONAPPFOLDER=/data \
    NETDAEMON_VERSION=$version

ENTRYPOINT ["dotnet", "/app/Service.dll"]