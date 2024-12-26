# NetDaemon - Write your automations in C# for Home Assistant

![CI build](https://github.com/net-daemon/netdaemon/workflows/CI%20build/badge.svg?branch=main) [![Coverage Status](https://coveralls.io/repos/github/net-daemon/netdaemon/badge.svg?branch=dev)](https://coveralls.io/github/net-daemon/netdaemon?branch=dev) [![Sonar Quality Gate](https://img.shields.io/sonar/quality_gate/net-daemon_netdaemon?server=https%3A%2F%2Fsonarcloud.io)](https://sonarcloud.io/summary/overall?id=net-daemon_netdaemon)

Welcome to the NetDaemon project. This is the application daemon that allows you to write your home automations in C# for Home Assistant.
This repo contains the latest generation of NetDaemon, V5.

Please see [https://netdaemon.xyz](https://netdaemon.xyz/docs/v3/started/get_started) for detailed instructions how to get started using NetDaemon.

> **The NetDaemon v5 is pretty stable and we aim to have as little breaking changes as possible**

## About V5
NetDaemon runtime version 5 is built for .NET 9 and C# 13. Version 5 is from release 2024.xx.0 and forward.

### Versioning
The NetDaemon nuget packaged uses CalVer versioning system. The versioning is in the format `YYYY.WW.PATCH` where `YYYY.WW`
is the year and weeknumber (01-52). `PATCH` is the patch version of the release.

## Issues

If you have issues or suggestions of improvements, please [add an issue](https://github.com/net-daemon/netdaemon/issues)

## Discuss the NetDaemon

Please [join the Discord server](https://discord.gg/K3xwfcX) to get support or if you want to contribute and help others.

## Get started with NetDaemon

https://netdaemon.xyz/docs/user/started/get_started/

## About NetDaemon

NetDaemon was founded by @helto4real in 2020 as a personal project exploring the use of C# in Home Assistant.
Early contributions from @Ludeeus were crucial in enabling its NetDaemon integration.
The project gained significant momentum when @FrankBakkerNl joined and introduced the HassModel, a user-friendly API built leveraging code generation capabilities.
Currently, @helto4real and @FrankBakkerNl serve as the primary maintainers, though numerous individuals have contributed to the project's development over the years.

Contributions are welcome, and we encourage you to join the Discord server to learn more about how you can help.

## Developer notes

- Check out [NetDaemon developer site](https://netdaemon.xyz/docs/developer)

Check out `dotnet-outdated-tool` for automatic upgrades of all projects nuget packages.

Install the tool by running:
```bash
dotnet tool install --global dotnet-outdated-tool
```

Then run the following command to upgrade all packages to the latest version:

```bash
dotnet outdated --pre-release Never --upgrade
```
