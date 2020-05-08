using System;
using System.Threading.Tasks;

namespace JoySoftware.HomeAssistant.NetDaemon.Common
{
    /// <summary>
    ///     Information about state changed actions
    /// </summary>
    internal class StateChangedInfo
    {
        /// <summary>
        ///     All changes tracked, even if only attributes
        /// </summary>
        public bool AllChanges { get; set; }

        /// <summary>
        ///     Entity changed
        /// </summary>
        public IEntity? Entity { get; set; }

        /// <summary>
        ///     Timespan it have kept same state
        /// </summary>
        public TimeSpan ForTimeSpan { get; set; }

        /// <summary>
        ///     From state
        /// </summary>
        public dynamic? From { get; set; }

        /// <summary>
        ///     To state
        /// </summary>
        public dynamic? To { get; set; }

        /// <summary>
        ///     Function to call when state changes
        /// </summary>
        public Func<string, EntityState?, EntityState?, Task>? FuncToCall { get; set; }

        /// <summary>
        ///     Filter state changes with lamda
        /// </summary>
        public Func<EntityState?, EntityState?, bool>? Lambda { get; set; }

        /// <summary>
        ///     Script to call when state changes
        /// </summary>
        public string[]? ScriptToCall { get; internal set; }
    }
}