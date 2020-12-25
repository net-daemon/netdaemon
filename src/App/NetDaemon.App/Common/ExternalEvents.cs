
using System;
using System.Collections.Generic;
using NetDaemon.Common.Configuration;

namespace NetDaemon.Common
{
    /// <summary>
    ///     Base class for all external events
    /// </summary>
    public class ExternalEventBase
    {
    }

    /// <summary>
    ///     Sent when app information are changed in the netdaemon
    /// </summary>
    public class AppsInformationEvent : ExternalEventBase
    {
    }

    /// <summary>
    ///     Information about the application
    /// </summary>
    public class ApplicationInfo
    {
        /// <summary>
        ///     Unique id
        /// </summary>
        public string? Id { get; set; }
        /// <summary>
        ///     All application dependencies
        /// </summary>
        public IEnumerable<string>? Dependencies { get; set; }

        /// <summary>
        ///     If app is enabled or disabled
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        ///     Application description
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        ///     Next scheduled event
        /// </summary>
        public DateTime? NextScheduledEvent { get; set; }

        /// <summary>
        ///     Last known error message
        /// </summary>
        public string? LastErrorMessage { get; set; }
    }

    /// <summary>
    ///     All config information
    /// </summary>
    public class ConfigInfo
    {
        /// <summary>
        ///     Settings for NetDaemon
        /// </summary>
        public NetDaemonSettings? DaemonSettings { get; set; }
        /// <summary>
        ///     Settings Home Assistant related
        /// </summary>
        public HomeAssistantSettings? HomeAssistantSettings { get; set; }
    }
}