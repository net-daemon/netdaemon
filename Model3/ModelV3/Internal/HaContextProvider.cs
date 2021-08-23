﻿using System;
using System.Reactive;
using System.Reactive.Linq;
using JoySoftware.HomeAssistant.Client;
using JoySoftware.HomeAssistant.Model;
using Model3.ModelV3;
using NetDaemon.Daemon;

namespace NetDaemon.Common.ModelV3
{
    internal class HaContextProvider : IHaContext
    {
        private readonly INetDaemonHost _netDaemonHost;
        private readonly IObservable<HassEvent> _hassEventObservable;
        private readonly EntityStateCache _entityStateCache;

        public HaContextProvider(INetDaemonHost netDaemonHost,
            IObservable<HassEvent> hassEventObservable,
            EntityStateCache entityStateCache)
        {
            _netDaemonHost = netDaemonHost;
            _hassEventObservable = hassEventObservable;
            _entityStateCache = entityStateCache;

            var stateChanges = _hassEventObservable
                .Select(e => e.Data as HassStateChangedEventData)
                .Where(e => e != null)
                .Select(e => e!);

            stateChanges.Subscribe(s =>
            {
                var state = s.NewState;
                if (state != null) _entityStateCache.Store(state);
            });

            StateAllChanges = stateChanges.Select(e => e.Map(this));
        }

        public EntityState? GetState(string entityId)
        {
            var hassState = _entityStateCache.GetState(entityId);

            return HassObjectMapper.Map(hassState);
        }

        public void CallService(string domain, string service, object? data, bool waitForResponse = false)
        {
            _netDaemonHost.CallService(domain, service, data, waitForResponse);
        }

        //public IRxEvent EventChanges { get; }
        public IObservable<StateChange> StateAllChanges { get; }

        public IObservable<StateChange> StateChanges => StateAllChanges.Where(e => e.New?.State != e.Old?.State);
    }
}