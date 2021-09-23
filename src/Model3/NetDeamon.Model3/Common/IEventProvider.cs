using System;

namespace NetDaemon.Model3.Common
{
    /// <summary>
    /// Provides methds for consuming events from Home Assistant
    /// </summary>
    public interface IEventProvider
    {
        /// <summary>
        /// Gets an Observable for a specific eventType and retrieves the EventData deserialized in a specific type
        /// </summary>
        /// <param name="eventType">The type of event to retrieve</param>
        /// <typeparam name="T">The type to deserialize the event data into</typeparam>
        /// <returns>An IObservable of the filtered Event stream</returns>
        IObservable<T> GetEventDataOfType<T>(string eventType) where T : class;
    }
}