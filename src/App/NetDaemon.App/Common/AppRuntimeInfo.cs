using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NetDaemon.Common
{
    /// <summary>
    ///     Runtime information an app instance. The app instance is a switch in
    ///     Home Assistant where the runtime information is shown as attributes
    /// </summary>
    public sealed class AppRuntimeInfo
    {
        /// <summary>
        ///     Next event being scheduled
        /// </summary>
        [JsonPropertyName("next_scheduled_event")]
        public DateTime? NextScheduledEvent { get; set; }

        /// <summary>
        ///     Next event being scheduled
        /// </summary>
        [JsonPropertyName("last_error_message")]
        public string? LastErrorMessage { get; set; }

        /// <summary>
        ///     Next event being scheduled
        /// </summary>
        [JsonPropertyName("has_error")]
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public bool? HasError { get; set; }

        /// <summary>
        ///     Custom attributes that the apps can set that are shown
        ///     in app switch
        /// </summary>
        [JsonPropertyName("app_attributes")]
        public Dictionary<string, object> AppAttributes { get; } = new();
    }
}


