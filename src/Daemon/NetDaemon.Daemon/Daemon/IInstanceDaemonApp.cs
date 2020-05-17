using JoySoftware.HomeAssistant.NetDaemon.Common;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("NetDaemon.Daemon.Tests")]

namespace JoySoftware.HomeAssistant.NetDaemon.Daemon
{
    /// <summary>
    ///     Interface for objects implementing the InstanceDeamonApps features
    /// </summary>
    public interface IInstanceDaemonApp
    {
        /// <summary>
        ///     Number of instanced deamonapps
        /// </summary>
        int Count { get; }

        /// <summary>
        ///     Returns a list of instanced daemonapps
        /// </summary>
        /// <returns></returns>
        IEnumerable<INetDaemonAppBase> InstanceDaemonApps();
    }
}