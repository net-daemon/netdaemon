using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JoySoftware.HomeAssistant.Client;
using JoySoftware.HomeAssistant.Model;
using Microsoft.Extensions.Logging;
using NetDaemon.Common;
using NetDaemon.Common.Exceptions;
using NetDaemon.Mapping;

namespace NetDaemon.Daemon
{
    internal class EntityStateManager
    {
        // TODO: wo only need some methods of NetDaemonHost try to reduce the dependency
        private NetDaemonHost _netDaemonHost;

        private readonly IHassClient _hassClient;
        private readonly CancellationToken _cancellationToken;

        public EntityStateManager(IHassClient hassClient, NetDaemonHost netDaemonHost, CancellationToken cancellationToken)
        {
            _netDaemonHost = netDaemonHost;
            _hassClient = hassClient;
            _cancellationToken = cancellationToken;
        }

        internal ConcurrentDictionary<string, EntityState> InternalState = new();

        public IEnumerable<EntityState> States => InternalState.Select(n => n.Value);


        public EntityState? GetState(string entityId)
        {
            _ = entityId ??
                throw new NetDaemonArgumentNullException(nameof(entityId));

            return InternalState.TryGetValue(entityId, out var returnValue)
                ? returnValue
                : null;
        }

        public async Task RefreshAsync()
        {
            var hassStates = await _hassClient.GetAllStates(_cancellationToken).ConfigureAwait(false);

            foreach (var state in hassStates.Select(s => s.Map()))
            {
                InternalState[state.EntityId] = state with
                {
                    Area = _netDaemonHost.GetAreaForEntityId(state.EntityId)
                };
            }
        }

        public void Store(EntityState newState) => InternalState[newState.EntityId] = newState;

        private readonly string[] _supportedDomains = { "binary_sensor", "sensor", "switch" };

        public async Task<EntityState?> SetStateAndWaitForResponseAsync(string entityId, dynamic state,
            dynamic? attributes, bool waitForResponse)
        {
            _cancellationToken.ThrowIfCancellationRequested();
            _ = entityId ?? throw new NetDaemonArgumentNullException(nameof(entityId));
            _ = _hassClient ?? throw new NetDaemonNullReferenceException(nameof(_hassClient));

            if (!entityId.Contains('.', StringComparison.InvariantCultureIgnoreCase))
                throw new NetDaemonException($"Wrong entity id {entityId} provided");

            try
            {
                // Use expando object as all other methods
                if (_netDaemonHost.HasNetDaemonIntegration &&
                    _supportedDomains.Contains(entityId.Split('.')[0]))
                {
                    var service = InternalState.ContainsKey(entityId) ? "entity_update" : "entity_create";
                    // We have an integration that will help persist 
                    await _hassClient.CallService("netdaemon", service,
                        new
                        {
                            entity_id = entityId,
                            state = state.ToString(),
                            attributes
                        }, waitForResponse).ConfigureAwait(false);

                    if (waitForResponse)
                    {
                        var result = await _hassClient.GetState(entityId).ConfigureAwait(false);
                        if (result != null)
                        {
                            EntityState entityState = result.Map();
                            // InternalState[entityState.EntityId] = entityState;
                            return entityState with
                            {
                                State = state,
                                Area = _netDaemonHost.GetAreaForEntityId(entityState.EntityId)
                            };
                        }
                    }

                    return null;
                }
                else
                {
                    HassState result = await _hassClient.SetState(entityId, state.ToString(), attributes)
                        .ConfigureAwait(false);

                    if (result != null)
                    {
                        EntityState entityState = result.Map();
                        // InternalState[entityState.EntityId] = entityState;
                        return entityState with
                        {
                            State = state,
                            Area = _netDaemonHost.GetAreaForEntityId(entityState.EntityId)
                        };
                    }

                    return null;
                }
            }
            catch (Exception e)
            {
                _netDaemonHost.Logger.LogError(e, "Failed to set state for entity {entityId}", entityId);
                throw;
            }
        }

        public async Task<EntityState?> SetStateAsync(string entityId, dynamic state,
            params (string name, object val)[] attributes)
        {
             _cancellationToken.ThrowIfCancellationRequested();
            _ = entityId ?? throw new NetDaemonArgumentNullException(nameof(entityId));
            _ = _hassClient ?? throw new NetDaemonNullReferenceException(nameof(_hassClient));

            if (!entityId.Contains('.', StringComparison.InvariantCultureIgnoreCase))
                throw new NetDaemonException($"Wrong entity id {entityId} provided");

            try
            {
                // Use expando object as all other methods
                dynamic dynAttributes = attributes.ToDynamic();
                if (_netDaemonHost.HasNetDaemonIntegration)
                {
                    var service = InternalState.ContainsKey(entityId) ? "entity_update" : "entity_create";
                    // We have an integration that will help persist 
                    await _netDaemonHost.CallServiceAsync("netdaemon", service,
                        new
                        {
                            entity_id = entityId,
                            state = state.ToString(),
                            attributes = dynAttributes
                        }, true).ConfigureAwait(false);
                    return null;
                }
                else
                {
                    HassState result = await _hassClient.SetState(entityId, state.ToString(), dynAttributes)
                        .ConfigureAwait(false);

                    if (result != null)
                    {
                        EntityState entityState = result.Map();
                        entityState = entityState with
                        {
                            State = state,
                            Area = _netDaemonHost.GetAreaForEntityId(entityState.EntityId)
                        };
                        InternalState[entityState.EntityId] = entityState;
                        return entityState;
                    }

                    return null;
                }
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
