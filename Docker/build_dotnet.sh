#!/bin/bash
ARCH=$(uname -m)
echo "TARGET PLATFORM: $TARGETPLATFORM"
if [ $TARGETPLATFORM == "linux/arm/v7" ]; then
    dotnet publish /usr/src/Service/Service.csproj -v q -c Release -r "linux-arm" -o "/daemon"
elif [ $TARGETPLATFORM == "linux/arm64" ]; then
    dotnet publish /usr/src/Service/Service.csproj -v q -c Release -r "linux-arm64" -o "/daemon"
elif [ $TARGETPLATFORM == "linux/amd64" ]; then
    dotnet publish /usr/src/Service/Service.csproj -v q -c Release -r "linux-x64" -o "/daemon"
else
    echo 'NOT VALID BUILD'; exit 1; 
fi
