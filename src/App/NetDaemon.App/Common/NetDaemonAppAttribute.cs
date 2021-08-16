using System;

namespace NetDaemon.Common
{
    /// <summary>
    /// Marks a class as a NetDaemonApp
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class NetDaemonAppAttribute : Attribute
    {
        /// <summary>
        /// Id of an app
        /// </summary>
        public string? Id { get; init; }
    }
}