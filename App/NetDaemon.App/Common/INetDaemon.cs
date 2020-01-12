using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace JoySoftware.HomeAssistant.NetDaemon.Common
{
    /// <summary>
    /// Interface that all NetDaemon apps needs to implement
    /// </summary>
    public interface INetDaemonApp
    {
        /// <summary>
        /// Start the application, normally implemented by the base class
        /// </summary>
        /// <param name="daemon"></param>
        Task StartUpAsync(INetDaemon daemon);

        /// <summary>
        /// Init the application, is called by the NetDaemon after startup
        /// </summary>
        /// <param name="daemon"></param>
        Task InitializeAsync();

        
    }

   
    public interface INetDaemon
    {
        ILogger Logger { get; }
        Task ListenStateAsync(string pattern, Func<StateChangedEvent, Task> action);
        Task TurnOnAsync(string entityId, params (string name, object val)[] attributes);
        Task TurnOffAsync(string entityIds, params (string name, object val)[] attributes);
        Task ToggleAsync(string entityIds, params (string name, object val)[] attributes);
        EntityState? GetState(string entity);

        IAction Action { get; }
      
    
    }
}
