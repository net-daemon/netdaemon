using System;

namespace NetDaemon.Common.Fluent
{
    /// <summary>
    ///     Properties on entities that can be filtered in lambda expression
    /// </summary>
    public interface IEntityProperties
    {
        /// <summary>
        ///     Filter on area where the entity device are at
        /// </summary>
        string? Area { get; set; }

        /// <summary>
        ///     Filter on attribute
        /// </summary>
        dynamic? Attribute { get; set; }

        /// <summary>
        ///     Filter on unique id of the entity
        /// </summary>
        string EntityId { get; set; }

        /// <summary>
        ///     Filter on last changed time
        /// </summary>
        DateTime LastChanged { get; set; }

        /// <summary>
        ///     Filter on last updated time
        /// </summary>
        DateTime LastUpdated { get; set; }

        /// <summary>
        ///     Filter on state
        /// </summary>
        dynamic? State { get; set; }

        /// <summary>
        ///     Context
        /// </summary>
        public Context? Context { get; set; }
    }
}