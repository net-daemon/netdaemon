using System;

namespace NetDaemon.Common
{
    /// <summary>
    /// Marks a class as a NetDaemonApp
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class NetDaemonAppAttribute : Attribute
    { }
}