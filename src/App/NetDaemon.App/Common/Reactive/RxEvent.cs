namespace NetDaemon.Common.Reactive
{
    /// <summary>
    ///     Represent an event from event stream
    /// </summary>
    public class RxEvent
    {
        private readonly string? _domain;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="eventName">Event</param>
        /// <param name="domain">Domain</param>
        /// <param name="data">Data</param>
        public RxEvent(string eventName, string? domain, dynamic? data)
        {
            Event = eventName;
            _domain = domain;
            Data = data;
        }

        /// <summary>
        ///     Data from event
        /// </summary>
        public dynamic? Data { get; }

        /// <summary>
        ///     Domain (call service event)
        /// </summary>
        public dynamic? Domain => _domain;

        /// <summary>
        ///     The event being sent
        /// </summary>
        public string Event { get; }
    }
}