using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JoySoftware.HomeAssistant.Client;
using JoySoftware.HomeAssistant.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetDaemon.Common;
using NetDaemon.Common.Exceptions;
using NetDaemon.Common.Reactive;
using NetDaemon.Daemon.Storage;
using NetDaemon.Infrastructure.Extensions;
using NetDaemon.Infrastructure.ObservableHelpers;
using NetDaemon.Mapping;

[assembly: InternalsVisibleTo("NetDaemon.Daemon.Tests")]
[assembly: InternalsVisibleTo("NetDaemon.Fakes")]

namespace NetDaemon.Daemon
{
    public class NetDaemonHost : INetDaemonHost, IAsyncDisposable
    {
        private static readonly ConcurrentBag<Func<ExternalEventBase, Task>> _concurrentBag = new();

        internal readonly ConcurrentBag<Func<ExternalEventBase, Task>> _externalEventCallSubscribers =
            _concurrentBag;

        // Internal for test
        internal readonly ConcurrentDictionary<string, HassArea> _hassAreas = new();
        internal readonly ConcurrentDictionary<string, HassDevice> _hassDevices = new();
        internal readonly ConcurrentDictionary<string, HassEntity> _hassEntities = new();

        internal EntityStateManager StateManager { get; }

        private AppManager _appManager;
        internal TextToSpeechService TextToSpeechService { get; }

        internal async Task WaitForTasksAsync()
        {
            await TextToSpeechService.StopAsync().ConfigureAwait(false);
            await Task.WhenAll(_backgroundTasks.Keys).ConfigureAwait(false);
        }

        private IInstanceDaemonApp? _appInstanceManager;

        // Internal token source for just cancel this objects activities
        private readonly CancellationTokenSource _cancelDaemon = new();

        private readonly ConcurrentBag<(string, string, Func<dynamic?, Task>)> _daemonServiceCallFunctions = new();

        /// <summary>
        ///     Currently running tasks for handling new events from HomeAssistant
        /// </summary>
        private readonly ConcurrentDictionary<Task, object?> _backgroundTasks = new();

        private IHassClient? _hassClient;

        private readonly IHttpHandler? _httpHandler;

        private readonly IDataRepository? _repository;

        private bool _isDisposed;

        // Following token source and token are set at RUN
        private CancellationToken _cancelToken;

        private CancellationTokenSource? _cancelTokenSource;

        internal bool HasNetDaemonIntegration;

        // This is non null if this is running in a add-on
        private readonly string? _addOnToken = Environment.GetEnvironmentVariable("HASSIO_TOKEN");
        private readonly IHassClientFactory _hassClientFactory;
        internal bool IsAddOn => _addOnToken != null;
        public IServiceProvider ServiceProvider { get; }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="hassClientFactory">Factory to use for instance HassClients</param>
        /// <param name="repository">Repository to use</param>
        /// <param name="loggerFactory">The loggerfactory</param>
        /// <param name="httpHandler">Http handler to use</param>
        /// <param name="serviceProvider">The service provider</param>
        public NetDaemonHost(
            IHassClientFactory? hassClientFactory = null,
            IDataRepository? repository = null,
            ILoggerFactory? loggerFactory = null,
            IHttpHandler? httpHandler = null,
            IServiceProvider? serviceProvider = null
        )
        {
            loggerFactory ??= DefaultLoggerFactory;
            _httpHandler = httpHandler;
            Logger = loggerFactory.CreateLogger<NetDaemonHost>();
            _hassClientFactory = hassClientFactory
                                 ?? throw new ArgumentNullException(nameof(hassClientFactory));
            ServiceProvider = serviceProvider ?? new ServiceCollection().BuildServiceProvider();
            _repository = repository;
            _isDisposed = false;
            Logger.LogTrace("Instance NetDaemonHost");

            _hassClient = _hassClientFactory.New() ??
                          throw new NetDaemonNullReferenceException(
                              $"Failed to create instance of {nameof(_hassClient)}");

            StateManager = new EntityStateManager(this);
            TextToSpeechService = new TextToSpeechService(this);

            HassEventsObservable = Observable.FromEventPattern<EventHandler<HassEvent>, HassEvent>(
                a => HassEvents += a,
                a => HassEvents -= a).Select(e => e.EventArgs)
                .AsConcurrent(t => TrackBackgroundTask(t));

            _appManager = new AppManager(ServiceProvider, Logger);
        }

