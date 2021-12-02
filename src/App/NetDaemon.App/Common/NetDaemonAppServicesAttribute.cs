using System;

namespace NetDaemon.Common
{
    /// <summary>
    /// Marks a class as a NetDaemonApp
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class NetDaemonAppServicesAttribute : Attribute
    {
        /// <summary>
        /// Id of an app services
        /// </summary>
        public string? Id { get; init; }
    }
}