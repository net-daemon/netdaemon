using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using JoySoftware.HomeAssistant.Model;
using NetDaemon.Common;

[assembly: InternalsVisibleTo("NetDaemon.Daemon.Tests")]

namespace NetDaemon.Daemon
{
    /// <summary>
    ///     The interface that interacts with the daemon host main logic
    /// </summary>
    public interface INetDaemonHost : INetDaemon
    {
        /// <summary>
        ///     Initializes the NetDaemon
        /// </summary>
        /// <param name="appInstanceManager">Handles all instancing of applications. </param>
        Task Initialize(IInstanceDaemonApp appInstanceManager);

        /// <summary>
        ///     Listens to the given service in the netdaemon domain. Those subscritions
        ///     are used internally and are not postponed during reloading service daemons.
        /// </summary>
        /// <param name="service"> The name of the service. </param>
        /// <param name="action"> The async. action that is executed. </param>
        void ListenCompanionServiceCall(string service, Func<dynamic?, Task> action);

        /// <summary>
        ///     Reload all apps
        /// </summary>
        Task ReloadAllApps();

        /// <summary>
        ///     Runs the netdaemon host asynchronous.
        /// </summary>
        /// <param name="host"> The Home Assistant instance to which the daemon connected to. </param>
        /// <param name="port"> The port os the HASS instance. </param>
        /// <param name="ssl"> Flag indicating whether SSL should be used. </param>
        /// <param name="token"> User specific access token. </param>
        /// <param name="cancellationToken"> A cancallation token. </param>
        /// <returns> The operational task. </returns>
        Task Run(string host, short port, bool ssl, string token, CancellationToken cancellationToken);

        /// <summary>
        ///     Sends the status of the netdaemon host to Home Assistant.
        /// </summary>
        /// <param name="numberOfLoadedApps"> The number of loaded apps. </param>
        /// <param name="numberOfRunningApps"> The number of running apps. </param>
        Task SetDaemonStateAsync(int numberOfLoadedApps, int numberOfRunningApps);

        /// <summary>
        ///     Stops the netdaemon host asynchronous.
        /// </summary>
        /// <returns> The operational task. </returns>
        Task Stop();
        /// <summary>
        ///     Clears all app instances registered
        /// </summary>
        Task UnloadAllApps();

        /// <summary>
        ///     To subscribe to external event, typical the API does this
        /// </summary>
        /// <param name="func">callback function</param>
        void SubscribeToExternalEvents(Func<ExternalEventBase, Task> func);

        /// <summary>
        ///     Returns true of the NetDaemon custom component is installed
        /// </summary>
        bool HomeAssistantHasNetDaemonIntegration();
        void CallService(string domain, string service, HassTarget target, object? data = null, bool waitForResponse = false);
    }
}