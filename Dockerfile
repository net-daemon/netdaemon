FROM alpine:3.11

ARG NETVERSION="3.1.200"

ENV \
    HASS_HOST=localhost \
    HASS_PORT=8123 \
    HASS_TOKEN=NOT_SET \
    HASS_DAEMONAPPFOLDER=/data


COPY . ./temp/

RUN \
    apk add --no-cache \
        bash \
        ca-certificates \
        krb5-libs \
        libgcc \
        libintl \
        icu \
        libssl1.1 \
        libstdc++ \
        zlib \
    \
    && echo $(uname -m) \
    && wget -q -nv -O /tmp/dotnet-install.sh https://dot.net/v1/dotnet-install.sh \
    \
    && sed -i 's|echo "linux-musl"|echo "linux"|' /tmp/dotnet-install.sh \
    \
    && bash /tmp/dotnet-install.sh --version ${NETVERSION} --install-dir "/root/.dotnet" \
    \
    && rm /tmp/dotnet-install.sh \
    \
    && ln -s /root/.dotnet/dotnet /bin/dotnet \
    \
    && dotnet help \
    \
    && rm -fr \
        /tmp/* \
        /var/{cache,log}/* \
        /var/lib/apt/lists/* \
    \
    && mkdir -p ${HASS_DAEMONAPPFOLDER} \
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