using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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
        private Task? _manageRuntimeInformationUpdatesTask;

        /// <summary>
        ///     Registered callbacks for service calls
        /// </summary>
        private readonly List<(string, string, Func<dynamic?, Task>)> _daemonCallBacksForServiceCalls
            = new List<(string, string, Func<dynamic?, Task>)>();

        /// <summary>
        ///     All actions being performed for service call events
        /// </summary>
        public List<(string, string, Func<dynamic?, Task>)> DaemonCallBacksForServiceCalls => _daemonCallBacksForServiceCalls;

        /// <summary>
        ///     Next scheduled time
        /// </summary>
        protected DateTime? NextScheduledEvent { get; set; } = null;

        // This is declared as static since it will contain state shared globally
        private static ConcurrentDictionary<string, object> _global = new ConcurrentDictionary<string, object>();

        private readonly ConcurrentDictionary<string, object> _attributes = new ConcurrentDictionary<string, object>();

        // To handle state saves max once at a time, internal due to tests
        private readonly Channel<bool> _lazyStoreStateQueue =
               Channel.CreateBounded<bool>(1);

        private readonly Channel<bool> _updateRuntimeInfoChannel =
               Channel.CreateBounded<bool>(5);

        /// <summary>
        ///     The last error message logged och catched
        /// </summary>
        public string? LastErrorMessage { get; set; } = null;
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
        public bool IsEnabled { get; set; } = true;

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
                                  .Where(s => s.EntityId == EntityId)
                                  .FirstOrDefault();

            var appState = appInfo?.State as string;
            if (appState == null || (appState != "on" && appState != "off"))
            {
                IsEnabled = true;
                await _daemon.SetStateAsync(EntityId, "on").ConfigureAwait(false);

                return;
            }
            IsEnabled = appState == "on";
        }

        private string EntityId => $"switch.netdaemon_{Id?.ToSafeHomeAssistantEntityId()}";
        /// <inheritdoc/>
        public void SaveAppState()
        {
            // Intentionally ignores full queue since we know
            // a state change already is in progress wich means
            // this state will be saved
            var x = _lazyStoreStateQueue.Writer.TryWrite(true);
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
            _manageRuntimeInformationUpdatesTask = ManageRuntimeInformationUpdates();
            _lazyStoreStateTask = Task.Run(async () => await HandleLazyStorage().ConfigureAwait(false));
            _storageObject = new FluentExpandoObject(false, true, daemon: this);
            Logger = daemon.Logger;

            Logger.LogInformation("Startup: {app}", GetUniqueIdForStorage());
            UpdateRuntimeInformation();
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
                catch (OperationCanceledException)
                {
                    break;
                }
                catch { }   // Ignore errors in thread
            }
        }

        #region -- Logger helpers --

        /// <summary>
        ///     Async disposable support
        /// </summary>
        public async virtual ValueTask DisposeAsync()
        {
            _cancelSource.Cancel();
            if (_manageRuntimeInformationUpdatesTask is object)
                await _manageRuntimeInformationUpdatesTask.ConfigureAwait(false);

            _daemonCallBacksForServiceCalls.Clear();
            
            this.IsEnabled = false;
            _lazyStoreStateTask = null;
            _storageObject = null;
            _daemon = null;

        }

        /// <inheritdoc/>
        public INetDaemonAppBase? GetApp(string appInstanceId)
        {
            _ = _daemon as INetDaemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
            return _daemon!.GetApp(appInstanceId);
        }

        /// <inheritdoc/>
        public void ListenServiceCall(string domain, string service, Func<dynamic?, Task> action)
            => _daemonCallBacksForServiceCalls.Add((domain.ToLowerInvariant(), service.ToLowerInvariant(), action));

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
        public void Log(Exception exception, string message, params object[] param) => LogInformation(exception, message, param);

        /// <inheritdoc/>
        public void LogInformation(string message) => Log(LogLevel.Information, message);

        /// <inheritdoc/>
        public void LogInformation(Exception exception, string message) => Log(LogLevel.Information, exception, message);
        
        /// <inheritdoc/>
        public void LogInformation(string message, params object[] param) => Log(LogLevel.Information, message, param);
        
        /// <inheritdoc/>
        public void LogInformation(Exception exception, string message, params object[] param) => Log(LogLevel.Information, exception, message, param);

        /// <inheritdoc/>
        public void LogDebug(string message) => Log(LogLevel.Debug, message);

        /// <inheritdoc/>
        public void LogDebug(Exception exception, string message) => Log(LogLevel.Debug, exception, message);

        /// <inheritdoc/>
        public void LogDebug(string message, params object[] param) => Log(LogLevel.Debug, message, param);

        /// <inheritdoc/>
        public void LogDebug(Exception exception, string message, params object[] param) => Log(LogLevel.Debug, exception, message, param);

        /// <inheritdoc/>
        public void LogError(string message)
        {
            Log(LogLevel.Error, message);
            LastErrorMessage = message;
            UpdateRuntimeInformation();
        }

        /// <inheritdoc/>
        public void LogError(Exception exception, string message)
        {
            Log(LogLevel.Error, exception, message);
            LastErrorMessage = message;
            UpdateRuntimeInformation();
        }

        /// <inheritdoc/>
        public void LogError(string message, params object[] param)
        {
            Log(LogLevel.Error, message, param);
            LastErrorMessage = message;
            UpdateRuntimeInformation();
        }

        /// <inheritdoc/>
        public void LogError(Exception exception, string message, params object[] param)
        {
            Log(LogLevel.Error, exception, message, param);
            LastErrorMessage = message;
            UpdateRuntimeInformation();
        }

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

        #endregion -- Logger helpers --

        /// <inheritdoc/>
        public void SetAttribute(string attribute, object? value)
        {
            if (value is object)
            {
                _attributes[attribute] = value;
            }
            else
            {
                _attributes.TryRemove(attribute, out _);
            }
            UpdateRuntimeInformation();
        }

        /// <summary>
        ///     Updates runtime information
        /// </summary>
        /// <remarks>
        ///     Use a channel to make sure bad apps do not flood the
        ///     updating of
        /// </remarks>
        internal void UpdateRuntimeInformation()
        {
            // We just ignore if channel is full, it will be ok
            _updateRuntimeInfoChannel.Writer.TryWrite(true);
        }

        private async Task ManageRuntimeInformationUpdates()
        {
            while (!_cancelSource.IsCancellationRequested)
            {
                try
                {
                    while (_updateRuntimeInfoChannel.Reader.TryRead(out _)) ;

                    _ = await _updateRuntimeInfoChannel.Reader.ReadAsync(_cancelSource.Token);
                    // do the deed
                    await HandleUpdateRuntimeInformation().ConfigureAwait(false);
                    // make sure we never push more messages that 10 per second
                    await Task.Delay(100).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    // Just exit
                    break;
                }
            }
        }

        private async Task HandleUpdateRuntimeInformation()
        {
            _ = _daemon as INetDaemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");

            var runtimeInfo = new AppRuntimeInfo
            {
                HasError = false
            };

            if (_attributes.Count() > 0)
                foreach (var (attr, value) in _attributes)
                {
                    if (value is object)
                        runtimeInfo.AppAttributes[attr] = value;
                }

            if (NextScheduledEvent is object)
                runtimeInfo.NextScheduledEvent = NextScheduledEvent;

            if (LastErrorMessage is object)
            {
                runtimeInfo.LastErrorMessage = LastErrorMessage;
                runtimeInfo.HasError = true;
            }

            await _daemon.SetStateAsync(EntityId, IsEnabled ? "on" : "off", ("runtime_info", runtimeInfo)).ConfigureAwait(false);
        }


    }
}