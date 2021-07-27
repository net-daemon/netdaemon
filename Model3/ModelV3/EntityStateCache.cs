using System.Collections.Concurrent;
using JoySoftware.HomeAssistant.Model;

namespace Model3.ModelV3
{
    public class EntityStateCache
    {
        private ConcurrentDictionary<string, HassState> _latestStates;

        public void Store(HassState newState) => _latestStates[newState.EntityId] = newState;

        public HassState GetState(string entityId) => _latestStates[entityId];
    }
}