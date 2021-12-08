using System;

namespace NetDaemon.Common
{
    /// <summary>
    /// Marks a class as a NetDaemon services provider.
    /// Each service provider classes with a ConfigureService() method will be executed at NetDaemon startup.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class NetDaemonServicesProviderAttribute : Attribute
    {
        /// <summary>
        /// Id of a services provider
        /// </summary>
        public string? Id { get; init; }
    }
}