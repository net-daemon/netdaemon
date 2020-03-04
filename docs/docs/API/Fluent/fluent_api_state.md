---
title: Fluent API
---

# The fluent API

## Basic state management

The fluent API lets you easily subscribe to state changes.

Let´s start with a very basic example. If the motion sensors state turns to "on", turn on the `light.light1` light. Here we do not use ExecuteAsync since the setup is a synchronous function (`Execute`).  The `WhenStateChange` method makes sure we subscribe to the specific state changes from anything to state `"on"`.

```c#
Entity("binary_sensor.my_motion_sensor").WhenStateChange(to: "on").UseEntity("light.light1").TurnOn().Execute();
```

If the from state is important then use it like:

```c#
Entity("binary_sensor.my_motion_sensor").WhenStateChange(from: "off", to:"on").UseEntity("light.light1").TurnOn().Execute();
```

You can also use lambda expressions to select state changes:

```c#
Entity("binary_sensor.my_motion_sensor").WhenStateChange((toState, fromState) => toState?.State == "on").UseEntity("light.light1").TurnOn().Execute();
```

Or even more advanced example. You can use any combination of state and attributes or even external methods i lambda expressions. Here is an example: When sun elevation below 3.0 and not rising and old state elevation is above 3.0 then we should turn on light.

```c#
Entity("sun.sun")
    .WhenStateChange((n, o) =>
        n?.Attribute?.elevation <= 3.0 &&
        n?.Attribute?.rising == false &&
        o?.Attribute?.elevation >  3.0).UseEntity("light.light1").TurnOn().Execute();
```

if you rather call a function when a state changes you can use Func<> for that like the example below.

```c#
Entity("binary_sensor.my_motion_sensor").WhenStateChange(to: "on").Call(async (entityId, newState, oldState) =>
{
    await Entity("light.light1").TurnOn().ExecuteAsync();
}).Execute();
```

If you prefer to call a method you can use following syntax:

```c#
Entity("binary_sensor.my_motion_sensor").WhenStateChange(to: "on").Call(MyMotionSensorStateChange).Execute();
```

And the method for state changes looks like:

```c#
private async Task MyMotionSensorStateChange(string entityId, EntityState? newState, EntityState? oldState)
{
    await Entity("light.light1").TurnOn().ExecuteAsync();
}
```
