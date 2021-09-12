using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NetDaemon.Common.Exceptions;
using NetDaemon.Daemon.Services;

namespace NetDaemon.Common
{
    /// <summary>
    ///     Base class for all NetDaemon App types
    /// </summary>
    public abstract class NetDaemonAppBase : INetDaemonAppBase, IApplicationMetadata, INetDaemonPersistantApp
    {
        private ApplicationPersistenceService? _persistenceService;
        
        /// <summary>
        ///     A set of properties found in static analysis of code for each app
        /// </summary>
        public static Dictionary<string, Dictionary<string, string>> CompileTimeProperties { get; } = new();

        private Task? _manageRuntimeInformationUpdatesTask;
      
        /// <summary>
        ///     All actions being performed for service call events
        /// </summary>
        public IList<(string, string, Func<dynamic?, Task>)> DaemonCallBacksForServiceCalls { get; }
            = new List<(string, string, Func<dynamic?, Task>)>();

        // This is declared as static since it will contain state shared globally
        private static readonly ConcurrentDictionary<string, object> _global = new();

        private readonly Channel<bool> _updateRuntimeInfoChannel =
            Channel.CreateBounded<bool>(5);

        private readonly CancellationTokenSource _cancelSource;
        private bool _isDisposed;

        /// <summary>
        ///     Constructor
        /// </summary>
        protected NetDaemonAppBase()
        {
            _cancelSource = new();
            _isDisposed = false;
        }
        
        /// <summary>
        ///    Dependencies on other applications that will be initialized before this app
        /// </summary>
        public IEnumerable<string> Dependencies { get; set; } = new List<string>();

        /// <inheritdoc/>
        public ConcurrentDictionary<string, object> Global => _global;

        /// <inheritdoc/>
        [SuppressMessage("", "CA1065")]
        public IHttpHandler Http
        {
            get
            {
                _ = Daemon ?? throw new NetDaemonNullReferenceException($"{nameof(Daemon)} cant be null!");
                return Daemon!.Http;
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
                var appKey = GetType().FullName;

                if (appKey is null)
                    return "";

                if (CompileTimeProperties.ContainsKey(appKey) &&
                    CompileTimeProperties[appKey].ContainsKey("description"))
                {
                    return CompileTimeProperties[appKey]["description"];
                }

                return "";
            }
        }

        /// <inheritdoc/>
        public ILogger? Logger { get; set; }

        /// <inheritdoc/>
        [SuppressMessage("", "CA1065")]
        public dynamic Storage => _persistenceService!.Storage;

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
        public Task RestoreAppStateAsync() => _persistenceService!.RestoreAppStateAsync();

        /// <inheritdoc/>
        public string EntityId => $"switch.netdaemon_{Id?.ToSafeHomeAssistantEntityId()}";

        /// <inheritdoc/>
        public Type AppType => GetType();

        /// <inheritdoc/>
        public AppRuntimeInfo RuntimeInfo { get; } = new AppRuntimeInfo {HasError = false};

        /// <inheritdoc/>
        [SuppressMessage("", "CA1065")]
        public IEnumerable<string> EntityIds => Daemon?.State.Select(n => n.EntityId) ??
                                                throw new NetDaemonNullReferenceException(
                                                    "Daemon not expected to be null");

        /// <summary>
        ///     Instance to Daemon service
        /// </summary>
        protected INetDaemon? Daemon { get; set; }

        /// <inheritdoc/>
        public IServiceProvider? ServiceProvider => Daemon?.ServiceProvider;

        /// <inheritdoc/>
        public void SaveAppState() => _persistenceService!.SaveAppState();

        /// <inheritdoc/>
        public void Speak(string entityId, string message)
        {
            _ = Daemon ?? throw new NetDaemonNullReferenceException($"{nameof(Daemon)} cant be null!");
            Daemon!.Speak(entityId, message);
        }

