using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace NetDaemon.Common
{
    /// <summary>
    ///     The interface that interacts with the daemon main logic
    /// </summary>
    public interface INetDaemonCommon
    {
        /// <summary>
        ///     Logger to use
        /// </summary>
        ILogger Logger { get; }

        /// <summary>
        ///     All current states for all known entities
        /// </summary>
        /// <remarks>
        ///     All states are read and cached at startup. Every state change updates the
        ///     states. There can be a small risk that the state is not updated
        ///     exactly when it happens but it should be fine. The SetState function
        ///     updates the state before sending.
        /// </remarks>
        [SuppressMessage("", "CA1721")]
        IEnumerable<EntityState> State { get; }

        /// <summary>
        ///     Loads persistent data from unique id
        /// </summary>
        /// <param name="id">Unique Id of the data</param>
        /// <returns>The data persistent or null if not exists</returns>
        Task<T?> GetDataAsync<T>(string id) where T : class;

        /// <summary>
        ///     Gets current state for the entity
        /// </summary>
        /// <param name="entityId">The unique id of the entity</param>
        /// <returns></returns>
        EntityState? GetState(string entityId);

        /// <summary>
        ///     Saves any data with unique id, data have to be json serializable
        /// </summary>
        /// <param name="id">Unique id for all apps</param>
        /// <param name="data">Dynamic data being saved</param>
        Task SaveDataAsync<T>(string id, T data);

        /// <summary>
        ///     Sends a custom event
        /// </summary>
        /// <param name="eventId">Any identity of the event</param>
        /// <param name="data">Any data sent with the event</param>
        Task<bool> SendEvent(string eventId, dynamic? data = null);

        /// <summary>
        ///     Set entity state
        /// </summary>
        /// <param name="entityId">The unique id of the entity</param>
        /// <param name="state">The state being set</param>
        /// <param name="attributes">Name/Value pair of the attribute</param>
        Task<EntityState?> SetStateAsync(string entityId, dynamic state, params (string name, object val)[] attributes);

        /// <summary>
        ///     Use text-to-speech to speak a message
        /// </summary>
        /// <param name="entityId">Unique id of the media player the speech should play</param>
        /// <param name="message">The message that will be spoken</param>
        void Speak(string entityId, string message);
    }
}