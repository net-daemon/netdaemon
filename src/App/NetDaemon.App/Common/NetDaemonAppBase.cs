using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace JoySoftware.HomeAssistant.NetDaemon.Common
{
    /// <summary>
    ///     Base class for all NetDaemon App types
    /// </summary>
    public abstract class NetDaemonAppBase : INetDaemonAppBase
    {
        /// <summary>
        ///     The NetDaemonHost instance
        /// </summary>
        protected INetDaemon? _daemon;

        // This is declared as static since it will contain state shared globally
        private static ConcurrentDictionary<string, object> _global = new ConcurrentDictionary<string, object>();

        // To handle state saves max once at a time, internal due to tests
        private readonly Channel<bool> _lazyStoreStateQueue =
               Channel.CreateBounded<bool>(1);

        private CancellationTokenSource _cancelSource = new CancellationTokenSource();
        private Task? _lazyStoreStateTask;
        private FluentExpandoObject? _storageObject;

        /// <summary>
        ///    Dependencies on other applications that will be initialized before this app
        /// </summary>
        public IEnumerable<string> Dependencies { get; set; } = new List<string>();

        /// <inheritdoc/>
        public ConcurrentDictionary<string, object> Global => _global;

        /// <inheritdoc/>
        public IHttpHandler Http
        {
            get
            {
                _ = _daemon as INetDaemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
                return _daemon!.Http;
            }
        }

        /// <inheritdoc/>
        public string? Id { get; set; }

        /// <inheritdoc/>
        public bool IsEnabled { get; set; }

        /// <inheritdoc/>
        public ILogger? Logger { get; set; }

        /// <inheritdoc/>
        public dynamic Storage => _storageObject ?? throw new NullReferenceException($"{nameof(_storageObject)} cant be null");

        internal Channel<bool> InternalLazyStoreStateQueue => _lazyStoreStateQueue;
        internal FluentExpandoObject? InternalStorageObject { get { return _storageObject; } set { _storageObject = value; } }
        /// <summary>
        ///     Implements the IEqualit.Equals method
        /// </summary>
        /// <param name="other">The instance to compare</param>
        public bool Equals([AllowNull] INetDaemonAppBase other)
        {
            if (other is object && other.Id is object && this.Id is object && this.Id == other.Id)
                return true;

            return false;
        }

        /// <summary>
        ///     Initializes the app, is virtual and overridden
        /// </summary>
        public virtual void Initialize()
        {
            // do nothing
        }

        /// <summary>
        ///     Initializes the app async, is virtual and overridden
        /// </summary>
        public virtual Task InitializeAsync()
        {
            // Do nothing
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public void Speak(string entityId, string message)
        {
            _ = _daemon as INetDaemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
            _daemon!.Speak(entityId, message);
        }

        /// <inheritdoc/>
        public async Task RestoreAppStateAsync()
        {
            _ = _daemon as INetDaemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");

            var obj = await _daemon!.GetDataAsync<IDictionary<string, object>>(GetUniqueIdForStorage()).ConfigureAwait(false);

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
                await _daemon.SetStateAsync($"switch.netdaemon_{Id?.ToSafeHomeAssistantEntityId()}", "on").ConfigureAwait(false);

                return;
            }
            IsEnabled = appState == "on";
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
        public virtual Task StartUpAsync(INetDaemon daemon)
        {
            _daemon = daemon;
            _lazyStoreStateTask = Task.Run(async () => await HandleLazyStorage().ConfigureAwait(false));
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
        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        // This code added to correctly implement the disposable pattern.
        /// <inheritdoc/>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }

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
        #endregion IDisposable Support


        #region -- Logger helpers --
        /// <inheritdoc/>
        public void Log(string message) => Log(LogLevel.Information, message);

        /// <inheritdoc/>
        public void Log(Exception exception, string message) => Log(LogLevel.Information, exception, message);

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
        public void LogDebug(string message) => Log(LogLevel.Debug, message);

        /// <inheritdoc/>
        public void LogDebug(Exception exception, string message) => Log(LogLevel.Debug, exception, message);

        /// <inheritdoc/>
        public void LogDebug(string message, params object[] param) => Log(LogLevel.Debug, message, param);

        /// <inheritdoc/>
        public void LogDebug(Exception exception, string message, params object[] param) => Log(LogLevel.Debug, exception, message, param);

        /// <inheritdoc/>
        public void LogError(string message) => Log(LogLevel.Error, message);

        /// <inheritdoc/>
        public void LogError(Exception exception, string message) => Log(LogLevel.Error, exception, message);

        /// <inheritdoc/>
        public void LogError(string message, params object[] param) => Log(LogLevel.Error, message, param);

        /// <inheritdoc/>
        public void LogError(Exception exception, string message, params object[] param) => Log(LogLevel.Error, exception, message, param);

        /// <inheritdoc/>
        public void LogTrace(string message) => Log(LogLevel.Trace, message);

        /// <inheritdoc/>
        public void LogTrace(Exception exception, string message) => Log(LogLevel.Trace, exception, message);

        /// <inheritdoc/>
        public void LogTrace(string message, params object[] param) => Log(LogLevel.Trace, message, param);

        /// <inheritdoc/>
        public void LogTrace(Exception exception, string message, params object[] param) => Log(LogLevel.Trace, exception, message, param);

        /// <inheritdoc/>
        public void LogWarning(string message) => Log(LogLevel.Warning, message);

        /// <inheritdoc/>
        public void LogWarning(Exception exception, string message) => Log(LogLevel.Warning, exception, message);

        /// <inheritdoc/>
        public void LogWarning(string message, params object[] param) => Log(LogLevel.Warning, message, param);

        /// <inheritdoc/>
        public void LogWarning(Exception exception, string message, params object[] param) => Log(LogLevel.Warning, exception, message, param);


        #endregion
    }
}
