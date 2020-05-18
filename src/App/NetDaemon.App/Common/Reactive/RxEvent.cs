namespace JoySoftware.HomeAssistant.NetDaemon.Common.Reactive
{
    /// <summary>
    ///     Represent an event from eventstream
    /// </summary>
    public struct RxEvent
    {
        private readonly dynamic? _data;
        private readonly string? _domain;
        private readonly string _eventName;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="eventName">Event</param>
        /// <param name="domain">Domain</param>
        /// <param name="data">Data</param>
        public RxEvent(string eventName, string? domain, dynamic? data)
        {
            _eventName = eventName;
            _domain = domain;
            _data = data;
        }

        /// <summary>
        ///     Data from event
        /// </summary>
        public dynamic? Data => _data;

        /// <summary>
        ///     Domain (call service event)
        /// </summary>
        public dynamic? Domain => _domain;

        /// <summary>
        ///     The event being sent
        /// </summary>
        public string Event => _eventName;
    }
}