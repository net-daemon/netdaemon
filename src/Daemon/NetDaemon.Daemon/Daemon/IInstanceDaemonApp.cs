using System.Collections.Generic;
using System.Runtime.CompilerServices;
using NetDaemon.Common;

[assembly: InternalsVisibleTo("NetDaemon.Daemon.Tests")]

namespace NetDaemon.Daemon
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