        // for unit testing with INetDaemonAppBase apps that are instantiated by the test code
        internal void AddRunningApp(INetDaemonAppBase app)
        {
            _ = app.Id ?? throw new InvalidOperationException("app.id should not be null");
            var applicationContext = ApplicationContext.CreateFromAppInstanceForTest(app, ServiceProvider);
            InternalRunningAppInstances[applicationContext.Id!] = applicationContext;
            InternalAllAppInstances[applicationContext.Id!] = applicationContext;
        }

        public IObservable<HassEvent> HassEventsObservable { get; }

        private event EventHandler<HassEvent>? HassEvents;

        public bool IsConnected { get; private set; }

        public IHttpHandler Http =>
            _httpHandler ?? throw new NetDaemonNullReferenceException("HttpHandler can not be null!");

        public ILogger Logger { get; }

        public IEnumerable<ApplicationContext> AllAppContexts => InternalAllAppInstances.Values;

        public IEnumerable<INetDaemonAppBase> AllAppInstances => InternalAllAppInstances.Values.Select(c => c.ApplicationInstance).OfType<INetDaemonAppBase>();

        private IEnumerable<NetDaemonRxApp> NetDaemonRxApps => InternalRunningAppInstances.Values.Select(c => c.ApplicationInstance).OfType<NetDaemonRxApp>();

        private ConcurrentDictionary<string, ApplicationContext> InternalAllAppInstances => _appManager.InternalAllAppInstances;

        private ConcurrentDictionary<string, ApplicationContext> InternalRunningAppInstances => _appManager.InternalRunningAppInstances;


        private IEnumerable<IObserver<RxEvent>> EventChangeObservers =>
            NetDaemonRxApps.SelectMany(app => ((EventObservable)app.EventChangesObservable).Observers);

        [SuppressMessage("", "CA1721")]
        public IEnumerable<EntityState> State => StateManager.States;


        private static ILoggerFactory DefaultLoggerFactory => LoggerFactory.Create(builder =>
        {
            builder
                .ClearProviders()
                .AddConsole();
        });

        public IDictionary<string, object> DataCache { get; } = new Dictionary<string, object>();
        internal CancellationToken CancelToken => _cancelToken;
        internal IHassClient Client => _hassClient ?? throw new NetDaemonNullReferenceException(nameof(_hassClient));

        public Task<IReadOnlyCollection<HassServiceDomain>> GetAllServices() => Client.GetServices();

        [SuppressMessage("", "CA1031")]
        public void TriggerWebhook(string id, object? data, bool waitForResponse = false)
        {
            var task = Client.TriggerWebhook(id, data);

            if (!waitForResponse)
            {
                TrackBackgroundTask(task, "TriggerWebhook");
            }
            else
            {
                task.Wait(_cancelToken);
            }
        }

        [SuppressMessage("", "CA1031")]
        public void CallService(string domain, string service, object? data = null, bool waitForResponse = false)
        {
            var task = Client.CallService(domain, service, data!, null, waitForResponse);

            if (!waitForResponse)
            {
                TrackBackgroundTask(task, $"CallService {domain},{service}");
            }
            else
            {
                task.Wait(_cancelToken);
            }
        }

        public void CallService(string domain, string service, HassTarget target, object? data = null, bool waitForResponse = false)
        {
            var task = Client.CallService(domain, service, data!, target, waitForResponse);

            if (!waitForResponse)
            {
                TrackBackgroundTask(task, $"CallService {domain},{service}");
            }
            else
            {
                task.Wait(_cancelToken);
            }
        }

        [SuppressMessage("", "CA1031")]
        public async Task CallServiceAsync(string domain, string service, object? data = null,
            bool waitForResponse = false)
        {
            try
            {
                await Client.CallService(domain, service, data!, null, waitForResponse).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Failed call service");
            }
        }

        public async ValueTask DisposeAsync()
        {
            lock (_cancelDaemon)
            {
                if (_isDisposed)
                    return;
                _isDisposed = true;
            }

            _cancelDaemon.Cancel();
            await Stop().ConfigureAwait(false);
            _cancelDaemon.Dispose();
            _cancelTokenSource?.Dispose();

            GC.SuppressFinalize(this);
            Logger.LogTrace("Instance NetDaemonHost Disposed");
        }

        private void EnableApplicationDiscoveryService()
        {
            // For service call reload_apps we do just that... reload the fucking apps yay :)
            ListenCompanionServiceCall("reload_apps", async (_) => await ReloadAllApps().ConfigureAwait(false));

            RegisterAppSwitchesAndTheirStates();
        }

