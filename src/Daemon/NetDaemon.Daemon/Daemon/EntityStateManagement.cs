﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using JoySoftware.HomeAssistant.Model;
using Microsoft.Extensions.Logging;
using NetDaemon.Common;
using NetDaemon.Common.Exceptions;
using NetDaemon.Mapping;

namespace NetDaemon.Daemon
{
    internal class EntityStateManager
    {
        private readonly NetDaemonHost _netDaemonHost;

        public EntityStateManager(NetDaemonHost netDaemonHost)
        {
            _netDaemonHost = netDaemonHost;
        }

        internal ConcurrentDictionary<string, EntityState> InternalState = new();

        public IEnumerable<EntityState> States => InternalState.Values;

        public EntityState? GetState(string entityId)
        {
            return InternalState.TryGetValue(entityId, out var returnValue) ? returnValue : null;
        }

        public async Task RefreshAsync()
        {
            var hassStates = await _netDaemonHost.Client.GetAllStates(_netDaemonHost.CancelToken).ConfigureAwait(false);

            foreach (var state in hassStates)
            {
                InternalState[state.EntityId] = state.MapWithArea(_netDaemonHost.GetAreaForEntityId(state.EntityId));
            }
        }

        public void Store(EntityState newState) => InternalState[newState.EntityId] = newState;

        private readonly string[] _supportedDomains = { "binary_sensor", "sensor", "switch", "climate" };

        public async Task<EntityState?> SetStateAndWaitForResponseAsync(string entityId, string state,
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
                            }, null, waitForResponse)
                        .ConfigureAwait(false);

                    if (!waitForResponse) return null;

                    result = await _netDaemonHost.Client.GetState(entityId).ConfigureAwait(false);
                }
                else
                {
                    result = await _netDaemonHost.Client.SetState(entityId, state, attributes)
                        .ConfigureAwait(false);
                }

                if (result == null) return null;

                EntityState entityState = result.MapWithArea(_netDaemonHost.GetAreaForEntityId(result.EntityId));
                InternalState[entityState.EntityId] = entityState with { State = state };

                return entityState;
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