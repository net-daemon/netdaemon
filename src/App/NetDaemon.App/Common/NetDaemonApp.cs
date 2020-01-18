using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace JoySoftware.HomeAssistant.NetDaemon.Common
{
    public class NetDaemonApp : INetDaemonApp
    {
        private INetDaemon? _daemon;
#pragma warning disable 4014
        public virtual Task StartUpAsync(INetDaemon daemon)
        {
            _daemon = daemon;
            Logger = daemon.Logger;

            return Task.CompletedTask;
        }

        public virtual Task InitializeAsync()
        {
            // Do nothing as default
            return Task.CompletedTask;
        }

        public void ListenState(string pattern, Func<string, EntityState?, EntityState?, Task> action)
        {
            _daemon?.ListenState(pattern, action);
        }

        public async Task TurnOnAsync(string entityId, params (string name, object val)[] attributeNameValuePair)
        {
            if (_daemon != null) await _daemon.TurnOnAsync(entityId, attributeNameValuePair);
        }

        public ILogger? Logger { get; private set; }

        public void Log(string message, LogLevel level = LogLevel.Information)
        {
            Logger.Log(level, message);
        }

        public void Log(string message, Exception exception, LogLevel level = LogLevel.Information)
        {
            Logger.Log(level, exception, message);
        }

        //public IAction Action => _daemon.Action;

    }
}