        /// <inheritdoc/>
        public virtual Task StartUpAsync(INetDaemon daemon)
        {
            _ = daemon ?? throw new NetDaemonArgumentNullException(nameof(daemon));

            Daemon = daemon;
            Logger = daemon.Logger;

            _manageRuntimeInformationUpdatesTask = ManageRuntimeInformationUpdates();
            _persistenceService = new ApplicationPersistenceService(this, daemon);
            _persistenceService.StartUpAsync(_cancelSource.Token);

            Logger.LogDebug("Startup: {app}", GetUniqueIdForStorage());

            var appInfo = Daemon!.State.FirstOrDefault(s => s.EntityId == EntityId);
            if (appInfo?.State is not string appState || (appState != "on" && appState != "off"))
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
        [SuppressMessage("Microsoft.Globalization", "CA1308")]
        public string GetUniqueIdForStorage() => $"{GetType().Name}_{Id}".ToLowerInvariant();

        /// <summary>
        ///     Async disposable support
        /// </summary>
        public async virtual ValueTask DisposeAsync()
        {
            lock (_cancelSource)
            {
                if (_isDisposed)
                    return;
                _isDisposed = true;
            }

            _cancelSource.Cancel();
            if (_manageRuntimeInformationUpdatesTask is not null)
                await _manageRuntimeInformationUpdatesTask.ConfigureAwait(false);

            DaemonCallBacksForServiceCalls.Clear();

            IsEnabled = false;
            _cancelSource.Dispose();
            Daemon = null;
        }

        /// <inheritdoc/>
        public INetDaemonAppBase? GetApp(string appInstanceId)
        {
            _ = Daemon ?? throw new NetDaemonNullReferenceException($"{nameof(Daemon)} cant be null!");
            return Daemon!.GetApp(appInstanceId);
        }

        /// <inheritdoc/>
        [SuppressMessage("Microsoft.Design", "CA1062")]
        [SuppressMessage("Microsoft.Design", "CA1308")]
        public void ListenServiceCall(string domain, string service, Func<dynamic?, Task> action)
            => DaemonCallBacksForServiceCalls.Add((domain.ToLowerInvariant(), service.ToLowerInvariant(), action));

        #region -- Logger helpers --

        /// <inheritdoc/>
        public void Log(string message) => Log(LogLevel.Information, message);

        /// <inheritdoc/>
        public void Log(Exception exception, string message) => Log(LogLevel.Information, exception, message);

        /// <inheritdoc/>
        public void Log(LogLevel level, string message, params object[] param)
        {
            if (Logger is null)
            {
                return;
            }

            if (param.Length > 0)
            {
                var result = param.Prepend(Id).ToArray();
                Logger.Log(level, $"  {{Id}}: {message}", result);
            }
            else
            {
                Logger.Log(level, $"  {{Id}}: {message}", new object[] {Id ?? ""});
            }
        }

        /// <inheritdoc/>
        public void Log(string message, params object[] param) => Log(LogLevel.Information, message, param);

        /// <inheritdoc/>
        public void Log(LogLevel level, Exception exception, string message, params object[] param)
        {
            if (param.Length > 0)
            {
                var result = param.Prepend(Id).ToArray();
                Logger.Log(level, exception, $"  {{Id}}: {message}", result);
            }
            else
            {
                Logger.Log(level, exception, $"  {{Id}}: {message}", new object[] {Id ?? ""});
            }
        }

        /// <inheritdoc/>
        public void Log(Exception exception, string message, params object[] param) =>
            LogInformation(exception, message, param);

        /// <inheritdoc/>
        public void LogInformation(string message) => Log(LogLevel.Information, message);

        /// <inheritdoc/>
        public void LogInformation(Exception exception, string message) =>
            Log(LogLevel.Information, exception, message);

        /// <inheritdoc/>
        public void LogInformation(string message, params object[] param) => Log(LogLevel.Information, message, param);

        /// <inheritdoc/>
        public void LogInformation(Exception exception, string message, params object[] param) =>
            Log(LogLevel.Information, exception, message, param);

        /// <inheritdoc/>
        public void LogDebug(string message) => Log(LogLevel.Debug, message);

        /// <inheritdoc/>
        public void LogDebug(Exception exception, string message) => Log(LogLevel.Debug, exception, message);

        /// <inheritdoc/>
        public void LogDebug(string message, params object[] param) => Log(LogLevel.Debug, message, param);

        /// <inheritdoc/>
        public void LogDebug(Exception exception, string message, params object[] param) =>
            Log(LogLevel.Debug, exception, message, param);

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
        public void LogTrace(Exception exception, string message, params object[] param) =>
            Log(LogLevel.Trace, exception, message, param);

        /// <inheritdoc/>
        public void LogWarning(string message) => Log(LogLevel.Warning, message);

        /// <inheritdoc/>
        public void LogWarning(Exception exception, string message) => Log(LogLevel.Warning, exception, message);

        /// <inheritdoc/>
        public void LogWarning(string message, params object[] param) => Log(LogLevel.Warning, message, param);

        /// <inheritdoc/>
        public void LogWarning(Exception exception, string message, params object[] param) =>
            Log(LogLevel.Warning, exception, message, param);

        #endregion -- Logger helpers --

        /// <inheritdoc/>
        public void SetAttribute(string attribute, object? value)
        {
            if (value is not null)
            {
                RuntimeInfo.AppAttributes[attribute] = value;
            }
            else if (RuntimeInfo.AppAttributes.ContainsKey(attribute))
            {
                RuntimeInfo.AppAttributes.Remove(attribute);
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
                    _ = await _updateRuntimeInfoChannel.Reader.ReadAsync(_cancelSource.Token).ConfigureAwait(false);
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
            _ = Daemon ?? throw new NetDaemonNullReferenceException($"{nameof(Daemon)} cant be null!");

            if (RuntimeInfo.LastErrorMessage is not null)
            {
                RuntimeInfo.HasError = true;
            }

            if (Daemon!.IsConnected)
            {
                await Daemon!.SetStateAsync(EntityId, IsEnabled ? "on" : "off", ("runtime_info", RuntimeInfo))
                    .ConfigureAwait(false);
            }
        }
    }
}