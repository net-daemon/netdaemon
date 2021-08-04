using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using NetDaemon.Common;

[assembly: InternalsVisibleTo("NetDaemon.Daemon.Tests")]

namespace NetDaemon.Daemon
{
    /// <summary>
    ///     Interface for objects implementing the InstanceDaemonApps features
    /// </summary>
    public interface IInstanceDaemonApp
    {
        /// <summary>
        ///     Number of instanced daemonapps
        /// </summary>
        int Count { get; }

        /// <summary>
        ///     Returns a list of instanced daemonapps
        /// </summary>
        /// <param name="serviceProvider"></param>
        IEnumerable<INetDaemonApp> InstanceDaemonApps(IServiceProvider serviceProvider);
    }
}