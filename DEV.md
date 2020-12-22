# Developing NetDaemon 
These instructions are for developing NetDaemon. For apps please use [the docs](https://netdaemon.xyz).

## Setup the environment vars
Easiest is to setup environment varables for your Home Assistant instance

| Environment variable | Description |
| ------ | ------ |
| HOMEASSISTANT__TOKEN   |  Token secret to access the HA instance  |
| HOMEASSISTANT__HOST | The ip or hostname of HA |
| HOMEASSISTANT__PORT | The port of home assistant (defaults to 8123 if not specified) |
| NETDAEMON__GENERATEENTITIES | Generate entities, recommed set false unless debugging |
| NETDAEMON__APPSOURCE | The folder/project/dll where it will find daemon. Set this to empty `""` to debug apps local. If needed to debug the dynamic source compilation, set to `/workspaces/netdaemon/Service/apps` |

Use `src/Service/apps` as starting point to debug your stuff! 

Good luck