        /// <inheritdoc/>
        public INetDaemonAppBase? GetApp(string appInstanceId)
        {
            return InternalRunningAppInstances.ContainsKey(appInstanceId)
                ? InternalRunningAppInstances[appInstanceId].ApplicationInstance as INetDaemonAppBase
                : null;
        }

        public async Task<T?> GetDataAsync<T>(string id) where T : class
        {
            _ = _repository ??
                throw new NetDaemonNullReferenceException($"{nameof(_repository)} can not be null!");

            if (DataCache.ContainsKey(id))
            {
                return (T)DataCache[id];
            }

            var data = await _repository!.Get<T>(id).ConfigureAwait(false);

            if (data != null)
                DataCache[id] = data;

            return data;
        }

        public EntityState? GetState(string entityId) => StateManager.GetState(entityId);

        /// <inheritdoc/>
        public async Task Initialize(IInstanceDaemonApp appInstanceManager)
        {
            if (!IsConnected)
                throw new NetDaemonException("NetDaemon is not connected, no use in initializing");

            _appInstanceManager = appInstanceManager;

            await ReloadAllApps().ConfigureAwait(false);
            EnableApplicationDiscoveryService();
        }

        /// <inheritdoc/>
        public void ListenCompanionServiceCall(string service, Func<dynamic?, Task> action)
        {
            _ = service ??
                throw new NetDaemonArgumentNullException(nameof(service));

            _daemonServiceCallFunctions.Add(("netdaemon", service.ToLowerInvariant(), action));
        }

        public void ListenServiceCall(string domain, string service, Func<dynamic?, Task> action)
        {
            _ = service ??
                throw new NetDaemonArgumentNullException(nameof(service));
            _ = domain ??
                throw new NetDaemonArgumentNullException(nameof(domain));
            _daemonServiceCallFunctions.Add((domain.ToLowerInvariant(), service.ToLowerInvariant(), action));
        }

        /// <summary>
        ///     Runs the Daemon
        /// </summary>
        /// <remarks>
        ///     Connects to Home Assistant and the task completes if canceled or if Home Assistant
        ///     can´t be connected or disconnects.
        /// </remarks>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="ssl"></param>
        /// <param name="token"></param>
        /// <param name="cancellationToken"></param>
        [SuppressMessage("", "CA1031")]
        public async Task Run(string host, short port, bool ssl, string token, CancellationToken cancellationToken)
        {
            // If the host is reconnected the hass client will be null so lets instance new one
            _hassClient = _hassClientFactory.New() ??
                          throw new NetDaemonNullReferenceException(
                              $"Failed to create instance of {nameof(_hassClient)}");
            // Create combine cancellation token
            InitCancellationTokens(cancellationToken);

            try
            {
                var connectResult = _addOnToken != null
                    ? await _hassClient.ConnectAsync(new Uri(_supervisorWebsocketUri),
                            _addOnToken, false)
                        .ConfigureAwait(false)
                    : await _hassClient.ConnectAsync(host, port, ssl, token, false).ConfigureAwait(false);

                if (!connectResult)
                {
                    IsConnected = false;
                    await _hassClient.CloseAsync().ConfigureAwait(false);
                    return;
                }

                var hassConfig = await _hassClient.GetConfig().ConfigureAwait(false);
                if (hassConfig.State != "RUNNING")
                {
                    Logger.LogInformation("Home Assistant is not ready yet, state: {State} ..", hassConfig.State);
                    await _hassClient.CloseAsync().ConfigureAwait(false);
                    return;
                }

                TrackBackgroundTask(TextToSpeechService.ProcessAsync());

                await RefreshInternalStatesAndSetArea().ConfigureAwait(false);

                await _hassClient.SubscribeToEvents().ConfigureAwait(false);

                await ConnectToHAIntegration().ConfigureAwait(false);

                IsConnected = true;

                Logger.LogInformation(
                    IsAddOn
                        ? "Successfully connected to Home Assistant Core in Home Assistant Add-on"
                        : "Successfully connected to Home Assistant Core on host {host}:{port}", host, port);

                while (!_cancelToken.IsCancellationRequested)
                {
                    if (!await ReadEvent().ConfigureAwait(false)) break;
                }
            }
            catch (OperationCanceledException)
            {
                // Normal operation, ignore and return
            }
            catch (Exception e)
            {
                IsConnected = false;
                Logger.LogError(e, "Error, during operation");
            }
            finally
            {
                // Set cancel token to avoid background processes
                // to access disconnected Home Assistant
                IsConnected = false;
                _cancelTokenSource?.Cancel();
                try
                {
                    using var waitTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                    await Task.WhenAny(Task.WhenAll(_backgroundTasks.Keys), waitTokenSource.Token.AsTask())
                        .ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Logger.LogTrace(e, "Failed to cancel the running tasks");
                }
            }
        }

