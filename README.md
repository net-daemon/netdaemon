# NetDaemon - Application daemon in .NET core for Home Assistant

[![Build Status](https://dev.azure.com/helto4real0721/NetDaemon/_apis/build/status/NetDaemon%20CI%20pipeline?branchName=master)](https://dev.azure.com/helto4real0721/NetDaemon/_build/latest?definitionId=3&branchName=master)

<a href="https://www.buymeacoffee.com/ij1qXRM6E" target="_blank"><img src="https://www.buymeacoffee.com/assets/img/custom_images/orange_img.png" alt="Buy Me A Coffee" style="height: auto !important;width: auto !important;" ></a>

Welcome to the NetDaemon project. This is the application daemon for Home Assistant for people that love to write code in the .NET ecosystem and want to do their automation for Home Assistant in .NET and C#.

Please see [https://netdaemon.xyz](https://netdaemon.xyz) for detailed instructions how to get started using NetDaemon.

> **The NetDaemon is currently in alpha release so expect things to change.**

## Issues

If you have issues or suggestions of improvements, please [add an issue](https://github.com/net-daemon/netdaemon/issues)

## Discuss the NetDaemon

Please [join the Discord server](https://discord.gg/K3xwfcX) to get support or if you want to contribute and help others.

## Docker Support
For those who have Homeassistant running on Docker, not supervised by HASS, the NetDaemon can be shipped as a container and ran in parallel.

To build the image simply execute the build command in the root of the solution

```
docker build . --tag netdaemon:latest
```

After the image has been built, you can deploy the container using the following command. It is advised to map the `/data` folder of the container to a local folder as the daemon's apps are linked to this folder.

The following environment variables are available to identify your Home Assistant instance
* `HASS_HOST`, defaults to *localhost*
* `HASS_PORT`, default to *8123*
* `HASS_TOKEN` needs to be set to a valid access token.

Example giving: 
```
docker run -v -e HASS_HOST=192.168.1.1 -e HASS_TOKEN=ey123... ~/netdaemon_config:/data --name netdaemon netdaemon
```

## Example apps

Please check out the apps being developed for netdaemon. Since documentation is still lacking behind it will be best looking at real code 😊

| User                                                                                                    | Description                                           |
| ------------------------------------------------------------------------------------------------------- | ----------------------------------------------------- |
| [@helto4real](https://github.com/helto4real/hassio/tree/master/netdaemon/apps)                          | My own netdaemon apps running in production           |
| [@isabellaalstrom](https://github.com/isabellaalstrom/home-assistant-config/tree/master/netdaemon/apps) | Isabella's netdaemon apps, check them out, nice stuff |

## VSCode customization

Please advice that some customizations to VSCode has been made through settings. Check out the settings.json in the .vscode folder.

## Attribution

ICON: Attribution: [chris](https://commons.wikimedia.org/wiki/User:Chrkl) 論
