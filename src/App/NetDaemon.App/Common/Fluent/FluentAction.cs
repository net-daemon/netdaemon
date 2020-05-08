using System.Collections.Generic;

namespace JoySoftware.HomeAssistant.NetDaemon.Common
{
    /// <summary>
    ///     Represents data about an action in a fluent API
    /// </summary>
    internal class FluentAction
    {
        // Todo: refactor the action class to manage only data that is relevant

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="type">The type of action that is being managed</param>
        public FluentAction(FluentActionType type)
        {
            ActionType = type;
            Attributes = new Dictionary<string, object>();
        }

        /// <summary>
        ///     Type of action
        /// </summary>
        public FluentActionType ActionType { get; }

        /// <summary>
        ///     Attributes used in action if specified
        /// </summary>
        public Dictionary<string, object> Attributes { get; }

        /// <summary>
        ///     Message to speak if it is a speak action
        /// </summary>
        public string? MessageToSpeak { get; internal set; }

        /// <summary>
        ///     The state to manage for state actions
        /// </summary>
        public dynamic? State { get; set; }
    }
}