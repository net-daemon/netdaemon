using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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
    public abstract class NetDaemonApp : INetDaemonApp, INetDaemonBase
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

        // This is declared as static since it will contain state shared globally
        static ConcurrentDictionary<string, object> _global = new ConcurrentDictionary<string, object>();

        /// <summary>
        ///    Dependencies on other applications that will be initialized before this app
        /// </summary>
        public IEnumerable<string> Dependencies { get; set; } = new List<string>();

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
        public bool IsEnabled { get; set; }

        /// <inheritdoc/>
        public ConcurrentDictionary<string, object> Global => _global;

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
            return _daemon!.Entities(this, func);
        }

        /// <inheritdoc/>
        public IEntity Entities(IEnumerable<string> entityIds)
        {
            _ = _daemon as INetDaemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
            return _daemon!.Entities(this, entityIds);
        }

        /// <inheritdoc/>
        public IEntity Entity(params string[] entityId)
        {
            _ = _daemon as INetDaemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
            return _daemon!.Entity(this, entityId);
        }

        /// <inheritdoc/>
        public IFluentEvent Event(params string[] eventParams) => _daemon?.Event(this, eventParams) ??
            throw new NullReferenceException($"{nameof(_daemon)} cant be null!");

        /// <inheritdoc/>
        public IFluentEvent Events(Func<FluentEventProperty, bool> func) => _daemon?.Events(this, func) ??
            throw new NullReferenceException($"{nameof(_daemon)} cant be null!");

        /// <inheritdoc/>
        public IFluentEvent Events(IEnumerable<string> eventParams) => _daemon?.Events(this, eventParams) ??
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
        public void ListenEvent(string ev, Func<string, dynamic?, Task> action) => _daemon?.ListenEvent(ev, action);

        /// <inheritdoc/>
        public void ListenEvent(Func<FluentEventProperty, bool> funcSelector, Func<string, dynamic, Task> func) =>
                _daemon?.ListenEvent(funcSelector, func);

        /// <inheritdoc/>
        public void ListenServiceCall(string domain, string service, Func<dynamic?, Task> action) =>
                _daemon?.ListenServiceCall(domain, service, action);

        /// <inheritdoc/>
        public string? ListenState(string pattern, Func<string, EntityState?, EntityState?, Task> action) => _daemon?.ListenState(pattern, action);

        /// <inheritdoc/>
        public void CancelListenState(string id) => _daemon?.CancelListenState(id);


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
        public NetDaemonApp? GetApp(string appInstanceId)
        {
            _ = _daemon as INetDaemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
            return _daemon!.GetApp(appInstanceId);
        }

        /// <inheritdoc/>
        public void Log(string message) => Log(LogLevel.Information, message);

        /// <inheritdoc/>
        public void LogWarning(string message) => Log(LogLevel.Warning, message);

        /// <inheritdoc/>
        public void LogError(string message) => Log(LogLevel.Error, message);

        /// <inheritdoc/>
        public void LogTrace(string message) => Log(LogLevel.Trace, message);

        /// <inheritdoc/>
        public void LogDebug(string message) => Log(LogLevel.Debug, message);

        /// <inheritdoc/>
        public void Log(Exception exception, string message) => Log(LogLevel.Information, exception, message);
        /// <inheritdoc/>
        public void LogWarning(Exception exception, string message) => Log(LogLevel.Warning, exception, message);
        /// <inheritdoc/>
        public void LogError(Exception exception, string message) => Log(LogLevel.Error, exception, message);
        /// <inheritdoc/>
        public void LogDebug(Exception exception, string message) => Log(LogLevel.Debug, exception, message);
        /// <inheritdoc/>
        public void LogTrace(Exception exception, string message) => Log(LogLevel.Trace, exception, message);

        /// <inheritdoc/>
        public void Log(LogLevel level, string message, params object[] param)
        {
            if (param is object && param.Length > 0)
            {
                var result = param.Prepend(Id).ToArray();
                Logger.Log(level, $"  {{Id}}: {message}", result);
            }
            else
            {
                Logger.Log(level, $"  {{Id}}: {message}", new object[] { Id ?? "" });
            }
        }

        /// <inheritdoc/>
        public void Log(string message, params object[] param) => Log(LogLevel.Information, message, param);

        /// <inheritdoc/>
        public void LogWarning(string message, params object[] param) => Log(LogLevel.Warning, message, param);

        /// <inheritdoc/>
        public void LogError(string message, params object[] param) => Log(LogLevel.Error, message, param);

        /// <inheritdoc/>
        public void LogDebug(string message, params object[] param) => Log(LogLevel.Debug, message, param);

        /// <inheritdoc/>
        public void LogTrace(string message, params object[] param) => Log(LogLevel.Trace, message, param);


        /// <inheritdoc/>
        public void Log(LogLevel level, Exception exception, string message, params object[] param)
        {
            if (param is object && param.Length > 0)
            {
                var result = param.Prepend(Id).ToArray();
                Logger.Log(level, exception, $"  {{Id}}: {message}", result);
            }
            else
            {
                Logger.Log(level, exception, $"  {{Id}}: {message}", new object[] { Id ?? "" });
            }
        }

        /// <inheritdoc/>
        public void Log(Exception exception, string message, params object[] param) => Log(LogLevel.Information, exception, message, param);

        /// <inheritdoc/>
        public void LogWarning(Exception exception, string message, params object[] param) => Log(LogLevel.Warning, exception, message, param);

        /// <inheritdoc/>
        public void LogError(Exception exception, string message, params object[] param) => Log(LogLevel.Error, exception, message, param);

        /// <inheritdoc/>
        public void LogDebug(Exception exception, string message, params object[] param) => Log(LogLevel.Debug, exception, message, param);

        /// <inheritdoc/>
        public void LogTrace(Exception exception, string message, params object[] param) => Log(LogLevel.Trace, exception, message, param);

        /// <inheritdoc/>
        public IMediaPlayer MediaPlayer(params string[] entityIds)
        {
            _ = _daemon as INetDaemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
            return _daemon!.MediaPlayer(this, entityIds);
        }

        /// <inheritdoc/>
        public IMediaPlayer MediaPlayers(IEnumerable<string> entityIds)
        {
            _ = _daemon as INetDaemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
            return _daemon!.MediaPlayers(this, entityIds);
        }

        /// <inheritdoc/>
        public IMediaPlayer MediaPlayers(Func<IEntityProperties, bool> func)
        {
            _ = _daemon as INetDaemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
            return _daemon!.MediaPlayers(this, func);
        }

        /// <inheritdoc/>
        public ICamera Camera(params string[] entityIds)
        {
            _ = _daemon as INetDaemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
            return _daemon!.Camera(this, entityIds);
        }

        /// <inheritdoc/>
        public ICamera Cameras(IEnumerable<string> entityIds)
        {
            _ = _daemon as INetDaemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
            return _daemon!.Cameras(this, entityIds);
        }

        /// <inheritdoc/>
        public ICamera Cameras(Func<IEntityProperties, bool> func)
        {
            _ = _daemon as INetDaemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
            return _daemon!.Cameras(this, func);
        }

        /// <inheritdoc/>
        public IScript RunScript(params string[] entityIds)
        {
            _ = _daemon as INetDaemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
            return _daemon!.RunScript(this, entityIds);
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

            var appInfo = _daemon!.State
                                  .Where(s => s.EntityId == $"switch.netdaemon_{Id?.ToSafeHomeAssistantEntityId()}")
                                  .FirstOrDefault();

            var appState = appInfo?.State as string;
            if (appState == null || (appState != "on" && appState != "off"))
            {
                IsEnabled = true;
                await _daemon.SetState($"switch.netdaemon_{Id?.ToSafeHomeAssistantEntityId()}", "on");

                return;
            }
            IsEnabled = appState == "on";
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

        #endregion IDisposable Support

        /// <inheritdoc/>
        public IFluentInputSelect InputSelect(params string[] inputSelectParams)
        {
            _ = _daemon as INetDaemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
            return _daemon!.InputSelect(this, inputSelectParams);
        }

        /// <inheritdoc/>
        public IFluentInputSelect InputSelects(IEnumerable<string> inputSelectParams)
        {
            _ = _daemon as INetDaemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
            return _daemon!.InputSelects(this, inputSelectParams);
        }

        /// <inheritdoc/>
        public IFluentInputSelect InputSelects(Func<IEntityProperties, bool> func)
        {
            _ = _daemon as INetDaemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
            return _daemon!.InputSelects(this, func);
        }

        /// <inheritdoc/>
        public IDelayResult DelayUntilStateChange(IEnumerable<string> entityIds, object? to = null, object? from = null, bool allChanges = false)
        {
            _ = _daemon as INetDaemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
            return _daemon!.DelayUntilStateChange(entityIds, to, from, allChanges);
        }

        /// <inheritdoc/>
        public IDelayResult DelayUntilStateChange(string entityId, object? to = null, object? from = null, bool allChanges = false)
        {
            _ = _daemon as INetDaemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
            return _daemon!.DelayUntilStateChange(entityId, to, from, allChanges);
        }

        /// <inheritdoc/>
        public IDelayResult DelayUntilStateChange(IEnumerable<string> entityIds, Func<EntityState?, EntityState?, bool> stateFunc)
        {
            _ = _daemon as INetDaemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
            return _daemon!.DelayUntilStateChange(entityIds, stateFunc);
        }


        /// <summary>
        ///     Implements the IEqualit.Equals method
        /// </summary>
        /// <param name="other">The instance to compare</param>
        public bool Equals([AllowNull] INetDaemonApp other)
        {
            if (other is object && other.Id is object && this.Id is object && this.Id == other.Id)
                return true;

            return false;
        }
    }
}