﻿using System.Reactive.Linq;

namespace NetDaemon.HassModel.Integration;

/// <summary>
///     Provides extensions on IHaContext to be used with the NetDaemon Home Assistant integration
/// </summary>
public static class IntegrationHaContextExtensions
{
    /// <summary>
    ///     Creates a service in Home Assistant and registers a callback to be invoked when the service is called
    /// </summary>
    /// <param name="haContext">IHaContext to use</param>
    /// <param name="serviceName">The name of the service to create in Home Assistant. Will be prefixes with 'NetDaemon.'</param>
    /// <param name="callBack">The Action to invoke when the Service is called</param>
    /// <typeparam name="T">Type to deserialize the payload of the service into</typeparam>
    public static void RegisterServiceCallBack<T>(this IHaContext haContext, string serviceName, Action<T> callBack) =>
        RegisterService<T>(haContext, serviceName).Subscribe(callBack);

    /// <summary>
    /// Creates a service in Home Assistant and returns an IObservable to use methods on
    /// </summary>
    /// <param name="haContext">IHaContext to use</param>
    /// <param name="serviceName">The name of the service to create in Home Assistant. Will be prefixes with 'NetDaemon.'</param>
    /// <typeparam name="T">Type to deserialize the payload of the service into</typeparam>
    /// <returns>IObservable to use methods on</returns>
    /// <exception cref="ArgumentException"></exception>
    public static IObservable<T> RegisterService<T>(this IHaContext haContext, string serviceName)
    {
        ArgumentNullException.ThrowIfNull(haContext);

        if (string.IsNullOrWhiteSpace(serviceName))
            throw new ArgumentException("serviceName must have a value", serviceName);

        haContext.CallService("netdaemon", "register_service", data: new { service = serviceName });

        return haContext.Events.Filter<HassServiceEventData<T>>("call_service")
            .Where(e => e.Data?.domain == "netdaemon"
                        && string.Equals(e.Data?.service, serviceName, StringComparison.OrdinalIgnoreCase))
            .Select(x => x.Data!.service_data);
    }

    /// <summary>
    /// Set application state
    /// </summary>
    /// <param name="haContext">IHaContext to use</param>
    /// <param name="entityId">EntityId of the entity to create</param>
    /// <param name="state">Entity state</param>
    /// <param name="attributes">Entity attributes</param>
    [Obsolete("SetEntityState is deprecated, use the MQTT extension instead to create entities.")]
    public static void SetEntityState(this IHaContext haContext, string entityId, string state,
        object? attributes = null)
    {
        ArgumentNullException.ThrowIfNull(haContext);
        var currentState = haContext.GetState(entityId);
        var service = currentState is null ? "entity_create" : "entity_update";
        // We have an integration that will help persist
        haContext.CallService("netdaemon", service,
            data: new
            {
                entity_id = entityId,
                state,
                attributes
            });
    }

    private record HassServiceEventData<T>(string domain, string service, T service_data);
}
