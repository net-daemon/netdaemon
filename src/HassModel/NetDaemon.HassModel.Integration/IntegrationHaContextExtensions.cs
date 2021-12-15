using System.Reactive.Linq;
using NetDaemon.HassModel.Common;

namespace NetDaemon.HassModel.Integration;

/// <summary>
/// Provides extensions on IHaContext to be used with the NetDaemon Home Assistant integration
/// </summary>
public static class IntegrationHaContextExtensions
{
    /// <summary>
    ///      Creates a service in Home Assistant and registers a callback to be invoked when the service is called
    /// </summary>
    /// <param name="haContext">IHaContext to use</param>
    /// <param name="serviceName">The name of the service to create in Home Assistant. Will be prefixes with 'NetDaemon.'</param>
    /// <param name="callBack">The Action to invoke when the Service is called</param>
    /// <typeparam name="T">Type to deserialize the payload of the service into</typeparam>
    public static void RegisterServiceCallBack<T>(this IHaContext haContext, string serviceName, Action<T> callBack)
    {
        ArgumentNullException.ThrowIfNull(haContext);
        if (string.IsNullOrWhiteSpace(serviceName)) throw new ArgumentException("serviceName must have a value", serviceName);

        haContext.CallService("netdaemon", "register_service", data: new { service = serviceName });

        haContext.Events.Filter<HassServiceEventData<T>>("call_service")
            .Where(e => e.Data?.domain == "netdaemon"
                        && string.Equals(e.Data?.service, serviceName, StringComparison.OrdinalIgnoreCase))
            .Subscribe(e => callBack.Invoke(e.Data!.service_data));
    }

    private record HassServiceEventData<T>(string domain, string service, T service_data);
}

