
using System;
using System.Threading.Tasks;
using JoySoftware.HomeAssistant.NetDaemon.Common;

// public static class ServiceCallExtensions
// {
//     public static Task ToggleAsync(this NetDaemonApp app, string entityId, params (string name, object val)[] attributeNameValuePair)
//     {
//         // Get the domain if supported, else domain is homeassistant
//         string domain = GetDomainFromEntity(entityId);
//         // Use it if it is supported else use default "homeassistant" domain

//         // Use expando object as all other methods
//         dynamic attributes = attributeNameValuePair.ToDynamic();
//         // and add the entity id dynamically
//         attributes.entity_id = entityId;

//         return app.CallService(domain, "toggle", attributes, false);
//     }

//     public static Task TurnOffAsync(this NetDaemonApp app, string entityId, params (string name, object val)[] attributeNameValuePair)
//     {
//         // Get the domain if supported, else domain is homeassistant
//         string domain = GetDomainFromEntity(entityId);
//         // Use it if it is supported else use default "homeassistant" domain

//         // Use expando object as all other methods
//         dynamic attributes = attributeNameValuePair.ToDynamic();
//         // and add the entity id dynamically
//         attributes.entity_id = entityId;

//         return app.CallService(domain, "turn_off", attributes, false);
//     }

//     public static Task TurnOnAsync(this NetDaemonApp app, string entityId, params (string name, object val)[] attributeNameValuePair)
//     {
//         // Use default domain "homeassistant" if supported is missing
//         string domain = GetDomainFromEntity(entityId);
//         // Use it if it is supported else use default "homeassistant" domain

//         // Convert the value pairs to dynamic type
//         dynamic attributes = attributeNameValuePair.ToDynamic();
//         // and add the entity id dynamically
//         attributes.entity_id = entityId;

//         return app.CallService(domain, "turn_on", attributes, false);
//     }

//     private static string GetDomainFromEntity(string entity)
//     {
//         var entityParts = entity.Split('.');
//         if (entityParts.Length != 2)
//             throw new ApplicationException($"entity_id is mal formatted {entity}");

//         return entityParts[0];
//     }
// }