        private const string _supervisorWebsocketUri = "ws://supervisor/core/websocket";

        /// <summary>
        ///     Reads next event from home assistant and handles it async
        /// </summary>
        /// <returns>true if read operation is success</returns>
        /// <exception cref="NetDaemonNullReferenceException"></exception>
        private async Task<bool> ReadEvent()
        {
            HassEvent changedEvent = await Client.ReadEventAsync(_cancelToken).ConfigureAwait(false);
            if (changedEvent == null)
            {
                // Will only happen when doing unit tests
                await Task.Delay(1, _cancelToken).ConfigureAwait(false);
            }
            else
            {
                if (changedEvent.Data is HassServiceEventData hseData && hseData.Domain == "homeassistant" &&
                    (hseData.Service == "stop" || hseData.Service == "restart"))
                {
                    // The user stopped HA so just stop processing messages
                    Logger.LogInformation("User {Action} Home Assistant, will try to reconnect...",
                        hseData.Service == "stop" ? "stopping" : "restarting");
                    return false;
                }

                TrackBackgroundTask(HandleNewEvent(changedEvent));
            }

            return true;
        }

        private void InitCancellationTokens(CancellationToken cancellationToken)
        {
            _cancelTokenSource =
                CancellationTokenSource.CreateLinkedTokenSource(_cancelDaemon.Token, cancellationToken);
            _cancelToken = _cancelTokenSource.Token;
            _cancelToken.ThrowIfCancellationRequested();
        }

        [SuppressMessage("", "IDE1006")]
        private record NetDaemonInfo(string Version);

        [SuppressMessage("", "CA1031")]
        internal async Task ConnectToHAIntegration()
        {
            try
            {
                var x = await Client.GetApiCall<NetDaemonInfo>("netdaemon/info").ConfigureAwait(false);
                if (x is not null)
                {
                    HasNetDaemonIntegration = true;
                    return;
                }
            }
            catch
            {
                // for now just ignore
            }

            Logger.LogWarning(
                "No NetDaemon integration found, please consider installing the companion integration for more features. See https://netdaemon.xyz/docs/started/integration for details.");
        }

        public Task SaveDataAsync<T>(string id, T data)
        {
            _ = _repository ??
                throw new NetDaemonNullReferenceException($"{nameof(_repository)} can not be null!");

            if (data == null)
                throw new NetDaemonArgumentNullException(nameof(data));

            DataCache[id] = data;
            return _repository.Save(id, data);
        }

