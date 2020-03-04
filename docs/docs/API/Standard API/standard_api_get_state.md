---
title: Fluent API
---

# The fluent API

## Get State of an entity

The method for getting state is using the `State` dictionary. It is thread safe dictionary containing all states in Home Assistant. All states are read at start-up and kept in sync by the netdaemon. This means that getting current state does not make a network call to Home Assistant that in most cases will be cached value. This is very efficient performance wise and will be sufficient in most cases. There might me an API addition in the future that let you get the value from the server directly.

Basic example getting state:

Using the new nullable features in c# 8 you can easily get state or null (no state found)

```c#
string? state = GetState("light.light1")?.State;
if (state != null)
{
    ...
}

```

Or get all information from the entity state:

```c#
string? entityState = GetState("light.light1");
if (entityState != null)
{
    var entityId = entityState.EntityId;
    var state = entityState.State;
    var brightness = entityState?.Attribute?.brightness;
    var lastUpdated = entityState?.LastUpdated; //DateTime in local time
    var lastChanged = entityState?.LastChanged; //DateTime in local time
}

```
