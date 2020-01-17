
# Decribes the fluent DSL

## Turn on a light
Turn on the light named kitchen light.
```c#
Action
    .TurnOn
        .Light("kitchenlight")
    .ExecuteAsync();
```

```c#
Light("kitchenlight").TurnOn()
    .ExecuteAsync();
```
Or use the whole entity 
```c#
Action
    .TurnOn
        .Entity("light.kitchenlight")
    .ExecuteAsync();
```
```c#
Entity("light.kitchenlight").TurnOn()
    .ExecuteAsync();
```

## Turn off
Turn off the light named kitchen light
```c#
Action
    .TurnOff
        .Light("kitchenlight")
    .ExecuteAsync();
```
or use the whole entity
```c#
Action
    .TurnOff
        .Entity("light.kitchenlight")
    .ExecuteAsync();
```

## Turn on with conditions
Turn on the kitchen light when the state is off and supported features is 63
```c#
Action
    .TurnOff
        .Light("kitchenlight")
           .Where(n=>n.State=="off" and n.Property.SupportedFeatures==63)
    .ExecuteAsync();
```
```c#
Light("kitchenlight", 
        n=> n.State=="on" and 
            n.Attribute.SupportedFeatures==63)
    .TurnOff()
    .ExecuteAsync();
```
Turn on the kitchen light only if the sensor temp_kitchen has state < 20 and was last updated < Now

```c#
Action
    .TurnOn
        .Light("kitchenlight")
    .If
        Sensor("temp_kitchen")
            .State(n=> n.State > 20 and n.LastUpdated < DateTime.Now())
    .ExecuteAsync();
```

## When kitchen pir changed state to "on", turn on kitchen light
```c#
When
    BinarySensor("kitchen_pir")
        .StateChanged
            .To(n=> n.State == "on")
            .From(n=> n.State=="off")
    .TurnOn
        .Light("kitchenlight")
   .ExecuteAsync();
```