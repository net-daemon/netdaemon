using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NetDaemon.Common.Fluent;

namespace NetDaemon.Common
{
    /// <summary>
    ///     Base class for all NetDaemon App types
    /// </summary>
    public abstract class NetDaemonAppBase : INetDaemonAppBase
    {
        /// <summary>
        ///     A set of properties found in static analysis of code for each app
        /// </summary>
        public static Dictionary<string, Dictionary<string, string>> CompileTimeProperties { get; set; } = new();

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


        // This is declared as static since it will contain state shared globally
        private static ConcurrentDictionary<string, object> _global = new();

        private readonly ConcurrentDictionary<string, object> _attributes = new();

        // To handle state saves max once at a time, internal due to tests
        private readonly Channel<bool> _lazyStoreStateQueue =
               Channel.CreateBounded<bool>(1);

        private readonly Channel<bool> _updateRuntimeInfoChannel =
               Channel.CreateBounded<bool>(5);

        // /// <summary>
        // ///     The last error message logged och catched
        // /// </summary>
        // public string? RuntimeInfo.LastErrorMessage { get; set; } = null;
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
                _ = _daemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
                return _daemon!.Http;
            }
        }

        /// <inheritdoc/>
        public string? Id { get; set; }

        /// <inheritdoc/>
        public bool IsEnabled { get; set; } = true;

        /// <inheritdoc/>
        public string Description
        {
            get
            {
                var app_key = this.GetType().FullName;

                if (app_key is null)
                    return "";

                if (CompileTimeProperties.ContainsKey(app_key))
                {
                    if (CompileTimeProperties[app_key].ContainsKey("description"))
                    {
                        return CompileTimeProperties[app_key]["description"];
                    }
                }
                return "";
            }
        }


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
            if (other is not null && other.Id is not null && this.Id is not null && this.Id == other.Id)
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

            var obj = await _daemon!.GetDataAsync<IDictionary<string, object?>>(GetUniqueIdForStorage()).ConfigureAwait(false);

            if (obj != null)
            {
                var expStore = (FluentExpandoObject)Storage;
                expStore.CopyFrom(obj);
            }

            bool isDisabled = this.Storage.__IsDisabled ?? false;
            var appInfo = _daemon!.State.FirstOrDefault(s => s.EntityId == EntityId);
            var appState = appInfo?.State as string;

            if (isDisabled)
            {
                IsEnabled = false;
                if (appState != "off")
                {
                    dynamic serviceData = new FluentExpandoObject();
                    serviceData.entity_id = EntityId;
                    await _daemon.CallServiceAsync("switch", "turn_off", serviceData);
                }
                return;
            }
            else if (appState == null || (appState != "on" && appState != "off"))
            {
                IsEnabled = true;
                if (appState != "on")
                {
                    dynamic serviceData = new FluentExpandoObject();
                    serviceData.entity_id = EntityId;
                    await _daemon.CallServiceAsync("switch", "turn_on", serviceData);
                }

                return;
            }
            IsEnabled = appState == "on";
        }

        /// <inheritdoc/>
        public string EntityId => $"switch.netdaemon_{Id?.ToSafeHomeAssistantEntityId()}";

        AppRuntimeInfo _runtimeInfo = new AppRuntimeInfo { HasError = false };

        /// <inheritdoc/>
        public AppRuntimeInfo RuntimeInfo => _runtimeInfo;

        /// <inheritdoc/>
        public IEnumerable<string> EntityIds => _daemon?.State.Select(n => n.EntityId) ??
            throw new NullReferenceException("Deamon not expected to be null");

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

            var appInfo = _daemon!.State.FirstOrDefault(s => s.EntityId == EntityId);
            var appState = appInfo?.State as string;
            if (appState == null || (appState != "on" && appState != "off"))
            {
                IsEnabled = true;
            }
            else
            {
                IsEnabled = appState == "on";
            }
            UpdateRuntimeInformation();
            return Task.CompletedTask;
        }

        /// <summary>
        ///     Returns unique Id for instance
        /// </summary>
        public string GetUniqueIdForStorage() => $"{this.GetType().Name}_{Id}".ToLowerInvariant();

        private async Task HandleLazyStorage()
        {
            _ = _storageObject ??
                throw new NullReferenceException($"{nameof(_storageObject)} cant be null!");
            _ = _daemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");

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
                catch (Exception e)
                {
                    Logger.LogError("Error in storage queue {e}", e);
                }   // Ignore errors in thread
            }
        }

        #region -- Logger helpers --

        /// <summary>
        ///     Async disposable support
        /// </summary>
        public async virtual ValueTask DisposeAsync()
        {
            _cancelSource.Cancel();
            if (_manageRuntimeInformationUpdatesTask is not null)
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
            _ = _daemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");
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
            if (param is not null && param.Length > 0)
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
            if (param is not null && param.Length > 0)
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
            RuntimeInfo.LastErrorMessage = message;
            UpdateRuntimeInformation();
        }

        /// <inheritdoc/>
        public void LogError(Exception exception, string message)
        {
            Log(LogLevel.Error, exception, message);
            RuntimeInfo.LastErrorMessage = message;
            UpdateRuntimeInformation();
        }

        /// <inheritdoc/>
        public void LogError(string message, params object[] param)
        {
            Log(LogLevel.Error, message, param);
            RuntimeInfo.LastErrorMessage = message;
            UpdateRuntimeInformation();
        }

        /// <inheritdoc/>
        public void LogError(Exception exception, string message, params object[] param)
        {
            Log(LogLevel.Error, exception, message, param);
            RuntimeInfo.LastErrorMessage = message;
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
            if (value is not null)
            {
                RuntimeInfo.AppAttributes[attribute] = value;
            }
            else
            {
                if (RuntimeInfo.AppAttributes.ContainsKey(attribute))
                {
                    RuntimeInfo.AppAttributes.Remove(attribute);
                }
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
            _ = _daemon ?? throw new NullReferenceException($"{nameof(_daemon)} cant be null!");

            if (RuntimeInfo.LastErrorMessage is not null)
            {
                RuntimeInfo.HasError = true;
            }

            await _daemon!.SetStateAsync(EntityId, IsEnabled ? "on" : "off", ("runtime_info", RuntimeInfo)).ConfigureAwait(false);
        }


    }
}