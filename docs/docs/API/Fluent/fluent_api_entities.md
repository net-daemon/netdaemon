---
title: Fluent API
---

# The fluent API

## Entity and Entities selection

To select whant entity you want to perform actions on use the `Entity()` or `Entities()` starting point.

### Simple selection

**Example 1: Selects one entity to perform action on**
```c#
await Entity("light.light1").TurnOn().ExecuteAsync();
```
This selects the `light.light1` to perform the `TurnOn` action on. A full fluent API command ends with `ExecuteAsync()` that execute the command now.

**Example 2: Selects multiple entities to perform action on**
Here we have several options to turn on both light1 and light2.

```c#
await Entity("light.light1", "light.light2").TurnOn().ExecuteAsync();
```

This one takes a IEnumerable<string> as input to selects multiple lights

```c#
await Entities(new string[]{"light.light1", "light.light2"}).TurnOn().ExecuteAsync();
```
**Example 3: Selects multiple entities to perform action on using lambda**

You can also use lambda expressions to select entities like select all lights that start name with `light.kitchen_`. Now it gets really interesting to use advanced selections with little code using linq.

```c#
await Entities(n => n.EntityId.StartsWith("light.kitchen_")).TurnOn().ExecuteAsync();
```
or select on attributes

```c#
await Entities(n => n.EntityId.StartsWith("light.kitchen_")).TurnOn().ExecuteAsync();
```


## Special entities

There are some entities that has native support in the API.

### MediaPlayer

Media player has support for the most common service calls through the FluentAPI.

Example:
```c#
await MediaPlayer("media_player.myplayer").Play().ExecuteAsync();
await MediaPlayer("media_player.myplayer").Stop().ExecuteAsync();
await MediaPlayer("media_player.myplayer").PlayPause().ExecuteAsync();
await MediaPlayer("media_player.myplayer").Pause().ExecuteAsync();
```

The same multiple selections with `IEnumerable<string>` and lambdas are supported like the `Entities`
Lambdas can be used on states and attributes too. Like stop all mediaplayers currently playing something:

```c#
await MediaPlayers(n => n.State == "playing").Stop().ExecuteAsync();
```

### InputSelect

Todo: document input_select


