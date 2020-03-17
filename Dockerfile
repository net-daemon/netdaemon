ARG RUNTIME_IMAGE_TAG=3.1

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env
WORKDIR /sources

# Copy solution, restore and build
COPY . ./
RUN dotnet publish src/Service/Service.csproj -c Release -o dist

# Build runtime image
FROM mcr.microsoft.com/dotnet/core/runtime:$RUNTIME_IMAGE_TAG
WORKDIR /app
COPY --from=build-env /sources/dist .

ENV HASS_HOST localhost
ENV HASS_PORT 8123
ENV HASS_TOKEN NOT_SET
ENV HASS_DAEMONAPPFOLDER /data

RUN mkdir ${HASS_DAEMONAPPFOLDER}

ENTRYPOINT ["dotnet", "Service.dll"]
