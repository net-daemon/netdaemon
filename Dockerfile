FROM ludeeus/container:dotnet-base

ENV \
    HASS_HOST=localhost \
    HASS_PORT=8123 \
    HASS_TOKEN=NOT_SET \
    HASS_DAEMONAPPFOLDER=/data


COPY . ./temp/

RUN mkdir -p ${HASS_DAEMONAPPFOLDER} \
    \
    && dotnet \
        publish \
        ./temp/src/Service/Service.csproj \
        -c Release \
        -o ./temp/dist \
    \
    && mv ./temp/dist /app \
    && rm -R ./temp

ENTRYPOINT ["dotnet", "/app/Service.dll"]