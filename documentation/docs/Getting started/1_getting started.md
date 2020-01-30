---
title: Installation
---
# Installation

## Install the Hass.io add-on

1. Add the `https://github.com/helto4real/hassio-add-ons` in `Add new repository URL` to the add-on store.

    ![](img/newrepo.png)

2. Add the NetDaemon add-on.

    ![](img/daemon.png)

3. After you install it, do not start it just yet. We need to configure some stuff manually (will be improved as we come closer to release)

## Add default content to your configuration folder root

Add the folder netdaemon to the config folder from the repo:

[https://github.com/helto4real/hassio-add-ons/tree/master/netdaemon](https://github.com/helto4real/hassio-add-ons/tree/master/netdaemon)

The folder should contain following files and folder:

![](img/netdaemonfolder.png)

## Open the content in vscode

1. If you have not installed .NET Core 3.1 SDK on your PC do that now! [Link to download here](https://dotnet.microsoft.com/download/dotnet-core/3.1)

2. Now when the root config has the `netdaemon` folder you can now open it with vscode. This work from the shared config drive if you use SMB share. Important that you open the folder where the cs.proj file is in the root.
3. Open the vscode terminal and run `dotnet restore`, this needs to be done to get intellisense to work properly. Sometimes you need to restart vscode once for it to work.
4. Hack away!

## Check out the examples in docs

Todo: make link to examples here

## Start the add-on

Now you can start the plugin, check the logs for any errors.

> **IMPORTANT - YOU NEED TO RESTART THE ADD-ON EVERYTIME YOU MAKE CHANGES TO A FILE. THIS WILL CHANGE IN FUTURE RELEASES!**
