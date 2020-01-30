<a name='assembly'></a>
# NetDeamon.App

## Contents

- [EntityManager](#T-JoySoftware-HomeAssistant-NetDaemon-Common-EntityManager 'JoySoftware.HomeAssistant.NetDaemon.Common.EntityManager')
  - [ExecuteAsync(keepItems)](#M-JoySoftware-HomeAssistant-NetDaemon-Common-EntityManager-ExecuteAsync-System-Boolean- 'JoySoftware.HomeAssistant.NetDaemon.Common.EntityManager.ExecuteAsync(System.Boolean)')
- [FluentExpandoObject](#T-JoySoftware-HomeAssistant-NetDaemon-Common-FluentExpandoObject 'JoySoftware.HomeAssistant.NetDaemon.Common.FluentExpandoObject')
  - [#ctor(ignoreCase,returnNullMissingProperties,root)](#M-JoySoftware-HomeAssistant-NetDaemon-Common-FluentExpandoObject-#ctor-System-Boolean,System-Boolean,System-Dynamic-ExpandoObject- 'JoySoftware.HomeAssistant.NetDaemon.Common.FluentExpandoObject.#ctor(System.Boolean,System.Boolean,System.Dynamic.ExpandoObject)')
  - [Augment()](#M-JoySoftware-HomeAssistant-NetDaemon-Common-FluentExpandoObject-Augment-JoySoftware-HomeAssistant-NetDaemon-Common-FluentExpandoObject- 'JoySoftware.HomeAssistant.NetDaemon.Common.FluentExpandoObject.Augment(JoySoftware.HomeAssistant.NetDaemon.Common.FluentExpandoObject)')
  - [HasProperty()](#M-JoySoftware-HomeAssistant-NetDaemon-Common-FluentExpandoObject-HasProperty-System-String- 'JoySoftware.HomeAssistant.NetDaemon.Common.FluentExpandoObject.HasProperty(System.String)')
  - [ToString()](#M-JoySoftware-HomeAssistant-NetDaemon-Common-FluentExpandoObject-ToString 'JoySoftware.HomeAssistant.NetDaemon.Common.FluentExpandoObject.ToString')
- [INetDaemon](#T-JoySoftware-HomeAssistant-NetDaemon-Common-INetDaemon 'JoySoftware.HomeAssistant.NetDaemon.Common.INetDaemon')
  - [ListenState(pattern,action)](#M-JoySoftware-HomeAssistant-NetDaemon-Common-INetDaemon-ListenState-System-String,System-Func{System-String,JoySoftware-HomeAssistant-NetDaemon-Common-EntityState,JoySoftware-HomeAssistant-NetDaemon-Common-EntityState,System-Threading-Tasks-Task}- 'JoySoftware.HomeAssistant.NetDaemon.Common.INetDaemon.ListenState(System.String,System.Func{System.String,JoySoftware.HomeAssistant.NetDaemon.Common.EntityState,JoySoftware.HomeAssistant.NetDaemon.Common.EntityState,System.Threading.Tasks.Task})')
- [INetDaemonApp](#T-JoySoftware-HomeAssistant-NetDaemon-Common-INetDaemonApp 'JoySoftware.HomeAssistant.NetDaemon.Common.INetDaemonApp')
  - [InitializeAsync(daemon)](#M-JoySoftware-HomeAssistant-NetDaemon-Common-INetDaemonApp-InitializeAsync 'JoySoftware.HomeAssistant.NetDaemon.Common.INetDaemonApp.InitializeAsync')
  - [StartUpAsync(daemon)](#M-JoySoftware-HomeAssistant-NetDaemon-Common-INetDaemonApp-StartUpAsync-JoySoftware-HomeAssistant-NetDaemon-Common-INetDaemon- 'JoySoftware.HomeAssistant.NetDaemon.Common.INetDaemonApp.StartUpAsync(JoySoftware.HomeAssistant.NetDaemon.Common.INetDaemon)')
- [NetDaemonApp](#T-JoySoftware-HomeAssistant-NetDaemon-Common-NetDaemonApp 'JoySoftware.HomeAssistant.NetDaemon.Common.NetDaemonApp')
  - [ListenState(pattern,action)](#M-JoySoftware-HomeAssistant-NetDaemon-Common-NetDaemonApp-ListenState-System-String,System-Func{System-String,JoySoftware-HomeAssistant-NetDaemon-Common-EntityState,JoySoftware-HomeAssistant-NetDaemon-Common-EntityState,System-Threading-Tasks-Task}- 'JoySoftware.HomeAssistant.NetDaemon.Common.NetDaemonApp.ListenState(System.String,System.Func{System.String,JoySoftware.HomeAssistant.NetDaemon.Common.EntityState,JoySoftware.HomeAssistant.NetDaemon.Common.EntityState,System.Threading.Tasks.Task})')

<a name='T-JoySoftware-HomeAssistant-NetDaemon-Common-EntityManager'></a>
## EntityManager `type`

##### Namespace

JoySoftware.HomeAssistant.NetDaemon.Common

<a name='M-JoySoftware-HomeAssistant-NetDaemon-Common-EntityManager-ExecuteAsync-System-Boolean-'></a>
### ExecuteAsync(keepItems) `method`

##### Summary

Executes the sequence of actions

##### Returns



##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| keepItems | [System.Boolean](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Boolean 'System.Boolean') | True if  you want to keep items |

##### Remarks

You want to keep the items when using this as part of an automation
    that are kept over time. Not keeping when just doing a command

<a name='T-JoySoftware-HomeAssistant-NetDaemon-Common-FluentExpandoObject'></a>
## FluentExpandoObject `type`

##### Namespace

JoySoftware.HomeAssistant.NetDaemon.Common

##### Summary

A custom expando object to alow to return null values if properties does not exist

##### Remarks

Thanks to @lukevendediger for original code and inspiration
    https://gist.github.com/lukevenediger/6327599

<a name='M-JoySoftware-HomeAssistant-NetDaemon-Common-FluentExpandoObject-#ctor-System-Boolean,System-Boolean,System-Dynamic-ExpandoObject-'></a>
### #ctor(ignoreCase,returnNullMissingProperties,root) `constructor`

##### Summary

Creates a BetterExpando object/

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| ignoreCase | [System.Boolean](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Boolean 'System.Boolean') | Don't be strict about property name casing. |
| returnNullMissingProperties | [System.Boolean](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Boolean 'System.Boolean') | If true, returns String.Empty for missing properties. |
| root | [System.Dynamic.ExpandoObject](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Dynamic.ExpandoObject 'System.Dynamic.ExpandoObject') | An ExpandoObject to consume and expose. |

<a name='M-JoySoftware-HomeAssistant-NetDaemon-Common-FluentExpandoObject-Augment-JoySoftware-HomeAssistant-NetDaemon-Common-FluentExpandoObject-'></a>
### Augment() `method`

##### Summary

Combine two instances together to get a union.

##### Returns

This instance but with additional properties

##### Parameters

This method has no parameters.

##### Remarks

Existing properties are not overwritten.

<a name='M-JoySoftware-HomeAssistant-NetDaemon-Common-FluentExpandoObject-HasProperty-System-String-'></a>
### HasProperty() `method`

##### Summary

Check if BetterExpando contains a property.

##### Parameters

This method has no parameters.

##### Remarks

Respects the case sensitivity setting

<a name='M-JoySoftware-HomeAssistant-NetDaemon-Common-FluentExpandoObject-ToString'></a>
### ToString() `method`

##### Summary

Returns this object as comma-separated name-value pairs.

##### Parameters

This method has no parameters.

<a name='T-JoySoftware-HomeAssistant-NetDaemon-Common-INetDaemon'></a>
## INetDaemon `type`

##### Namespace

JoySoftware.HomeAssistant.NetDaemon.Common

<a name='M-JoySoftware-HomeAssistant-NetDaemon-Common-INetDaemon-ListenState-System-String,System-Func{System-String,JoySoftware-HomeAssistant-NetDaemon-Common-EntityState,JoySoftware-HomeAssistant-NetDaemon-Common-EntityState,System-Threading-Tasks-Task}-'></a>
### ListenState(pattern,action) `method`

##### Summary

Listen to statechange

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| pattern | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | Match pattern, entity_id or domain |
| action | [System.Func{System.String,JoySoftware.HomeAssistant.NetDaemon.Common.EntityState,JoySoftware.HomeAssistant.NetDaemon.Common.EntityState,System.Threading.Tasks.Task}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{System.String,JoySoftware.HomeAssistant.NetDaemon.Common.EntityState,JoySoftware.HomeAssistant.NetDaemon.Common.EntityState,System.Threading.Tasks.Task}') | The func to call when matching |

##### Remarks

The callback function is
        - EntityId
        - newEvent
        - oldEvent

<a name='T-JoySoftware-HomeAssistant-NetDaemon-Common-INetDaemonApp'></a>
## INetDaemonApp `type`

##### Namespace

JoySoftware.HomeAssistant.NetDaemon.Common

##### Summary

Interface that all NetDaemon apps needs to implement

<a name='M-JoySoftware-HomeAssistant-NetDaemon-Common-INetDaemonApp-InitializeAsync'></a>
### InitializeAsync(daemon) `method`

##### Summary

Init the application, is called by the NetDaemon after startup

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| daemon | [M:JoySoftware.HomeAssistant.NetDaemon.Common.INetDaemonApp.InitializeAsync](#T-M-JoySoftware-HomeAssistant-NetDaemon-Common-INetDaemonApp-InitializeAsync 'M:JoySoftware.HomeAssistant.NetDaemon.Common.INetDaemonApp.InitializeAsync') |  |

<a name='M-JoySoftware-HomeAssistant-NetDaemon-Common-INetDaemonApp-StartUpAsync-JoySoftware-HomeAssistant-NetDaemon-Common-INetDaemon-'></a>
### StartUpAsync(daemon) `method`

##### Summary

Start the application, normally implemented by the base class

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| daemon | [JoySoftware.HomeAssistant.NetDaemon.Common.INetDaemon](#T-JoySoftware-HomeAssistant-NetDaemon-Common-INetDaemon 'JoySoftware.HomeAssistant.NetDaemon.Common.INetDaemon') |  |

<a name='T-JoySoftware-HomeAssistant-NetDaemon-Common-NetDaemonApp'></a>
## NetDaemonApp `type`

##### Namespace

JoySoftware.HomeAssistant.NetDaemon.Common

<a name='M-JoySoftware-HomeAssistant-NetDaemon-Common-NetDaemonApp-ListenState-System-String,System-Func{System-String,JoySoftware-HomeAssistant-NetDaemon-Common-EntityState,JoySoftware-HomeAssistant-NetDaemon-Common-EntityState,System-Threading-Tasks-Task}-'></a>
### ListenState(pattern,action) `method`

##### Summary

Listen for state changes and call a function when state changes

##### Parameters

| Name | Type | Description |
| ---- | ---- | ----------- |
| pattern | [System.String](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.String 'System.String') | Ientity id or domain |
| action | [System.Func{System.String,JoySoftware.HomeAssistant.NetDaemon.Common.EntityState,JoySoftware.HomeAssistant.NetDaemon.Common.EntityState,System.Threading.Tasks.Task}](http://msdn.microsoft.com/query/dev14.query?appId=Dev14IDEF1&l=EN-US&k=k:System.Func 'System.Func{System.String,JoySoftware.HomeAssistant.NetDaemon.Common.EntityState,JoySoftware.HomeAssistant.NetDaemon.Common.EntityState,System.Threading.Tasks.Task}') | The action to call when state is changed, see remarks |

##### Remarks

Make function like

```
ListenState("binary_sensor.pir", async (string entityId, EntityState newState, EntityState oldState) =&gt;
{
    await Task.Delay(1000);// Insert some code
});
```

Valid patterns are:
        light.thelight      - En entity id
        light   - No dot means the whole domain
        empty   - All events
