using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JoySoftware.HomeAssistant.Client;
using JoySoftware.HomeAssistant.Model;
using Microsoft.Extensions.Logging;
using NetDaemon.Common.Exceptions;

namespace NetDaemon.Daemon
{
    internal class EntityStateManager
    {
        // TODO: we only need some methods of NetDaemonHost try to reduce the dependency
        private readonly NetDaemonHost _netDaemonHost;

        public EntityStateManager(NetDaemonHost netDaemonHost)
        {
            _netDaemonHost = netDaemonHost;
        }

        internal ConcurrentDictionary<string, HassState> InternalState = new();

        public IEnumerable<HassState> States => InternalState.Values;

        public HassState? GetState(string entityId)
        {
            return InternalState.TryGetValue(entityId, out var returnValue) ? returnValue : null;
        }

        public async Task RefreshAsync()
        {
            var hassStates = await _netDaemonHost.Client.GetAllStates(_netDaemonHost.CancelToken).ConfigureAwait(false);

            foreach (var state in hassStates)
            {
                InternalState[state.EntityId] = state;
            }
        }

        public void Store(HassState newState) => InternalState[newState.EntityId] = newState;

        private readonly string[] _supportedDomains = {"binary_sensor", "sensor", "switch"};
        
        public async Task<HassState?> SetStateAndWaitForResponseAsync(string entityId, string? state,
            object? attributes, bool waitForResponse)
        {
            if (!entityId.Contains('.', StringComparison.InvariantCultureIgnoreCase))
                throw new NetDaemonException($"Wrong entity id {entityId} provided");
            try
            {
                HassState? result;
                if (_netDaemonHost.HasNetDaemonIntegration &&
                    _supportedDomains.Contains(entityId.Split('.')[0]))
                {
                    var service = InternalState.ContainsKey(entityId) ? "entity_update" : "entity_create";
                    // We have an integration that will help persist 
                    await _netDaemonHost.Client.CallService("netdaemon", service,
                            new
                            {
                                entity_id = entityId,
                                state,
                                attributes
                            }, waitForResponse)
                        .ConfigureAwait(false);

                    if (!waitForResponse) return null;

                    result = await _netDaemonHost.Client.GetState(entityId).ConfigureAwait(false);
                }
                else
                {
                    result = await _netDaemonHost.Client.SetState(entityId, state ?? "", attributes)
                        .ConfigureAwait(false);
                }

                if (result == null) return null;

                InternalState[result.EntityId] = result with {State = state};

                return result;
            }
            catch (Exception e)
            {
                _netDaemonHost.Logger.LogError(e, "Failed to set state for entity {entityId}", entityId);
                throw;
            }
        }

        public void Clear() => InternalState.Clear();
    }
}