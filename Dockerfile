FROM mcr.microsoft.com/dotnet/core/sdk:3.1 as build

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


FROM ludeeus/container:dotnet-base
COPY --from=build /app /app

ENV \
    HASS_HOST=localhost \
    HASS_PORT=8123 \
    HASS_TOKEN=NOT_SET

ENTRYPOINT ["dotnet", "/app/Service.dll"]