        [SuppressMessage("", "CA1031")]
        public async Task<bool> SendEvent(string eventId, dynamic? data = null)
        {
            if (!IsConnected) return false;

            try
            {
                return await Client.SendEvent(eventId, data).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Logger.LogDebug(e, "Error sending event!");
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task SetDaemonStateAsync(int numberOfLoadedApps, int numberOfRunningApps)
        {
            await SetStateAndWaitForResponseAsync(
                    "sensor.netdaemon_status",
                    "Connected", // State will always be connected, otherwise state could not be set.
                    new
                    {
                        number_of_loaded_apps = numberOfLoadedApps,
                        number_of_running_apps = numberOfRunningApps,
                        version = GetType().Assembly.GetName().Version?.ToString() ?? "N/A",
                    }, false)
                .ConfigureAwait(false);
        }

        public EntityState? SetState(string entityId, object state, object? attributes = null,
            bool waitForResponse = false)
        {
            var task = SetStateAndWaitForResponseAsync(entityId, state, attributes, true);

            if (!waitForResponse)
            {
                TrackBackgroundTask(task, $"Set state for {entityId}");
                return null;
            }

            task.Wait(_cancelToken);
            return task.Result;
        }

        public async Task<EntityState?> SetStateAndWaitForResponseAsync(string entityId, object state,
            object? attributes, bool waitForResponse)
        {
            _ = entityId ?? throw new ArgumentNullException(nameof(entityId));
            _ = state ?? throw new ArgumentNullException(nameof(state));
            var stateString = Convert.ToString(state, CultureInfo.InvariantCulture) ?? string.Empty;

            var hassState = await StateManager.SetStateAndWaitForResponseAsync(entityId, stateString, attributes,
                    waitForResponse)
                .ConfigureAwait(false);
            return hassState;
        }

        public Task<EntityState?> SetStateAsync(string entityId, object state,
            params (string name, object val)[] attributes)
            => SetStateAndWaitForResponseAsync(entityId, state, attributes.ToDynamic(), true);

        public void Speak(string entityId, string message) => TextToSpeechService.Speak(entityId, message);

        [SuppressMessage("", "CA1031")]
        public async Task Stop()
        {
            try
            {
                Logger.LogTrace("Try stopping Instance NetDaemonHost");

                await UnloadAllApps().ConfigureAwait(false);

                StateManager.Clear();
                InternalAllAppInstances.Clear();
                InternalRunningAppInstances.Clear();
                _hassAreas.Clear();
                _hassDevices.Clear();
                _hassEntities.Clear();
                _daemonServiceCallFunctions.Clear();
                _externalEventCallSubscribers.Clear();
                _backgroundTasks.Clear();

                IsConnected = false;
                if (_hassClient is not null)
                {
                    await _hassClient.CloseAsync().ConfigureAwait(false);
                    await _hassClient.DisposeAsync().ConfigureAwait(false);
                    _hassClient = null;
                }

                Logger.LogTrace("Stopped Instance NetDaemonHost");
            }
            catch (Exception e)
            {
                Logger.LogError("Error stopping NetDaemon, use trace level for details");
                Logger.LogTrace(e, "Error stopping NetDaemon");
            }
        }

        /// <inheritdoc/>
        public async Task ReloadAllApps()
        {
            if (_appInstanceManager == null) throw new InvalidOperationException("_appInstanceManager has not yet been initialized");

            await _appManager.ReloadAllApps(_appInstanceManager).ConfigureAwait(false);

            await SetDaemonStateAsync(_appInstanceManager.Count, InternalRunningAppInstances.Count)
                .ConfigureAwait(false);

            foreach (var applicationContext in _appManager.InternalRunningAppInstances.Values)
            {
                if (applicationContext.ApplicationInstance is NetDaemonRxApp netDaemonRxApp)
                {
                    await netDaemonRxApp.HandleAttributeInitialization(this).ConfigureAwait(false);
                }
            }
        }

        /// <inheritdoc/>
        [SuppressMessage("", "CA1031")]
        public Task UnloadAllApps() => _appManager.UnloadAllApps();

        /// <summary>
        ///     Fixes the type differences that can be from Home Assistant depending on
        ///     different conditions
        /// </summary>
        /// <remarks>
        ///     If a sensor is unavailable that normally has a primitive value
        ///     it can be a string. The automations might expect a integer.
        ///     Another scenario is that a value of 10 is cast as long and
        ///     next time a value of 11.3 is cast to double. T
        ///     FixStateTypes fixes these problem by casting correct types
        ///     or setting null. It returns false if the casts can not be
        ///     managed.
        /// </remarks>
        internal static bool FixStateTypes(ref EntityState oldState, ref EntityState newState)
        {
            // Both states can not be null, something is seriously wrong
            if (newState.State is null && oldState.State is null)
                return false;

            if (newState.State is not null && oldState.State is not null)
            {
                Type? newStateType = newState.State?.GetType();
                Type? oldStateType = oldState.State?.GetType();

                if (newStateType != oldStateType)
                {
                    // We have a potential problem with unavailable or unknown entity state
                    // Lets start checking that
                    if (newStateType == typeof(string) || oldStateType == typeof(string))
                    {
                        // We have a statechange to or from string, just ignore for now and set the string to null
                        if (newStateType == typeof(string))
                            newState = newState with { State = null };
                        else
                            oldState = oldState with { State = null };
                    }
                    else if (newStateType == typeof(double) || oldStateType == typeof(double))
                    {
                        if (newStateType == typeof(double))
                        {
                            // Try convert the integer to double
                            if (oldStateType == typeof(long))
                                oldState = oldState with { State = Convert.ToDouble(oldState!.State, CultureInfo.InvariantCulture) };
                            else
                                return false; // We do not support any other conversion
                        }
                        else
                        {
                            // Try convert the long to double
                            if (newStateType == typeof(long))
                                newState = newState with { State = Convert.ToDouble(newState!.State, CultureInfo.InvariantCulture) };
                            else
                                return false; // We do not support any other conversion
                        }
                    }
                    else
                    {
                        // We do not support the conversion, just return false
                    }
                }
            }

            return true;
        }

        /// <summary>
        ///     Returns any area asociated with the entity or null if missing
        /// </summary>
        /// <remarks>
        ///     It looks for relevant area in the following order:
        ///         - The area assigned to the entity
        ///         - The area assigned to the device the entity is assigned to
        ///         - null if not found
        /// </remarks>
        /// <param name="entityId"></param>
        internal string? GetAreaForEntityId(string entityId)
        {
            if (_hassEntities.TryGetValue(entityId, out HassEntity? entity))
            {
                if (!string.IsNullOrEmpty(entity.AreaId))
                    return entity.AreaId;
                if (entity.DeviceId is null)
                    return null;
                if (!_hassDevices.TryGetValue(entity.DeviceId, out HassDevice? device) || device.AreaId is null)
                    return null;
                if (_hassAreas.TryGetValue(device.AreaId, out HassArea? area))
                {
                    return area.Name;
                }
            }
            return null;
        }
        internal async Task RefreshInternalStatesAndSetArea()
        {
            _ = _hassClient ?? throw new NetDaemonNullReferenceException(nameof(_hassClient));

            foreach (var device in await _hassClient.GetDevices().ConfigureAwait(false))
            {
                if (device?.Id != null)
                    _hassDevices[device.Id] = device;
            }

            foreach (var area in await _hassClient.GetAreas().ConfigureAwait(false))
            {
                if (area?.Id != null)
                    _hassAreas[area.Id] = area;
            }

            foreach (var entity in await _hassClient.GetEntities().ConfigureAwait(false))
            {
                if (entity?.EntityId != null)
                    _hassEntities[entity.EntityId] = entity;
            }

            await StateManager.RefreshAsync().ConfigureAwait(false);
        }

        [SuppressMessage("", "CA1031")]
        protected virtual async Task HandleNewEvent(HassEvent hassEvent)
        {
            _ = hassEvent ??
                throw new NetDaemonArgumentNullException(nameof(hassEvent));
            try
            {
                switch (hassEvent.EventType)
                {
                    case "state_changed":
                        HandleStateChangeEvent(hassEvent);
                        break;
                    case "call_service":
                        HandleCallServiceEvent(hassEvent);
                        break;
                    case "device_registry_updated":
                    case "area_registry_updated":
                        await RefreshInternalStatesAndSetArea().ConfigureAwait(false);
                        break;
                    default:
                        HandleCustomEvent(hassEvent);
                        break;
                }

                HassEvents?.Invoke(this, hassEvent);
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Failed to handle new event ({hassEvent.EventType})");
            }
        }

        [SuppressMessage("", "CA1031")]
        private void HandleStateChangeEvent(HassEvent hassEvent)
        {
            var stateData = (HassStateChangedEventData?)hassEvent.Data;

            if (stateData is null)
            {
                throw new NetDaemonNullReferenceException("StateData is null!");
            }

            if (stateData.NewState is null || stateData.OldState is null)
            {
                // This is an entity that is removed and have no new state so just return;
                return;
            }

            // Make sure we get the area name with the new state
            var area = GetAreaForEntityId(stateData!.NewState.EntityId);
            var newState = stateData!.NewState!.MapWithArea(area);
            var oldState = stateData!.OldState!.MapWithArea(area);

            if (!FixStateTypes(ref oldState, ref newState))
            {
                if (stateData.NewState?.State != stateData.OldState?.State)
                {
                    var sb = new StringBuilder();
                    sb.Append("Can not fix state typing for ").AppendLine(stateData.NewState?.EntityId);
                    sb.Append("NewStateObject: ").Append(stateData.NewState).AppendLine();
                    sb.Append("OldStateObject: ").Append(stateData.OldState).AppendLine();
                    sb.Append("NewState: ").AppendLine(stateData.NewState?.State);
                    sb.Append("OldState: ").AppendLine(stateData.OldState?.State);
                    sb.Append("NewState type: ").AppendLine(stateData.NewState?.State?.GetType().ToString() ?? "null");
                    sb.Append("OldState type: ").AppendLine(stateData.OldState?.State?.GetType().ToString() ?? "null");
                    Logger.LogTrace(sb.ToString());
                }

                return;
            }

            StateManager.Store(newState);

            foreach (var netDaemonRxApp in NetDaemonRxApps)
            {
                // Call the observable with no blocking
                foreach (var observer in ((StateChangeObservable)netDaemonRxApp.StateChangesObservable).Observers)
                {
                    TrackBackgroundTask(Task.Run(() =>
                    {
                        try
                        {
                            observer.OnNext((oldState, newState));
                        }
                        catch (Exception e)
                        {
                            observer.OnError(e);
                            netDaemonRxApp.LogError(e,
                                $"Fail to OnNext on state change observer. {newState.EntityId}:{newState?.State}({oldState?.State})");
                        }
                    }, _cancelToken));
                }
            }
        }

        [SuppressMessage("", "CA1031")]
        private void HandleCallServiceEvent(HassEvent hassEvent)
        {
            _ = EventChangeObservers ??
                throw new NetDaemonNullReferenceException(nameof(EventChangeObservers));

            var serviceCallData = (HassServiceEventData?)hassEvent.Data;

            if (serviceCallData == null)
            {
                throw new NetDaemonNullReferenceException("ServiceData is null! not expected");
            }

            foreach (var netDaemonRxApp in NetDaemonRxApps)
            {
                // Call any service call registered
                foreach (var (domain, service, func) in netDaemonRxApp.DaemonCallBacksForServiceCalls)
                {
                    if (domain == serviceCallData.Domain &&
                        service == serviceCallData.Service)
                    {
                        TrackBackgroundTask(Task.Run(() => func(serviceCallData.Data)));
                    }
                }
            }

            // Call the observable with no blocking
            foreach (var observer in EventChangeObservers)
            {
                TrackBackgroundTask(Task.Run(() =>
                {
                    try
                    {
                        var rxEvent = new RxEvent(serviceCallData.Service, serviceCallData.Domain,
                            serviceCallData.ServiceData);
                        observer.OnNext(rxEvent);
                    }
                    catch (Exception e)
                    {
                        try
                        {
                            observer.OnError(e);
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError(ex, "Fail to OnError on event observer (service_call)");
                        }

                        Logger.LogError(e, "Fail to OnNext on event observer (service_call)");
                    }
                }, _cancelToken));
            }

            foreach (var (domain, service, func) in _daemonServiceCallFunctions)
            {
                if (domain == serviceCallData.Domain &&
                    service == serviceCallData.Service)
                {
                    TrackBackgroundTask(Task.Run(() => func(serviceCallData.Data)));
                }
            }
        }

        [SuppressMessage("", "CA1031")]
        private void HandleCustomEvent(HassEvent hassEvent)
        {
            _ = EventChangeObservers ??
                throw new NetDaemonNullReferenceException(nameof(EventChangeObservers));

            // Convert ExpandoObject to FluentExpandoObject
            // We need to do this so not existing attributes
            // is returning null
            if (hassEvent.Data is ExpandoObject exObject)
            {
                hassEvent.Data = new FluentExpandoObject(true, true, exObject);
            }

            // Call the observable with no blocking
            foreach (var observer in EventChangeObservers)
            {
                TrackBackgroundTask(Task.Run(() =>
                {
                    try
                    {
                        var rxEvent = new RxEvent(hassEvent.EventType, null, hassEvent.Data);
                        observer.OnNext(rxEvent);
                    }
                    catch (Exception e)
                    {
                        observer.OnError(e);
                        Logger.LogError(e, "Fail to OnNext on event observer (event)");
                    }
                }, _cancelToken));
            }
        }

        [SuppressMessage("", "CA1031")]
        private void RegisterAppSwitchesAndTheirStates()
        {
            ListenServiceCall("switch", "turn_on",
                async (data) => await SetStateOnDaemonAppSwitch("on", data).ConfigureAwait(false));

            ListenServiceCall("switch", "turn_off",
                async (data) => await SetStateOnDaemonAppSwitch("off", data).ConfigureAwait(false));

            ListenServiceCall("switch", "toggle", async (data) =>
            {
                try
                {
                    string? entityId = data?.entity_id;
                    if (entityId is null)
                        return;

                    var currentState = GetState(entityId)?.State as string;

                    if (currentState == "on")
                        await SetStateOnDaemonAppSwitch("off", data).ConfigureAwait(false);
                    else
                        await SetStateOnDaemonAppSwitch("on", data).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Logger.LogWarning(e, "Failed to set state from netdaemon switch");
                }
            });

            async Task SetStateOnDaemonAppSwitch(string state, dynamic? data)
            {
                object? entityIdField = data?.entity_id;

                // the Entity_id can be a single string or an array
                var entityIds = (entityIdField switch
                {
                    string id => new[] { id },
                    object[] arr => arr.OfType<string>().ToArray(),
                    _ => Array.Empty<string>()
                })
                    // We only want app switches
                    .Where(id => id.StartsWith("switch.netdaemon_", true, CultureInfo.InvariantCulture))
                    .ToList();

                if (entityIds.Count == 0) return;

                foreach (var entityId in entityIds)
                {
                    await SetDependentState(entityId, state).ConfigureAwait(false);
                }

                await ReloadAllApps().ConfigureAwait(false);

                await PostExternalEvent(new AppsInformationEvent()).ConfigureAwait(false);
            }
        }

        private async Task SetDependentState(string entityId, string state)
        {
            var app = InternalAllAppInstances.Values.FirstOrDefault(n => n.EntityId == entityId);

            if (app is not null)
            {
                if (state == "off")
                {
                    // We need to turn off any dependent apps
                    foreach (var depApp in InternalAllAppInstances.Values.Where(n => n.Dependencies.Contains(app.Id)))
                    {
                        await SetDependentState(depApp.EntityId, state).ConfigureAwait(false);
                    }

                    app.IsEnabled = false;
                    await PersistAppStateAsync(app).ConfigureAwait(false);
                    Logger.LogDebug("SET APP {App} state = disabled", app.Id);
                }
                else if (state == "on")
                {
                    app.IsEnabled = true;
                    // Enable all apps that this app is dependent on
                    foreach (var depOnId in app.Dependencies)
                    {
                        var depOnApp = InternalAllAppInstances.Values.FirstOrDefault(n => n.Id == depOnId);
                        if (depOnApp is not null)
                        {
                            await SetDependentState(depOnApp.EntityId, state).ConfigureAwait(false);
                        }
                    }

                    await PersistAppStateAsync(app).ConfigureAwait(false);
                    Logger.LogDebug("SET APP {App} state = enabled", app.Id);
                }
            }

            List<(string, object)>? attributes = null;

            var entityAttributes = GetState(entityId)?.Attribute as IDictionary<string, object>;

            if (entityAttributes is not null && entityAttributes.Count > 0)
                attributes = entityAttributes.Keys.Select(n => (n, entityAttributes[n])).ToList();

            if (attributes is not null && attributes.Count > 0)
                await SetStateAsync(entityId, state, attributes.ToArray()).ConfigureAwait(false);
            else
                await SetStateAsync(entityId, state).ConfigureAwait(false);
        }

        //TODO: Refactor this
        private async Task PersistAppStateAsync(ApplicationContext app)
        {
            var uniqueIdForStorage = $"{app.ApplicationType.Name}_{app.Id}".ToLowerInvariant();
            var obj = await GetDataAsync<IDictionary<string, object?>>(uniqueIdForStorage)
                          .ConfigureAwait(false) ??
                      new Dictionary<string, object?>();

            obj["__IsDisabled"] = !app.IsEnabled;
            await SaveDataAsync(uniqueIdForStorage, obj).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public void SubscribeToExternalEvents(Func<ExternalEventBase, Task> func)
        {
            _externalEventCallSubscribers.Add(func);
        }

        private async Task PostExternalEvent(ExternalEventBase ev)
        {
            if (_externalEventCallSubscribers.IsEmpty)
                return;

            var callbackTaskList = new List<Task>(_externalEventCallSubscribers.Count);

            foreach (var callback in _externalEventCallSubscribers)
            {
                callbackTaskList.Add(Task.Run(() => callback(ev)));
            }

            await callbackTaskList.WhenAll(_cancelToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public bool HomeAssistantHasNetDaemonIntegration() => HasNetDaemonIntegration;

        private void TrackBackgroundTask(Task task, string? description = null)
        {
            _backgroundTasks.TryAdd(task, null);

            [SuppressMessage("", "CA1031")]
            async Task Wrap()
            {
                try
                {
                    await task.ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Logger.LogError(e, description == null ? null : "Exception in background task: " + description);
                }
                finally
                {
                    _backgroundTasks.TryRemove(task, out var _);
                }
            }
            // We do not handle task here cause exceptions
            // are handled in the Wrap local functions and
            // all tasks should be cancelable
            _ = Wrap();
        }
    }
}