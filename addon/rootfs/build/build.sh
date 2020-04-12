#!/bin/bash

cd /tmp

ARC=$(uname -m)

echo "Building NetDaemon for platform $ARC"

if [ "$ARC" == "armhf" ]; then
    export RID="linux-arm"
elif [ "$ARC" == "aarch64" ]; then
    export RID="linux-arm"
elif [ "$ARC" == "x86_64" ]; then
    export RID="linux-x64"
else
    echo 'NOT VALID ARCHITECTURE' && exit 1

fi

cd src/Service/
dotnet add package Microsoft.Packaging.Tools.Trimming --version 1.1.0-preview1-26619-01
dotnet publish -c Release -o /netdaemon/bin/publish -r $RID /p:PublishSingleFile=true /p:PublishTrimmed=true /p:TrimUnusedDependencies=true
