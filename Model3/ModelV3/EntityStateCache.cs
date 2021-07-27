using System.Collections.Concurrent;
using System.Threading.Tasks;
using JoySoftware.HomeAssistant.Client;
using JoySoftware.HomeAssistant.Model;

namespace Model3.ModelV3
{
    public class EntityStateCache
    {
        
        public async Task RefreshAsync(IHassClient client)
        {
            var hassStates = await client.GetAllStates().ConfigureAwait(false);

            foreach (var state in hassStates)
            {
                _latestStates[state.EntityId] = state;
            }
        }

        
        private ConcurrentDictionary<string, HassState> _latestStates = new ();

        public void Store(HassState newState) => _latestStates[newState.EntityId] = newState;

        public HassState? GetState(string entityId) => _latestStates.TryGetValue(entityId, out var state) ? state : null;
    }
}