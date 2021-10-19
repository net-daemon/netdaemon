using System;
using System.Text.Json;

namespace NetDaemon.HassModel.Common
{
    /// <summary>
    /// Represents an Event from  Home Assistant
    /// </summary>
    public record Event
    {
        /// <summary>
        /// The EventData as a JsonElement
        /// </summary>
        public JsonElement? DataElement { get; init; }

        /// <summary>
        /// The type of the Event
        /// </summary>
        public string EventType { get; init; } = string.Empty;

        /// <summary>
        /// The Event Origin
        /// </summary>
        public string Origin { get; init; } = string.Empty;

        /// <summary>
        /// The Time the Event fired
        /// </summary>
        public DateTime? TimeFired { get; init; }
    }

    /// <summary>
    /// Event with typed Data field
    /// </summary>
    /// <typeparam name="TData">The type to </typeparam>
    public record Event<TData> : Event
        where TData : class
    {
        /// <summary>Copy constructor from Base type</summary>
        public Event(Event source) : base(source)
        {
            _lazyData = new Lazy<TData?>(() => DataElement?.ToObject<TData>());
        }
        
        private Lazy<TData?> _lazyData;
        
        /// <summary>
        /// The Data of this Event deserialized as TData
        /// </summary>
        public TData? Data => _lazyData.Value;
    }
}
