using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NetDaemon.Common
{
    /// <summary>
    /// Interface to support async initialization of apps
    /// </summary>
    public interface IInitializable
    {
        /// <summary>
        /// Init the application, is called by the NetDaemon after startup
        /// </summary>
        void Initialize();
    }
  
    /// <summary>
    /// Interface to support initialization of apps
    /// </summary>
    public interface IAsyncInitializable
    {
        /// <summary>
        /// Init the application async, is called by the NetDaemon after startup
        /// </summary>
        Task InitializeAsync();
    }
}