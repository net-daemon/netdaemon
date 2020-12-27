using System.Collections.Generic;

namespace NetDaemon.Common
{
    /// <summary>
    ///     Type of log to supress
    /// </summary>
    public enum SupressLogType
    {
        /// <summary>
        ///     Supress Missing Execute Error in a method
        /// </summary>
        MissingExecute
    }

    /// <summary>
    ///     Attribute to mark function as callback for state changes
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public sealed class DisableLogAttribute : System.Attribute
    {
        // See the attribute guidelines at
        //  http://go.microsoft.com/fwlink/?LinkId=85236
        private readonly SupressLogType[]? _logTypesToSupress;

        /// <summary>
        ///     Default constructor
        /// </summary>
        /// <param name="logTypes">List of logtypes to supress</param>
        public DisableLogAttribute(params SupressLogType[] logTypes) => _logTypesToSupress = logTypes;

        /// <summary>
        ///     Log types to supress
        /// </summary>
        public IEnumerable<SupressLogType>? LogTypesToSupress => _logTypesToSupress;

        /// <summary>
        ///     Log tupes used
        /// </summary>
        public SupressLogType[]? LogTypes { get; }
    }

    /// <summary>
    ///     Attribute to mark function as callback for service calls
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class HomeAssistantServiceCallAttribute : System.Attribute { }

    /// <summary>
    ///     Attribute to mark function as callback for state changes
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public sealed class HomeAssistantStateChangedAttribute : System.Attribute
    {
        /// <summary>
        ///     Default constructor
        /// </summary>
        /// <param name="entityId">Unique id of entity</param>
        /// <param name="to">To state filtered</param>
        /// <param name="from">From state filtered</param>
        /// <param name="allChanges">Get all changes, ie also attribute changes</param>
        public HomeAssistantStateChangedAttribute(string entityId, object? to = null, object? from = null, bool allChanges = false)
        {
            EntityId = entityId;
            To = to;
            From = from;
            AllChanges = allChanges;
        }

        /// <summary>
        ///     Get all changes, even if only attribute changes
        /// </summary>
        public bool AllChanges { get; }

        /// <summary>
        ///     Unique id of the entity
        /// </summary>
        public string EntityId { get; }

        /// <summary>
        ///     From state filter
        /// </summary>
        public object? From { get; }

        /// <summary>
        ///     To state filter
        /// </summary>
        public object? To { get; }
    }
}