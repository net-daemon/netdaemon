
# Decribes the fluent DSL

## Turn on a light
Turn on the light named kitchen light.

```c#
Light("kitchenlight").TurnOn()
    .ExecuteAsync();
```
Or use the whole entity 
```c#
Entity("light.kitchenlight").TurnOn()
    .ExecuteAsync();
```

## Turn off
Turn off the light named kitchen light
or use the whole entity

## Turn on with conditions
Turn on the kitchen light when the state is off and supported features is 63
```c#
Light("kitchenlight", 
        n=> n.State=="on" and 
            n.Attribute.SupportedFeatures==63)
    .TurnOff()
    .ExecuteAsync();
```
Turn on the kitchen light only if the sensor temp_kitchen has state < 20 and was last updated < Now


## When kitchen pir changed state to "on", turn on kitchen light
```c#

```