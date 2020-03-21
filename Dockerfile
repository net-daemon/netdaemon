FROM ludeeus/devcontainer:dotnet

ENV \
    HASS_HOST=localhost \
    HASS_PORT=8123 \
    HASS_TOKEN=NOT_SET \
    HASS_DAEMONAPPFOLDER=/data


RUN mkdir -p ${HASS_DAEMONAPPFOLDER} ./temp
COPY . ./temp/

RUN \
    dotnet \
    publish \
    ./temp/src/Service/Service.csproj \
    -c Release \
    -o ./temp/dist \
    \
    && mv ./temp/dist /app \
    && rm -R ./temp

ENTRYPOINT ["dotnet", "/app/Service.dll"]