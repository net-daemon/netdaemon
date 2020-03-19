
# Build amd64 image
docker build . --tag horizon0156/netdaemon:amd64-latest

# Build arm32 image
docker build . --build-arg RUNTIME_IMAGE_TAG=3.1-buster-slim-arm32v7 --tag horizon0156/netdaemon:arm32-latest

# Push images
docker push horizon0156/netdaemon:arm32-latest 
docker push horizon0156/netdaemon:amd64-latest

# Create multi-arch manifest
docker manifest create \
    horizon0156/netdaemon:latest \
    horizon0156/netdaemon:amd64-latest \
    horizon0156/netdaemon:arm32-latest
docker manifest push --purge horizon0156/netdaemon:latest