---
title: Example app
---
# Example app

This application shows basic capabilities of the fluent API of NetDaemon. It has two files, `ExampleApp.yaml` that contains basic configuration of the instance and `ExampleApp.cs` that contains the app logic.

**ExampleApp.yaml**
```yaml
example_app:
    class: ExampleApp
```

**ExampleApp.cs**
```c#
using System;
using System.Threading.Tasks;
using JoySoftware.HomeAssistant.NetDaemon.Common;

/// <summary>
///     Example app
/// </summary>
public class ExampleApp : NetDaemonApp
{
    public override async Task InitializeAsync()
    {
        Entity("binary_sensor.kitchen_pir")
        .WhenStateChange(to: "on")
            .UseEntity("light.kitchen_light")
                .TurnOn()
        .Execute();

        Entity("binary_sensor.kitchen_pir")
        .WhenStateChange(to: "off")
            .AndNotChangeFor(TimeSpan.FromMinutes(10))
                .UseEntity("light.kitchen_light")
                    .TurnOff()
        .Execute();

    }
}
```

## The NetDaemonApp base class

```c#
public class ExampleApp : NetDaemonApp
```

All applications in netdaemon have to inherit the NetDaemonApp base class. This provides discoverability and functionality to the application.

## The InitializeAsync function

```c#
public override async Task InitializeAsync()
```

This async function is called by the daemon and it´s purpose is to do all the initialization of your application. **Never block this function!** Typically you configure what should happen when a state change or run a function every minute for an example.

**Example:**

```c#
    Entity("binary_sensor.kitchen_pir")
        .WhenStateChange(to: "off")
            .AndNotChangeFor(TimeSpan.FromMinutes(10))
                .UseEntity("light.kitchen_light")
                    .TurnOff()
        .Execute();
```


| Function        | Description                                                                               |
| --------------- | ----------------------------------------------------------------------------------------- |
| Entity          | Selects one or more entities where actions are applied                                    |
| WhenStateChange | If state changes on previously defined entity do action                                   |
| AndNotChangeFor | Do action only if state has not change for a period of time (10 minutes)                  |
| UseEntity       | The action on previously selected entity/ies                                              |
| TurnOff         | The action on previously selected entity/ies                                              |
| Execute         | Ends the api call. You cannot skip this function or the automation will not be activated! |