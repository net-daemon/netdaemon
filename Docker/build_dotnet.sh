#!/bin/bash
ARCH=$(uname -m)

if [ $ARCH == "armv7l" ]; then
    dotnet publish /usr/src/Service/Service.csproj -v q -c Release -r "linux-arm" -o "/daemon"
elif [ $ARCH == "aarch64" ]; then
    dotnet publish /usr/src/Service/Service.csproj -v q -c Release -r "linux-arm" -o "/daemon"
elif [ $ARCH == "x86_64" ]; then
    dotnet publish /usr/src/Service/Service.csproj -v q -c Release -r "linux-x64" -o "/daemon"
else
    echo 'NOT VALID BUILD'; exit 1; 
fi
