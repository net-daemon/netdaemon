using System.Collections.Generic;

namespace JoySoftware.HomeAssistant.NetDaemon.Common
{
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
        // See the attribute guidelines at
        //  http://go.microsoft.com/fwlink/?LinkId=85236
        private string _entityId;

        private readonly object? _to;
        private readonly object? _from;
        private readonly bool _allChanges;

        /// <summary>
        ///     Default constructor
        /// </summary>
        /// <param name="entityId">Unique id of entity</param>
        /// <param name="to">To state filtered</param>
        /// <param name="from">From state filtered</param>
        /// <param name="allChanges">Get all changes, ie also attribute changes</param>
        public HomeAssistantStateChangedAttribute(string entityId, object? to = null, object? from = null, bool allChanges = false)
        {
            _entityId = entityId;
            _to = to;
            _from = from;
            _allChanges = allChanges;
        }

        /// <summary>
        ///     Unique id of the entity
        /// </summary>
        public string EntityId => _entityId;

        /// <summary>
        ///     Get all changes, even if only attribute changes
        /// </summary>
        public bool AllChanges => _allChanges;

        /// <summary>
        ///     From state filter
        /// </summary>
        public object? From => _from;

        /// <summary>
        ///     To state filter
        /// </summary>
        public object? To => _to;
    }

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
        private SupressLogType[] _logTypesToSupress;


        /// <summary>
        ///     Default constructor
        /// </summary>
        /// <param name="logTypes">List of logtypes to supress</param>
        public DisableLogAttribute(params SupressLogType[] logTypes)
        {
            _logTypesToSupress = logTypes;
        }

        /// <summary>
        ///     Log types to supress
        /// </summary>
        public IEnumerable<SupressLogType> LogTypesToSupress => _logTypesToSupress;
    }
}