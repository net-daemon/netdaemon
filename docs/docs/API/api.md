---
title: The NetDaemon API
---

# NetDaemon API

The netdaemon API is used to access Home Assistant features. There are a classic API and a fluent API. **The project is in pre-alpha stage so the API is bound to change!**

It is up to the writer to decide what API suits bests their needs.

## Fluent API

Example turn on light using the fluent API.

### Select entities

Select one or more entities to perform any kind of actions on.

Examples:

```c#
await Entity("light.tomas_rum_fonster")
    .TurnOn()
        .WithAttribute("brightness", 50)
            .ExecuteAsync();

```

## The standard API

Examples turn on light using the standard API using CallService or the more direct TurnOnAsync method.

```c#
await await CallService("light", "turn_on",
    new { entity_id = "light.tomas_rum_fonster", brightness = 50});

await TurnOnAsync("light.tomas_rum_fonster", ("brightness", 50));
```
