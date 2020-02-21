---
title: Welcome
summary: Application daemon written in .NET core.
authors:
    - Tomas Hellström (@helto4real)
---
# NetDaemon

This is the application daemon project for Home Assistant. This project makes it possible to make automations using the .NET Core (3.1) framework.

Why a new application daemon for Home Assistant? There already exists one!? The existing appdaemon is a great software and are using python as language and ecosystem. This is for people who loves to code in the .NET core ecosystem and c#. The daemon will be supported by all supported platforms of .NET core.

## Pre-Alpha - Expect things to change!

This is in pre-alpha experimental phase and expect API:s to change over time. Please use and contribute ideas for improvement or better yet PR:s.

Only amd64 (non arm) is currently supported but ARM devices as Raspberry PI will be supported in the near future.

The daemon is currently only distributed through Hassio add-on but a docker container and instruction to run locally will be provided in time.

Please see [the getting started](/netdaemon/Getting%20started/getting%20started/) documentation for setup.

> **IMPORTANT - YOU NEED TO RESTART THE ADD-ON EVERYTIME YOU MAKE CHANGES TO A FILE**

## Async model

The application daemon are built entirely on the async model of .NET. This requires some knowledge of async/await/Tasks to use it properly. The docs will give you tips with do and don´ts around this but I strongly suggest you read the official docs.  [Here is a good start to read about async model.](https://docs.microsoft.com/en-us/dotnet/csharp/async)
