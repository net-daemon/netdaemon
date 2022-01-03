#-------------------------------------------------------------------------------------------------------------
# Copyright (c) Microsoft Corporation. All rights reserved.
# Licensed under the MIT License. See https://go.microsoft.com/fwlink/?linkid=2090316 for license information.
#-------------------------------------------------------------------------------------------------------------

FROM mcr.microsoft.com/dotnet/sdk:6.0-focal

RUN apt-get update && apt-get install -y ssh
# This Dockerfile adds a non-root 'vscode' user with sudo access. However, for Linux,
# this user's GID/UID must match your local user UID/GID to avoid permission issues
# with bind mounts. Update USER_UID / USER_GID if yours is not 1000. See
# https://aka.ms/vscode-remote/containers/non-root-user for details.
ARG USERNAME=vscode
ARG USER_UID=1000
ARG USER_GID=$USER_UID

# [Optional] Version of Node.js to install.
ARG INSTALL_NODE="true"
ARG NODE_MAJOR_VERSION="10"

# [Optional] Install the Azure CLI
ARG INSTALL_AZURE_CLI="false"

# Avoid warnings by switching to noninteractive
ENV DEBIAN_FRONTEND=noninteractive
ENV TZ=Europe/Stockholm

# Switch back to dialog for any ad-hoc use of apt-get
ENV DEBIAN_FRONTEND=noninteractive
ENV TEST=AK

# dotnet specific
ENV ASPNETCORE_ENVIRONMENT="Development"
ENV DOTNET_ENVIRONMENT="Development"
