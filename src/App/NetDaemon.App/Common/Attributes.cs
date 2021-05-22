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
        // ReSharper disable once UnassignedGetOnlyAutoProperty
        public SupressLogType[]? LogTypes { get; }
    }

    /// <summary>
    ///     Attribute to mark function as callback for service calls
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class HomeAssistantServiceCallAttribute : System.Attribute { }
}