using System;

namespace NetDaemon.Common
{
    /// <summary>
    /// Provides application runtime data to apps
    /// </summary>
    public interface IApplicationContext
    {
        /// <summary>
        ///     Unique id of the application
        /// </summary>
        string? Id { get; }

        /// <summary>
        ///     Gets whether this app is enabled.
        /// </summary>
        bool IsEnabled { get; }

        /// <summary>
        ///     Unique id of the application entity
        /// </summary>
        string EntityId => $"switch.netdaemon_{Id?.ToSafeHomeAssistantEntityId()}";
    }
}