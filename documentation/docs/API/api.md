---
title: API
---

# NetDaemon API

The netdaemon API is used to access Home Assistant features. There are a classic API and a fluent API. **The project is in pre-alpha stage so the API is bound to change!**

## Fluent API

### Select entities

Select one or more entities to perform any kind of actions on.

Examples:

```c#
// Selects one entity and call action TurnOn.
await Entity("binary_sensor.my_pir").TurnOn().ExecuteAsync();

// Selects multiple entities and call action TurnOn
await Entity(
    "binary_sensor.my_pir",
    "binary_sensor.another_pir"
    ).TurnOn().ExecuteAsync();

```

>TODO: Document more!!