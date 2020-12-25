# Developing NetDaemon 
These instructions are for developing NetDaemon. For apps please use [the docs](https://netdaemon.xyz).
For your convenience we provided with a docker setup with Home Assistant you can run on your development machine. 
`tests/Docker/HA` you will find the docker-compose file. Run it outside the devcontainer. Remarkts that you should use port 8124 connecting to this instance of Home Assistant.

## Use appsettings.Development.json
Copy the "_appsettings.Development.json under `src/Service`

## Setup the environment vars
Alternative to using appsettings for development is to use environment varables for your Home Assistant instance

| Environment variable | Description |
| ------ | ------ |
| HOMEASSISTANT__TOKEN   |  Token secret to access the HA instance  |
| HOMEASSISTANT__HOST | The ip or hostname of HA |
| HOMEASSISTANT__PORT | The port of home assistant (defaults to 8123 if not specified) |
| NETDAEMON__GENERATEENTITIES | Generate entities, recommed set false unless debugging |
| NETDAEMON__APPSOURCE | The folder/project/dll where it will find daemon. Set this to empty `""` to debug apps local. If needed to debug the dynamic source compilation, set to `/workspaces/netdaemon/Service/apps` |

Use `src/Development/apps` as starting point to debug your stuff! 

Good luck
