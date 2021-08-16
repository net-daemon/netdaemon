using System;
using System.Threading.Tasks;

namespace NetDaemon.Common
{
    /// <summary>
    ///     Interface that all NetDaemon apps needs to implement
    /// </summary>
    public interface INetDaemon : INetDaemonCommon
    {
        /// <summary>
        ///     Returns true if NetDaemon is connected to Home Assistant
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        ///     Http features of NetDaemon is exposed through the Http property
        /// </summary>
        IHttpHandler Http { get; }

        /// <summary>
        ///     Calls a service
        /// </summary>
        /// <param name="domain">The domain of the service</param>
        /// <param name="service">The service being called</param>
        /// <param name="data">Any data that the service requires</param>
        /// <param name="waitForResponse">Waits for Home Assistant to return result before returning</param>
        void CallService(string domain, string service, dynamic? data = null, bool waitForResponse = false);

        /// <summary>
        ///     Calls a service
        /// </summary>
        /// <param name="domain">The domain of the service</param>
        /// <param name="service">The service being called</param>
        /// <param name="data">Any data that the service requires</param>
        /// <param name="waitForResponse">If we should wait for the service to get response from Home Assistant or send/forget scenario</param>
        Task CallServiceAsync(string domain, string service, dynamic? data = null, bool waitForResponse = false);

        /// <summary>
        ///     Trigger a state change using trigger templates
        /// </summary>
        /// <param name="id">webhook id</param>
        /// <param name="data">data being sent</param>
        /// <param name="waitForResponse">If we should wait for the service to get response from Home Assistant or send/forget scenario</param>
        void TriggerWebhook(string id, object? data, bool waitForResponse = false);

        /// <summary>
        ///     Get application instance by application instance id
        /// </summary>
        /// <param name="appInstanceId">The identity of the app instance</param>
        INetDaemonAppBase? GetApp(string appInstanceId);

        /// <summary>
        ///     Set entity state
        /// </summary>
        /// <param name="entityId">Entity unique id</param>
        /// <param name="state">The state that being set, only primitives are supported</param>
        /// <param name="attributes">Attributes, use anonomous types and lowercase letters</param>
        /// <param name="waitForResponse">If true, waits for object to set and returns the new EntityState else returns null</param>
        /// <returns>Returns entitystate if waitForResponse is true</returns>
        EntityState? SetState(string entityId, dynamic state, dynamic? attributes = null, bool waitForResponse = false);

        /// <summary>
        ///     Access the underlying service provider for IOT access to services
        /// </summary>
        public IServiceProvider? ServiceProvider { get; }
    }
}