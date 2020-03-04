using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("NetDaemon.Daemon.Tests")]

namespace JoySoftware.HomeAssistant.NetDaemon.Common
{
    /// <summary>
    ///     Base class för all NetDaemon apps
    /// </summary>
    public class NetDaemonApp : INetDaemonApp, INetDaemon, IDisposable
    {
        private INetDaemon? _daemon;

        private Task? _lazyStoreStateTask;

        // To handle state saves max once at a time, internal due to tests
        private readonly Channel<bool> _lazyStoreStateQueue =
               Channel.CreateBounded<bool>(1);

        internal Channel<bool> InternalLazyStoreStateQueue => _lazyStoreStateQueue;

        private CancellationTokenSource _cancelSource = new CancellationTokenSource();

        private FluentExpandoObject? _storageObject;
        internal FluentExpandoObject? InternalStorageObject { get { return _storageObject; } set { _storageObject = value; } }

        /// <inheritdoc/>
        public ILogger? Logger { get; set; }

        /// <inheritdoc/>
        public IScheduler Scheduler => _daemon?.Scheduler ??
            throw new NullReferenceException($"{nameof(_daemon)} cant be null!");

        /// <inheritdoc/>
        public IEnumerable<EntityState> State => _daemon?.State ??
            throw new NullReferenceException($"{nameof(_daemon)} cant be null!");

        /// <inheritdoc/>
        public dynamic Storage => _storageObject ?? throw new NullReferenceException($"{nameof(_storageObject)} cant be null");

        /// <inheritdoc/>
        public string? Id { get; set; }

        /// <inheritdoc/>
        public Task CallService(string domain, string service, dynamic? data = null, bool waitForResponse = false)
        {
            _ = _daemon as INetDaemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
            return _daemon!.CallService(domain, service, data, waitForResponse);
        }

        /// <inheritdoc/>
        public IEntity Entities(Func<IEntityProperties, bool> func)
        {
            _ = _daemon as INetDaemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
            return _daemon!.Entities(func);
        }

        /// <inheritdoc/>
        public IEntity Entities(IEnumerable<string> entityIds)
        {
            _ = _daemon as INetDaemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
            return _daemon!.Entities(entityIds);
        }

        /// <inheritdoc/>
        public IEntity Entity(params string[] entityId)
        {
            _ = _daemon as INetDaemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
            return _daemon!.Entity(entityId);
        }

        /// <inheritdoc/>
        public IFluentEvent Event(params string[] eventParams) => _daemon?.Event(eventParams) ??
            throw new NullReferenceException($"{nameof(_daemon)} cant be null!");

        /// <inheritdoc/>
        public IFluentEvent Events(Func<FluentEventProperty, bool> func) => _daemon?.Events(func) ??
            throw new NullReferenceException($"{nameof(_daemon)} cant be null!");

        /// <inheritdoc/>
        public IFluentEvent Events(IEnumerable<string> eventParams) => _daemon?.Events(eventParams) ??
            throw new NullReferenceException($"{nameof(_daemon)} cant be null!");

        /// <inheritdoc/>
        public EntityState? GetState(string entityId) => _daemon?.GetState(entityId);

