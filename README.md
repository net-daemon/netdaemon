# NetDaemon - Write your automations in C# for Home Assistant

![CI build](https://github.com/net-daemon/netdaemon/workflows/CI%20build/badge.svg?branch=main) [![Coverage Status](https://coveralls.io/repos/github/net-daemon/netdaemon/badge.svg?branch=dev)](https://coveralls.io/github/net-daemon/netdaemon?branch=dev) [![Sonar Quality Gate](https://img.shields.io/sonar/quality_gate/net-daemon_netdaemon?server=https%3A%2F%2Fsonarcloud.io)](https://sonarcloud.io/summary/overall?id=net-daemon_netdaemon)

Welcome to the NetDaemon project. This is the application daemon that allows you to write your home automations in C# for Home Assistant. This repo contains the third generation of NetDaemon, V3. It is currently in beta. If you need the V2 version, please see [NetDaemon V2 repo](https://github.com/net-daemon/netdaemon_v2)

Please see [https://netdaemon.xyz](https://netdaemon.xyz/docs/v3/started/get_started) for detailed instructions how to get started using NetDaemon.

> **The NetDaemon v3 is pretty stable but is still in development so expect things to change.**

## About V3
NetDaemon runtime version 3 is built from scratch. The reason is to make the product better for both users and contributors by redesigning it using C# modern design. 

## What has changed from V2?
### Breaking changes
- We now no longer support apps using the old base class `NetDaemonRxApp`. HassModel is the only supported model moving forward.
- We changed the way configuration works. Configuration from yaml is now injected using `IAppConfig<MyConfigClass<`. See the template [v3 branch](https://github.com/net-daemon/netdaemon-app-template/tree/v3) for examples how to use it.
- Some public interfaces like `INetDaemon`is no longer used. 
- Different namespaces are used, like `NetDaemon.AppModel`and `NetDaemon.Runtime`. Please see the template [v3 branch](https://github.com/net-daemon/netdaemon-app-template/tree/v3).
- We now use `input_booleans` instead of `switch`for handling app state. If you are not using the service callbacks you will now no longer have to use the integration to handle app states persistent.
- The text to speech queued feature is now an extension.
- Using a `.csproj`as target for the runtime is no longer supported. You need to deploy the compiled binaries and point to those.

### What did we improve
- Overall design is now easier for contributors to contribute to the product. This is very important for long-term lifetime of NetDaemon.
- Built to extend, the interfaces are minimal and default extensions are provided. 
- Docker images has been improved to use a base image. Better separation between different type hosts (docker, add-on) etc.

### Versioning
The V3 uses CalVer versioning and is always the current version. Looking for V2 versions and updates, please use version `22.1.x`.

## Issues

If you have issues or suggestions of improvements, please [add an issue](https://github.com/net-daemon/netdaemon/issues)

## Discuss the NetDaemon

Please [join the Discord server](https://discord.gg/K3xwfcX) to get support or if you want to contribute and help others.

## Install NetDaemon

https://netdaemon.xyz/docs/started/installation

## Example apps

Please check out the apps being developed for netdaemon3. Since documentation is still lacking behind it will be best looking at real code 😊. To check out the new HassModel examples, please checkout the descriptions what user has adopted it below.

| User                                                               | Description                                 |
| ------------------------------------------------------------------ | ------------------------------------------- |
| [@helto4real](https://github.com/helto4real/NetDaemon3Automations) | Tomas netdaemon3 apps running in production |

