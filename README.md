# NetDaemon - Write your automations in C# for Home Assistant

[![CI build](https://github.com/net-daemon/netdaemon/actions/workflows/ci_build.yml/badge.svg)](https://github.com/net-daemon/netdaemon/actions/workflows/ci_build.yml)
[![Coverage Status](https://coveralls.io/repos/github/net-daemon/netdaemon/badge.svg?branch=dev)](https://coveralls.io/github/net-daemon/netdaemon?branch=dev)

Welcome to the NetDaemon project!

NetDaemon is an application daemon that enables you to write powerful home automation scripts in C# for Home Assistant. This repository contains **NetDaemon V5**, the latest version of the framework.

## About NetDaemon

NetDaemon was founded by [@helto4real](https://github.com/helto4real) in 2020 as a personal project to explore the use of C# in Home Assistant. Early contributions from [@Ludeeus](https://github.com/ludeeus) helped integrate NetDaemon with Home Assistant. The project gained significant momentum when [@FrankBakkerNl](https://github.com/FrankBakkerNl) joined and introduced the HassModel API, which leverages code generation to create a user-friendly experience.

Currently, [@helto4real](https://github.com/helto4real) and [@FrankBakkerNl](https://github.com/FrankBakkerNl) serve as the primary maintainers, though many others have contributed over the years.

## Getting Started

To learn how to install and use NetDaemon, visit our official documentation:

[🔗 Getting Started Guide](https://netdaemon.xyz/docs/user/started/get_started/)

## Usage

NetDaemon allows you to write your automations easily and cleanly using C#.

```cs
[NetDaemonApp]
class MyApp
{
    public MyApp(Entities entities)
    {
        LightEntity hallwayLight = entities.Light.HallwayLight;
        BinarySensorEntity motionSensor = entities.BinarySensor.HallwayMotionSensor;
        
        // Check state of entities directly
        if (motionSensor.IsOn() && hallwayLight.IsOff()){
            hallwayLight.TurnOn();
        }

        // Subscribe to changes in the state of the motion sensor
        motionSensor.StateChanges()
            .Where(e => e.New?.IsOn() ?? false)
            .Subscribe(_ => hallwayLight.TurnOn());
    }
}
```

## Support & Community
If you have issues or suggestions, please feel free to:

- [Open an issue](https://github.com/net-daemon/netdaemon/issues)
- Join our [Discord server](https://discord.gg/K3xwfcX) for support, discussions, and contributions.

Contributions are welcome! If you'd like to help improve NetDaemon, we encourage you to join our [Discord server](https://discord.gg/K3xwfcX) to learn more about how you can contribute to the project.

## Release Notes

Check out the [Release Notes](https://github.com/net-daemon/netdaemon/releases) for detailed information on the latest changes, bug fixes, and new features.

NetDaemon is stable, and we're committed to maintaining that stability by minimizing breaking changes in future releases.

## Versioning

NetDaemon uses the CalVer versioning system for its NuGet packages. The version format is `YYYY.WW.PATCH`, where:

- `YYYY.WW` represents the year and week number (01-52).
- `PATCH` indicates the patch version of the release.

## Developer notes

- Visit the [NetDaemon Developer Site](https://netdaemon.xyz/docs/developer) for development resources.

To automatically upgrade all NuGet packages, use the `dotnet-outdated-tool`:

1. Install the tool with the following command:
```bash
dotnet tool install --global dotnet-outdated-tool
```

2. Run this command to upgrade all packages:

```bash
dotnet outdated --pre-release Never --upgrade
```