        /// <inheritdoc/>
        public virtual Task InitializeAsync()
        {
            // Do nothing as default
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public ILight Light(params string[] entity)
        {
            _ = _daemon as INetDaemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
            return _daemon!.Light(entity);
        }

        /// <inheritdoc/>
        public void ListenEvent(string ev, Func<string, dynamic?, Task> action) => _daemon?.ListenEvent(ev, action);

        /// <inheritdoc/>
        public void ListenEvent(Func<FluentEventProperty, bool> funcSelector, Func<string, dynamic, Task> func) =>
                _daemon?.ListenEvent(funcSelector, func);

        /// <inheritdoc/>
        public void ListenServiceCall(string domain, string service, Func<dynamic?, Task> action) =>
                _daemon?.ListenServiceCall(domain, service, action);

        /// <inheritdoc/>
        public void ListenState(string pattern, Func<string, EntityState?, EntityState?, Task> action) => _daemon?.ListenState(pattern, action);

        /// <inheritdoc/>
        public async ValueTask<T> GetDataAsync<T>(string id)
        {
            _ = _daemon as INetDaemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
            return await _daemon!.GetDataAsync<T>(id);
        }

        /// <inheritdoc/>
        public Task SaveDataAsync<T>(string id, T data)
        {
            _ = _daemon as INetDaemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
            return _daemon!.SaveDataAsync<T>(id, data);
        }

        /// <inheritdoc/>
        public void Log(string message, LogLevel level = LogLevel.Information) => Logger.Log(level, message);

        /// <inheritdoc/>
        public void Log(string message, Exception exception, LogLevel level = LogLevel.Information) => Logger.Log(level, exception, message);

        /// <inheritdoc/>
        public IMediaPlayer MediaPlayer(params string[] entityIds)
        {
            _ = _daemon as INetDaemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
            return _daemon!.MediaPlayer(entityIds);
        }

        /// <inheritdoc/>
        public IMediaPlayer MediaPlayers(IEnumerable<string> entityIds)
        {
            _ = _daemon as INetDaemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
            return _daemon!.MediaPlayers(entityIds);
        }

        /// <inheritdoc/>
        public IMediaPlayer MediaPlayers(Func<IEntityProperties, bool> func)
        {
            _ = _daemon as INetDaemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
            return _daemon!.MediaPlayers(func);
        }

        /// <inheritdoc/>
        public IScript RunScript(params string[] entityIds)
        {
            _ = _daemon as INetDaemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
            return _daemon!.RunScript(entityIds);
        }

        /// <inheritdoc/>
        public async Task<bool> SendEvent(string eventId, dynamic? data = null)
        {
            _ = _daemon as INetDaemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
            return await _daemon!.SendEvent(eventId, data);
        }

        /// <inheritdoc/>
        public async Task<EntityState?> SetState(string entityId, dynamic state, params (string name, object val)[] attributes)
        {
            _ = _daemon as INetDaemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
            return await _daemon!.SetState(entityId, state, attributes);
        }

        /// <inheritdoc/>
        public void Speak(string entityId, string message)
        {
            _ = _daemon as INetDaemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
            _daemon!.Speak(entityId, message);
        }

        /// <inheritdoc/>
        public virtual Task StartUpAsync(INetDaemon daemon)
        {
            _daemon = daemon;
            _lazyStoreStateTask = Task.Run(async () => await HandleLazyStorage());
            _storageObject = new FluentExpandoObject(false, true, daemon: this);
            Logger = daemon.Logger;

            return Task.CompletedTask;
        }

        private string GetUniqueIdForStorage() => $"{this.GetType().Name}_{Id}".ToLowerInvariant();

        private async Task HandleLazyStorage()
        {
            _ = _storageObject as FluentExpandoObject ??
                throw new NullReferenceException($"{nameof(_storageObject)} cant be null!");
            _ = _daemon as INetDaemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");

            while (!_cancelSource.IsCancellationRequested)
            {
                try
                {
                    // Dont care about the result, just that it is time to store state
                    _ = await _lazyStoreStateQueue.Reader.ReadAsync(_cancelSource.Token);

                    await _daemon!.SaveDataAsync<IDictionary<string, object>>(GetUniqueIdForStorage(), (IDictionary<string, object>)Storage)
                            .ConfigureAwait(false);
                }
                catch { }   // Ignore errors in thread
            }
        }

        /// <inheritdoc/>
        public void SaveAppState()
        {
            // Intentionally ignores full queue since we know
            // a state change already is in progress wich means
            // this state will be saved
            var x = _lazyStoreStateQueue.Writer.TryWrite(true);
        }

        /// <inheritdoc/>
        public async Task RestoreAppStateAsync()
        {
            _ = _daemon as INetDaemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");

            var obj = await _daemon!.GetDataAsync<IDictionary<string, object>>(GetUniqueIdForStorage());

            if (obj != null)
            {
                var expStore = (FluentExpandoObject)Storage;
                expStore.CopyFrom(obj);
            }
        }

        /// <inheritdoc/>
        public Task ToggleAsync(string entityId, params (string name, object val)[] attributes)
        {
            _ = _daemon as INetDaemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
            return _daemon!.ToggleAsync(entityId, attributes);
        }

        /// <inheritdoc/>
        public Task TurnOffAsync(string entityId, params (string name, object val)[] attributes)
        {
            _ = _daemon as INetDaemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
            return _daemon!.TurnOffAsync(entityId, attributes);
        }

        /// <inheritdoc/>
        public Task TurnOnAsync(string entityId, params (string name, object val)[] attributes)
        {
            _ = _daemon as INetDaemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
            return _daemon!.TurnOnAsync(entityId, attributes);
        }

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        /// <inheritdoc/>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _cancelSource.Cancel();
                }

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        /// <inheritdoc/>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }

        /// <inheritdoc/>
        public IFluentInputSelect InputSelect(params string[] inputSelectParams)
        {
            _ = _daemon as INetDaemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
            return _daemon!.InputSelect(inputSelectParams);
        }

        /// <inheritdoc/>
        public IFluentInputSelect InputSelects(IEnumerable<string> inputSelectParams)
        {
            _ = _daemon as INetDaemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
            return _daemon!.InputSelects(inputSelectParams);
        }

        /// <inheritdoc/>
        public IFluentInputSelect InputSelects(Func<IEntityProperties, bool> func)
        {
            _ = _daemon as INetDaemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
            return _daemon!.InputSelects(func);
        }

        #endregion IDisposable Support
    }
}