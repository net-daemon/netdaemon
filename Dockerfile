ARG DOTNET_VERSION=3.1.200

FROM mcr.microsoft.com/dotnet/core/sdk:${DOTNET_VERSION} AS build-env
WORKDIR /sources

# Copy solution, restore and build
COPY . ./
RUN dotnet publish src/Service/Service.csproj -c Release -o dist

# Build runtime image
FROM mcr.microsoft.com/dotnet/core/runtime:${DOTNET_VERSION}
WORKDIR /app
COPY --from=build-env /sources/dist .

ENV \
    HASS_HOST=localhost \
    HASS_PORT=8123 \
    HASS_TOKEN=NOT_SET \
    HASS_DAEMONAPPFOLDER=/data

RUN mkdir -p ${HASS_DAEMONAPPFOLDER}

ENTRYPOINT ["dotnet", "help"]