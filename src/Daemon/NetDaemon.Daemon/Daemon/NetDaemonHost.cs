using JoySoftware.HomeAssistant.Client;
using JoySoftware.HomeAssistant.NetDaemon.Common;
using JoySoftware.HomeAssistant.NetDaemon.Common.Reactive;
using JoySoftware.HomeAssistant.NetDaemon.Daemon.Storage;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("NetDaemon.Daemon.Tests")]

namespace JoySoftware.HomeAssistant.NetDaemon.Daemon
{
    public class NetDaemonHost : INetDaemonHost, IAsyncDisposable
    {
        internal readonly ConcurrentDictionary<string, HassArea> _hassAreas =
            new ConcurrentDictionary<string, HassArea>();

        // Internal for test
        internal readonly ConcurrentDictionary<string, HassDevice> _hassDevices =
            new ConcurrentDictionary<string, HassDevice>();

        internal readonly ConcurrentDictionary<string, HassEntity> _hassEntities =
            new ConcurrentDictionary<string, HassEntity>();

        internal readonly Channel<(string, string, dynamic?)> _serviceCallMessageChannel =
                Channel.CreateBounded<(string, string, dynamic?)>(20);

        internal readonly Channel<(string, dynamic, dynamic?)> _setStateMessageChannel =
                Channel.CreateBounded<(string, dynamic, dynamic?)>(20);

        internal readonly Channel<(string, string)> _ttsMessageChannel =
                                                    Channel.CreateBounded<(string, string)>(20);

        // Used for testing
        internal int InternalDelayTimeForTts = 2500;

        // internal so we can use for unittest
        internal ConcurrentDictionary<string, EntityState> InternalState = new ConcurrentDictionary<string, EntityState>();

        private readonly IInstanceDaemonApp _appInstanceManager;

        // Internal token source for just cancel this objects activities
        private readonly CancellationTokenSource _cancelDaemon = new CancellationTokenSource();

        private readonly ConcurrentBag<(string, string, Func<dynamic?, Task>)> _daemonServiceCallFunctions
                            = new ConcurrentBag<(string, string, Func<dynamic?, Task>)>();

        /// <summary>
        ///     Currently running tasks for handling new events from HomeAssistant
        /// </summary>
        private readonly List<Task> _eventHandlerTasks = new List<Task>();

        private readonly IHassClient _hassClient;

        private readonly IHttpHandler? _httpHandler;

        private readonly IDataRepository? _repository;

        private readonly ConcurrentDictionary<string, INetDaemonAppBase> _runningAppInstances =
            new ConcurrentDictionary<string, INetDaemonAppBase>();

        private readonly Scheduler _scheduler;

        private readonly List<string> _supportedDomainsForTurnOnOff = new List<string>
        {
            "light",
            "switch",
            "input_boolean",
            "automation",
            "input_boolean",
            "camera",
            "scene",
            "script",
        };

        // Following token source and token are set at RUN
        private CancellationToken _cancelToken;

        private CancellationTokenSource? _cancelTokenSource;
        private IDictionary<string, object> _dataCache = new Dictionary<string, object>();

        private bool _stopped;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="hassClient">Client to use</param>
        /// <param name="repository">Repository to use</param>
        /// <param name="loggerFactory">The loggerfactory</param>
        /// <param name="httpHandler">Http handler to use</param>
        /// <param name="appInstanceManager">Handles instances of apps</param>
        public NetDaemonHost(
            IInstanceDaemonApp appInstanceManager,
            IHassClient? hassClient,
            IDataRepository? repository,
            ILoggerFactory? loggerFactory = null,
            IHttpHandler? httpHandler = null
            )
        {
            loggerFactory ??= DefaultLoggerFactory;
            _httpHandler = httpHandler;
            Logger = loggerFactory.CreateLogger<NetDaemonHost>();
            _hassClient = hassClient ?? throw new ArgumentNullException("HassClient can't be null!");
            _scheduler = new Scheduler(loggerFactory: loggerFactory);
            _repository = repository;
            _appInstanceManager = appInstanceManager;
            Logger.LogInformation("Instance NetDaemonHost");
        }

        public bool Connected { get; private set; }

        public IHttpHandler Http
        {
            get
            {
                _ = _httpHandler ?? throw new NullReferenceException("HttpHandler can not be null!");
                return _httpHandler;
            }
        }

        public ILogger Logger { get; }

        public IEnumerable<INetDaemonAppBase> RunningAppInstances => _runningAppInstances.Values;

        public IScheduler Scheduler => _scheduler;

        public IEnumerable<EntityState> State => InternalState.Select(n => n.Value);

        // For testing
        internal ConcurrentDictionary<string, INetDaemonAppBase> InternalRunningAppInstances => _runningAppInstances;

        private static ILoggerFactory DefaultLoggerFactory => LoggerFactory.Create(builder =>
                                        {
                                            builder
                                                .ClearProviders()
                                                .AddConsole();
                                        });

        public void CallService(string domain, string service, dynamic? data = null)
        {
            this._cancelToken.ThrowIfCancellationRequested();

            if (_serviceCallMessageChannel.Writer.TryWrite((domain, service, data)) == false)
                throw new ApplicationException("Servicecall queue full!");
        }

        public async Task CallServiceAsync(string domain, string service, dynamic? data = null, bool waitForResponse = false)
        {
            this._cancelToken.ThrowIfCancellationRequested();

            try
            {
                await _hassClient.CallService(domain, service, data, false).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Failed call service");
            }
        }

        /// <inheritdoc/>
        public ICamera Camera(INetDaemonApp app, params string[] entityIds)
        {
            this._cancelToken.ThrowIfCancellationRequested();
            return new CameraManager(entityIds, this, app);
        }

        /// <inheritdoc/>
        public ICamera Cameras(INetDaemonApp app, IEnumerable<string> entityIds)
        {
            this._cancelToken.ThrowIfCancellationRequested();
            return new CameraManager(entityIds, this, app);
        }

        /// <inheritdoc/>
        public ICamera Cameras(INetDaemonApp app, Func<IEntityProperties, bool> func)
        {
            this._cancelToken.ThrowIfCancellationRequested();
            try
            {
                IEnumerable<IEntityProperties> x = State.Where(func);

                return new CameraManager(x.Select(n => n.EntityId).ToArray(), this, app);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Failed to select camera func in app {appId}", app.Id);
                throw;
            }
        }

        public async ValueTask DisposeAsync()
        {
            _cancelDaemon.Cancel();
            await Stop().ConfigureAwait(false);
            Logger.LogTrace("Instance NetDaemonHost Disposed");
        }

        public void EnableApplicationDiscoveryServiceAsync()
        {
            // For service call reload_apps we do just that... reload the fucking apps yay :)
            ListenCompanionServiceCall("reload_apps", async (_) => await ReloadAllApps());

            RegisterAppSwitchesAndTheirStates();
        }

        public IEntity Entities(INetDaemonApp app, Func<IEntityProperties, bool> func)
        {
            this._cancelToken.ThrowIfCancellationRequested();

            try
            {
                IEnumerable<IEntityProperties> x = State.Where(func);
                var selectedEntities = x.Select(n => n.EntityId).ToArray();
                return new EntityManager(selectedEntities, this, app);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Failed to select entities using func in app {appId}", app.Id);
                throw;
            }
        }

        public IEntity Entities(INetDaemonApp app, IEnumerable<string> entityIds)
        {
            this._cancelToken.ThrowIfCancellationRequested();
            return new EntityManager(entityIds, this, app);
        }

        public IEntity Entity(INetDaemonApp app, params string[] entityIds)
        {
            this._cancelToken.ThrowIfCancellationRequested();
            return new EntityManager(entityIds, this, app);
        }

        /// <inheritdoc/>
        public INetDaemonAppBase? GetApp(string appInstanceId)
        {
            this._cancelToken.ThrowIfCancellationRequested();

            return _runningAppInstances.ContainsKey(appInstanceId) ?
                _runningAppInstances[appInstanceId] : null;
        }

        public async ValueTask<T?> GetDataAsync<T>(string id) where T : class
        {
            this._cancelToken.ThrowIfCancellationRequested();

            _ = _repository as IDataRepository ??
              throw new NullReferenceException($"{nameof(_repository)} can not be null!");

            if (_dataCache.ContainsKey(id))
            {
                return (T)_dataCache[id];
            }
            var data = await _repository!.Get<T>(id).ConfigureAwait(false);

            if (data != null)
                _dataCache[id] = data;

            return data;
        }

        public EntityState? GetState(string entity)
        {
            this._cancelToken.ThrowIfCancellationRequested();

            return InternalState.TryGetValue(entity, out EntityState? returnValue)
                ? returnValue
                : null;
        }

        /// <inheritdoc/>
        public async Task Initialize()
        {
            if (!Connected)
                throw new ApplicationException("NetDaemon is not connected, no use in initializing");

            await LoadAllApps().ConfigureAwait(false);
            EnableApplicationDiscoveryServiceAsync();
        }

        /// <inheritdoc/>
        public IFluentInputSelect InputSelect(INetDaemonApp app, params string[] inputSelectParams)
        {
            this._cancelToken.ThrowIfCancellationRequested();
            return new InputSelectManager(inputSelectParams, this, app);
        }

        /// <inheritdoc/>
        public IFluentInputSelect InputSelects(INetDaemonApp app, IEnumerable<string> inputSelectParams)
        {
            this._cancelToken.ThrowIfCancellationRequested();
            return new InputSelectManager(inputSelectParams, this, app);
        }

        /// <inheritdoc/>
        public IFluentInputSelect InputSelects(INetDaemonApp app, Func<IEntityProperties, bool> func)
        {
            this._cancelToken.ThrowIfCancellationRequested();
            IEnumerable<string> x = State.Where(func).Select(n => n.EntityId);
            return new InputSelectManager(x, this, app);
        }

        /// <inheritdoc/>
        public void ListenCompanionServiceCall(string service, Func<dynamic?, Task> action)
        {
            this._cancelToken.ThrowIfCancellationRequested();
            _daemonServiceCallFunctions.Add(("netdaemon", service.ToLowerInvariant(), action));
        }

        public void ListenServiceCall(string domain, string service, Func<dynamic?, Task> action)
            => _daemonServiceCallFunctions.Add((domain.ToLowerInvariant(), service.ToLowerInvariant(), action));

        /// <inheritdoc/>
        public IMediaPlayer MediaPlayer(INetDaemonApp app, params string[] entityIds)
        {
            this._cancelToken.ThrowIfCancellationRequested();
            return new MediaPlayerManager(entityIds, this, app);
        }

        /// <inheritdoc/>
        public IMediaPlayer MediaPlayers(INetDaemonApp app, IEnumerable<string> entityIds)
        {
            this._cancelToken.ThrowIfCancellationRequested();
            return new MediaPlayerManager(entityIds, this, app);
        }

        /// <inheritdoc/>
        public IMediaPlayer MediaPlayers(INetDaemonApp app, Func<IEntityProperties, bool> func)
        {
            this._cancelToken.ThrowIfCancellationRequested();
            try
            {
                IEnumerable<IEntityProperties> x = State.Where(func);

                return new MediaPlayerManager(x.Select(n => n.EntityId).ToArray(), this, app);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Failed to select mediaplayers func in app {appId}", app.Id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task ReloadAllApps()
        {
            await UnloadAllApps().ConfigureAwait(false);
            await _scheduler.Restart().ConfigureAwait(false);
            await LoadAllApps().ConfigureAwait(false);
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
        public async Task Run(string host, short port, bool ssl, string token, CancellationToken cancellationToken)
        {
            // Create combine cancellation token
            _cancelTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_cancelDaemon.Token, cancellationToken);
            _cancelToken = _cancelTokenSource.Token;

            _cancelToken.ThrowIfCancellationRequested();

            string? hassioToken = Environment.GetEnvironmentVariable("HASSIO_TOKEN");

            if (_hassClient == null)
            {
                throw new NullReferenceException("HassClient cant be null when running daemon, check constructor!");
            }

            try
            {
                bool connectResult;

                if (hassioToken != null)
                {
                    // We are running as hassio add-on
                    connectResult = await _hassClient.ConnectAsync(new Uri("ws://supervisor/core/websocket"),
                        hassioToken, false).ConfigureAwait(false);
                }
                else
                {
                    connectResult = await _hassClient.ConnectAsync(host, port, ssl, token, false).ConfigureAwait(false);
                }

                if (!connectResult)
                {
                    Connected = false;
                    return;
                }

                // Setup TTS
                Task handleTextToSpeechMessagesTask = HandleTextToSpeechMessages(cancellationToken);
                Task handleAsyncServiceCalls = HandleAsyncServiceCalls(cancellationToken);
                Task hanldeAsyncSetState = HandleAsyncSetState(cancellationToken);

                await RefreshInternalStatesAndSetArea().ConfigureAwait(false);

                await _hassClient.SubscribeToEvents().ConfigureAwait(false);

                Connected = true;

                Logger.LogInformation(
                    hassioToken != null
                        ? "Successfully connected to Home Assistant Core in Home Assistant Add-on"
                        : "Successfully connected to Home Assistant Core on host {host}:{port}", host, port);

                while (!cancellationToken.IsCancellationRequested)
                {
                    HassEvent changedEvent = await _hassClient.ReadEventAsync(cancellationToken).ConfigureAwait(false);
                    if (changedEvent != null)
                    {
                        // Remove all completed Tasks
                        _eventHandlerTasks.RemoveAll(x => x.IsCompleted);
                        _eventHandlerTasks.Add(HandleNewEvent(changedEvent, cancellationToken));
                    }
                    else
                    {
                        // Will only happen when doing unit tests
                        await Task.Delay(1, cancellationToken).ConfigureAwait(false);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Normal
            }
            catch (Exception e)
            {
                Connected = false;
                Logger.LogError(e, "Error, during operation");
            }
        }

        public IScript RunScript(INetDaemonApp app, params string[] entityId)
        {
            this._cancelToken.ThrowIfCancellationRequested();
            return new EntityManager(entityId, this, app);
        }

        public Task SaveDataAsync<T>(string id, T data)
        {
            this._cancelToken.ThrowIfCancellationRequested();

            _ = _repository as IDataRepository ??
                throw new NullReferenceException($"{nameof(_repository)} can not be null!");

            if (data == null)
                throw new ArgumentNullException(nameof(data));

            _dataCache[id] = data;
            return _repository!.Save(id, data);
        }

        public async Task<bool> SendEvent(string eventId, dynamic? data = null)
        {
            this._cancelToken.ThrowIfCancellationRequested();
            if (!Connected)
                return false;

            try
            {
                return await _hassClient.SendEvent(eventId, data).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Logger.LogDebug(e, "Error sending event!");
            }
            return false;
        }

        /// <inheritdoc/>
        public async Task SetDaemonStateAsync(int numberOfLoadedApps, int numberOfRunningApps)
        {
            this._cancelToken.ThrowIfCancellationRequested();

            await SetStateAsync(
                "netdaemon.status",
                "Connected", // State will alawys be connected, otherwise state could not be set.
                ("number_of_loaded_apps", numberOfLoadedApps),
                ("number_of_running_apps", numberOfRunningApps),
                ("version", GetType().Assembly.GetName().Version?.ToString() ?? "N/A")).ConfigureAwait(false);
        }

        public void SetState(string entityId, dynamic state, dynamic? attributes = null)
        {
            this._cancelToken.ThrowIfCancellationRequested();

            if (_setStateMessageChannel.Writer.TryWrite((entityId, state, attributes)) == false)
                throw new ApplicationException("Servicecall queue full!");
        }

        public async Task<EntityState?> SetStateAsync(string entityId, dynamic state,
                    params (string name, object val)[] attributes)
        {
            this._cancelToken.ThrowIfCancellationRequested();
            try
            {
                // Use expando object as all other methods
                dynamic dynAttributes = attributes.ToDynamic();

                HassState result = await _hassClient.SetState(entityId, state.ToString(), dynAttributes).ConfigureAwait(false);

                if (result != null)
                {
                    EntityState entityState = result.ToDaemonEntityState();
                    entityState.Area = GetAreaForEntityId(entityState.EntityId);
                    InternalState[entityState.EntityId] = entityState;
                    return entityState;
                }

                return null;
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Failed to set state for entity {entityId}", entityId);
                throw;
            }
        }

        public void Speak(string entityId, string message)
        {
            this._cancelToken.ThrowIfCancellationRequested();

            _ttsMessageChannel.Writer.TryWrite((entityId, message));
        }

        public async Task Stop()
        {
            try
            {
                if (_stopped)
                {
                    return;
                }
                Logger.LogInformation("Try stopping Instance NetDaemonHost");

                await UnloadAllApps().ConfigureAwait(false);

                await _scheduler.Stop().ConfigureAwait(false);

                await _hassClient.CloseAsync().ConfigureAwait(false);

                InternalState.Clear();

                _stopped = true;

                Logger.LogInformation("Stopped Instance NetDaemonHost");
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error stopping NetDaemon");
            }
        }

        /// <inheritdoc/>
        public async Task UnloadAllApps()
        {
            if (_runningAppInstances is null || _runningAppInstances.Count() == 0)
                return;

            foreach (var app in _runningAppInstances)
            {
                await app.Value.DisposeAsync().ConfigureAwait(false);
            }
            _runningAppInstances.Clear();
        }

        /// <summary>
        ///     Fixes the type differences that can be from Home Assistant depending on
        ///     different conditions
        /// </summary>
        /// <param name="stateData">The state data to be fixed</param>
        /// <remarks>
        ///     If a sensor is unavailable that normally has a primtive value
        ///     it can be a string. The automations might expect a integer.
        ///     Another scenario is that a value of 10 is cast as long and
        ///     next time a value of 11.3 is cast to double. T
        ///     FixStateTypes fixes these problem by casting correct types
        ///     or setting null. It returns false if the casts can not be
        ///     managed.
        /// </remarks>
        internal static bool FixStateTypes(HassStateChangedEventData stateData)
        {
            // NewState and OldState can not be null, something is seriously wrong
            if (stateData.NewState is null || stateData.OldState is null)
                return false;

            // Both states can not be null, something is seriously wrong
            if (stateData.NewState.State is null && stateData.OldState.State is null)
                return false;

            if (stateData.NewState.State is object && stateData.OldState.State is object)
            {
                Type? newStateType = stateData.NewState?.State?.GetType();
                Type? oldStateType = stateData.OldState?.State?.GetType();

                if (newStateType != oldStateType)
                {
                    // We have a potential problem with unavailable or unknown entity state
                    // Lets start checking that
                    if (newStateType == typeof(string) || oldStateType == typeof(string))
                    {
                        // We have a statechange to or from string, just ignore for now and set the string to null
                        // Todo: Implement a bool that tells that the change are unavailable
                        if (newStateType == typeof(string))
                            stateData!.NewState!.State = null;
                        else
                            stateData!.OldState!.State = null;
                    }
                    else if (newStateType == typeof(double) || oldStateType == typeof(double))
                    {
                        if (newStateType == typeof(double))
                        {
                            // Try convert the integer to double
                            if (oldStateType == typeof(long))
                                stateData!.OldState!.State = Convert.ToDouble(stateData!.NewState!.State);
                            else
                                return false; // We do not support any other conversion
                        }
                        else
                        {
                            // Try convert the long to double
                            if (newStateType == typeof(long))
                                stateData!.NewState!.State = Convert.ToDouble(stateData!.NewState!.State);
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

        // public void StopDaemonActivities()
        // {
        //     // Do some stuff to clear memory
        //     // this is not nescessary now when
        //     // we do not do hot-reload for recompile
        //     // I keep this in-case we go that route
        //     _hassAreas.Clear();
        //     _hassDevices.Clear();
        //     _hassEntities.Clear();
        //     _runningAppInstances.Clear();
        //     _daemonServiceCallFunctions.Clear();
        //     _eventHandlerTasks.Clear();
        //     _dataCache.Clear();
        //     GC.Collect();
        //     GC.WaitForPendingFinalizers();
        // }
        internal string? GetAreaForEntityId(string entityId)
        {
            HassEntity? entity;
            if (_hassEntities.TryGetValue(entityId, out entity) && entity is object)
            {
                if (entity.DeviceId is object)
                {
                    // The entity is on a device
                    HassDevice? device;
                    if (_hassDevices.TryGetValue(entity.DeviceId, out device) && device is object)
                    {
                        if (device.AreaId is object)
                        {
                            // This device is in an area
                            HassArea? area;
                            if (_hassAreas.TryGetValue(device.AreaId, out area) && area is object)
                            {
                                return area.Name;
                            }
                        }
                    }
                }
            }
            return null;
        }

        internal async Task RefreshInternalStatesAndSetArea()
        {
            this._cancelToken.ThrowIfCancellationRequested();

            foreach (var device in await _hassClient.GetDevices().ConfigureAwait(false))
            {
                if (device is object && device.Id is object)
                    _hassDevices[device.Id] = device;
            }
            foreach (var area in await _hassClient.GetAreas().ConfigureAwait(false))
            {
                if (area is object && area.Id is object)
                    _hassAreas[area.Id] = area;
            }
            foreach (var entity in await _hassClient.GetEntities().ConfigureAwait(false))
            {
                if (entity is object && entity.EntityId is object)
                    _hassEntities[entity.EntityId] = entity;
            }
            var hassStates = await _hassClient.GetAllStates(_cancelToken).ConfigureAwait(false);
            var initialStates = hassStates.Select(n => n.ToDaemonEntityState())
                .ToDictionary(n => n.EntityId);

            InternalState.Clear();
            foreach (var key in initialStates.Keys)
            {
                var state = initialStates[key];
                state.Area = GetAreaForEntityId(state.EntityId);
                InternalState[key] = state;
            }
        }

        internal IList<INetDaemonAppBase> SortByDependency(IEnumerable<INetDaemonAppBase> unsortedList)
        {
            if (unsortedList.SelectMany(n => n.Dependencies).Count() > 0)
            {
                // There are dependecies defined
                var edges = new HashSet<Tuple<INetDaemonAppBase, INetDaemonAppBase>>();

                foreach (var instance in unsortedList)
                {
                    foreach (var dependency in instance.Dependencies)
                    {
                        var dependentApp = unsortedList.Where(n => n.Id == dependency).FirstOrDefault();
                        if (dependentApp == null)
                            throw new ApplicationException($"There is no app named {dependency}, please check dependencies or make sure you have not disabled the dependent app!");

                        edges.Add(new Tuple<INetDaemonAppBase, INetDaemonAppBase>(instance, dependentApp));
                    }
                }
                var sortedInstances = TopologicalSort<INetDaemonAppBase>(unsortedList.ToHashSet(), edges) ??
                    throw new ApplicationException("Application dependencies is wrong, please check dependencies for circular dependencies!");

                return sortedInstances;
            }
            return unsortedList.ToList();
        }

        protected virtual async Task HandleNewEvent(HassEvent hassEvent, CancellationToken token)
        {
            this._cancelToken.ThrowIfCancellationRequested();

            if (hassEvent.EventType == "state_changed")
            {
                try
                {
                    var stateData = (HassStateChangedEventData?)hassEvent.Data;

                    if (stateData is null)
                    {
                        throw new NullReferenceException("StateData is null!");
                    }

                    if (stateData.NewState is null || stateData.OldState is null)
                    {
                        // This is an entity that is removed and have no new state so just return;
                        return;
                    }

                    if (!FixStateTypes(stateData))
                    {
                        if (stateData?.NewState?.State != stateData?.OldState?.State)
                        {
                            var sb = new StringBuilder();
                            sb.AppendLine($"Can not fix state typing for {stateData?.NewState?.EntityId}");
                            sb.AppendLine($"NewStateObject: {stateData?.NewState}");
                            sb.AppendLine($"OldStateObject: {stateData?.OldState}");
                            sb.AppendLine($"NewState: {stateData?.NewState?.State}");
                            sb.AppendLine($"OldState: {stateData?.OldState?.State}");
                            sb.AppendLine($"NewState type: {stateData?.NewState?.State?.GetType().ToString() ?? "null"}");
                            sb.AppendLine($"OldState type: {stateData?.OldState?.State?.GetType().ToString() ?? "null"}");
                            Logger.LogTrace(sb.ToString());
                        }
                        return;
                    }

                    // Make sure we get the area name with the new state
                    var newState = stateData!.NewState!.ToDaemonEntityState();
                    var oldState = stateData!.OldState!.ToDaemonEntityState();
                    newState.Area = GetAreaForEntityId(newState.EntityId);
                    InternalState[stateData.EntityId] = newState;

                    var tasks = new List<Task>();
                    foreach (var app in _runningAppInstances)
                    {
                        if (app.Value is NetDaemonApp netDaemonApp)
                        {
                            foreach ((string pattern, Func<string, EntityState?, EntityState?, Task> func) in netDaemonApp.StateCallbacks.Values)
                            {
                                if (string.IsNullOrEmpty(pattern))
                                {
                                    tasks.Add(func(stateData.EntityId,
                                        newState,
                                        oldState
                                    ));
                                }
                                else if (stateData.EntityId.StartsWith(pattern))
                                {
                                    tasks.Add(func(stateData.EntityId,
                                        newState,
                                        oldState
                                    ));
                                }
                            }
                        }
                        else if (app.Value is NetDaemonRxApp netDaemonRxApp)
                        {
                            // Call the observable with no blocking
                            foreach (var observer in ((StateChangeObservable)netDaemonRxApp.StateChangesObservable).Observers)
                            {
                                tasks.Add(Task.Run(() =>
                                {
                                    try
                                    {
                                        observer.OnNext((oldState, newState));
                                    }
                                    catch (Exception e)
                                    {
                                        observer.OnError(e);
                                        Logger.LogError(e, $"Fail to OnNext on state change observer. {newState.EntityId}:{newState?.State}({oldState?.State})");
                                    }
                                }));
                            }
                        }
                    }

                    // No hit
                    // Todo: Make it timeout! Maybe it should be handling in it's own task like scheduler
                    if (tasks.Count > 0)
                    {
                        await tasks.WhenAll(token).ConfigureAwait(false);
                    }
                }
                catch (Exception e)
                {
                    Logger.LogError(e, "Failed to handle new event (state_changed)");
                }
            }
            else if (hassEvent.EventType == "call_service")
            {
                try
                {
                    var serviceCallData = (HassServiceEventData?)hassEvent.Data;

                    if (serviceCallData == null)
                    {
                        throw new NullReferenceException("ServiceData is null! not expected");
                    }
                    var tasks = new List<Task>();
                    foreach (var app in _runningAppInstances)
                    {
                        if (app.Value is NetDaemonApp netDaemonApp)
                        {
                            foreach (var (domain, service, func) in netDaemonApp.DaemonCallBacksForServiceCalls)
                            {
                                if (domain == serviceCallData.Domain &&
                                    service == serviceCallData.Service)
                                {
                                    tasks.Add(func(serviceCallData.Data));
                                }
                            }
                        }
                        else if (app.Value is NetDaemonRxApp netDaemonRxApp)
                        {
                            // Call the observable with no blocking
                            foreach (var observer in ((EventObservable)netDaemonRxApp.EventChangesObservable).Observers)
                            {
                                tasks.Add(Task.Run(() =>
                                {
                                    try
                                    {
                                        var rxEvent = new RxEvent(serviceCallData.Service, serviceCallData.Domain,
                                                                 serviceCallData.ServiceData);
                                        observer.OnNext(rxEvent);
                                    }
                                    catch (Exception e)
                                    {
                                        observer.OnError(e);
                                        Logger.LogError(e, "Fail to OnNext on event observer (service_call)");
                                    }
                                }));
                            }
                        }
                    }

                    foreach (var (domain, service, func) in _daemonServiceCallFunctions)
                    {
                        if (domain == serviceCallData.Domain &&
                            service == serviceCallData.Service)
                        {
                            tasks.Add(func(serviceCallData.Data));
                        }
                    }

                    if (tasks.Count > 0)
                    {
                        await tasks.WhenAll(token).ConfigureAwait(false);
                    }
                }
                catch (Exception e)
                {
                    Logger.LogError(e, "Failed to handle new event (service_call)");
                }
            }
            else if (hassEvent.EventType == "device_registry_updated" || hassEvent.EventType == "area_registry_updated")
            {
                try
                {
                    await RefreshInternalStatesAndSetArea().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Logger.LogError(e, "Failed to RefreshInternalStatesAndSetArea");
                }
            }
            else
            {
                try
                {
                    var tasks = new List<Task>();
                    foreach (var app in _runningAppInstances)
                    {
                        if (app.Value is NetDaemonApp netDaemonApp)
                        {
                            foreach ((string ev, Func<string, dynamic, Task> func) in netDaemonApp.EventCallbacks)
                            {
                                if (ev == hassEvent.EventType)
                                {
                                    tasks.Add(func(ev, hassEvent.Data));
                                }
                            }
                            foreach ((Func<FluentEventProperty, bool> selectFunc, Func<string, dynamic, Task> func) in netDaemonApp.EventFunctionCallbacks)
                            {
                                if (selectFunc(new FluentEventProperty { EventId = hassEvent.EventType, Data = hassEvent.Data }))
                                {
                                    tasks.Add(func(hassEvent.EventType, hassEvent.Data));
                                }
                            }
                        }
                        else if (app.Value is NetDaemonRxApp netDaemonRxApp)
                        {
                            // Call the observable with no blocking
                            foreach (var observer in ((EventObservable)netDaemonRxApp.EventChangesObservable).Observers)
                            {
                                tasks.Add(Task.Run(() =>
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
                                }));
                            }
                        }
                    }
                    if (tasks.Count > 0)
                    {
                        await tasks.WhenAll(token).ConfigureAwait(false);
                    }
                }
                catch (Exception e)
                {
                    Logger.LogError(e, "Failed to handle new event (custom_event)");
                }
            }
        }

        private static string GetDomainFromEntity(string entity)
        {
            string[] entityParts = entity.Split('.');
            if (entityParts.Length != 2)
            {
                throw new ApplicationException($"entity_id is mal formatted {entity}");
            }

            return entityParts[0];
        }

        /// <summary>
        /// Topological Sorting (Kahn's algorithm)
        /// </summary>
        /// <remarks>https://en.wikipedia.org/wiki/Topological_sorting</remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="nodes">All nodes of directed acyclic graph.</param>
        /// <param name="edges">All edges of directed acyclic graph.</param>
        /// <returns>Sorted node in topological order.</returns>
        private static List<T>? TopologicalSort<T>(HashSet<T> nodes, HashSet<Tuple<T, T>> edges) where T : IEquatable<T>
        {
            // Empty list that will contain the sorted elements
            var L = new List<T>();

            // Set of all nodes with no incoming edges
            var S = new HashSet<T>(nodes.Where(n => edges.All(e => e.Item2.Equals(n) == false)));

            // while S is non-empty do
            while (S.Any())
            {
                //  remove a node n from S
                var n = S.First();
                S.Remove(n);

                // add n to tail of L
                L.Add(n);

                // for each node m with an edge e from n to m do
                foreach (var e in edges.Where(e => e.Item1.Equals(n)).ToList())
                {
                    var m = e.Item2;

                    // remove edge e from the graph
                    edges.Remove(e);

                    // if m has no other incoming edges then
                    if (edges.All(me => me.Item2.Equals(m) == false))
                    {
                        // insert m into S
                        S.Add(m);
                    }
                }
            }

            // if graph has edges then
            if (edges.Any())
            {
                // return error (graph has at least one cycle)
                return null;
            }
            else
            {
                L.Reverse();
                // return L (a topologically sorted order)
                return L;
            }
        }

        private async Task HandleAsyncServiceCalls(CancellationToken cancellationToken)
        {
            this._cancelToken.ThrowIfCancellationRequested();

            bool hasLoggedError = false;

            //_serviceCallMessageQueue
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    (string domain, string service, dynamic? data)
                    = await _serviceCallMessageChannel.Reader.ReadAsync(cancellationToken).ConfigureAwait(false);

                    await _hassClient.CallService(domain, service, data, false).ConfigureAwait(false); ;

                    hasLoggedError = false;
                }
                catch (OperationCanceledException)
                {
                    // Ignore we are leaving
                }
                catch (Exception e)
                {
                    if (hasLoggedError == false)
                        Logger.LogDebug(e, "Failure sending call service");
                    hasLoggedError = true;
                    await Task.Delay(100); // Do a delay to avoid loop
                }
            }
        }

        private async Task HandleAsyncSetState(CancellationToken cancellationToken)
        {
            this._cancelToken.ThrowIfCancellationRequested();
            bool hasLoggedError = false;

            //_serviceCallMessageQueue
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    (string entityId, dynamic state, dynamic? attributes)
                    = await _setStateMessageChannel.Reader.ReadAsync(cancellationToken).ConfigureAwait(false);

                    await _hassClient.SetState(entityId, state, attributes).ConfigureAwait(false);

                    hasLoggedError = false;
                }
                catch (OperationCanceledException)
                {
                    // Ignore we are leaving
                }
                catch (Exception e)
                {
                    if (hasLoggedError == false)
                        Logger.LogDebug(e, "Failure setting state");
                    hasLoggedError = true;
                    await Task.Delay(100); // Do a delay to avoid loop
                }
            }
        }

        private async Task HandleTextToSpeechMessages(CancellationToken cancellationToken)
        {
            this._cancelToken.ThrowIfCancellationRequested();
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    (string entityId, string message) = await _ttsMessageChannel.Reader.ReadAsync(cancellationToken).ConfigureAwait(false);

                    dynamic attributes = new ExpandoObject();
                    attributes.entity_id = entityId;
                    attributes.message = message;
                    await _hassClient.CallService("tts", "google_cloud_say", attributes, true).ConfigureAwait(false);
                    await Task.Delay(InternalDelayTimeForTts).ConfigureAwait(false); // Wait 2 seconds to wait for status to complete

                    EntityState? currentPlayState = GetState(entityId);

                    if (currentPlayState != null && currentPlayState.Attribute?.media_duration != null)
                    {
                        int delayInMilliSeconds = (int)Math.Round(currentPlayState?.Attribute?.media_duration * 1000) - InternalDelayTimeForTts;

                        if (delayInMilliSeconds > 0)
                        {
                            await Task.Delay(delayInMilliSeconds).ConfigureAwait(false); // Wait remainder of text message
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Do nothing it should be normal
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error reading TTS channel");
            }
        }

        /// <inheritdoc/>
        private async Task LoadAllApps()
        {
            // First unload any apps running
            await UnloadAllApps().ConfigureAwait(false);

            // Get all instances
            var instancedApps = _appInstanceManager.InstanceDaemonApps();

            if (_runningAppInstances.Count() > 0)
                throw new ApplicationException("Did not expect running instances!");

            foreach (INetDaemonAppBase appInstance in instancedApps!)
            {
                if (await RestoreAppState(appInstance).ConfigureAwait(false))
                {
                    _runningAppInstances[appInstance.Id!] = appInstance;
                }
            }

            // Now run initialize on all sorted by dependencies
            foreach (var sortedApp in SortByDependency(_runningAppInstances.Values))
            {
                // Init by calling the InitializeAsync
                var taskInitAsync = sortedApp.InitializeAsync();
                var taskAwaitedAsyncTask = await Task.WhenAny(taskInitAsync, Task.Delay(5000)).ConfigureAwait(false);
                if (taskAwaitedAsyncTask != taskInitAsync)
                    Logger.LogWarning("InitializeAsync of application {app} took longer that 5 seconds, make sure InitializeAsync is not blocking!", sortedApp.Id);

                // Init by calling the Initialize
                var taskInit = Task.Run(sortedApp.Initialize);
                var taskAwaitedTask = await Task.WhenAny(taskInit, Task.Delay(5000)).ConfigureAwait(false);
                if (taskAwaitedTask != taskInit)
                    Logger.LogWarning("Initialize of application {app} took longer that 5 seconds, make sure Initialize function is not blocking!", sortedApp.Id);

                // Todo: refactor
                await sortedApp.HandleAttributeInitialization(this);
                Logger.LogInformation("Successfully loaded app {appId} ({class})", sortedApp.Id, sortedApp.GetType().Name);
            }

            await SetDaemonStateAsync(_appInstanceManager.Count, _runningAppInstances.Count).ConfigureAwait(false);
        }

        private void RegisterAppSwitchesAndTheirStates()
        {
            ListenServiceCall("switch", "turn_on", async (data) =>
            {
                await SetStateOnDaemonAppSwitch("on", data).ConfigureAwait(false);
            });

            ListenServiceCall("switch", "turn_off", async (data) =>
            {
                await SetStateOnDaemonAppSwitch("off", data).ConfigureAwait(false);
            });

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
                catch (System.Exception e)
                {
                    Logger.LogWarning(e, "Failed to set state from netdaemon switch");
                }
            });

            async Task SetStateOnDaemonAppSwitch(string state, dynamic? data)
            {
                string? entityId = data?.entity_id;
                if (entityId is null)
                    return;

                if (!entityId.StartsWith("switch.netdaemon_"))
                    return; // We only want app switches

                List<(string, object)>? attributes = null;

                var entityAttributes = GetState(entityId)?.Attribute as IDictionary<string, object>;

                if (entityAttributes is object)
                    attributes = entityAttributes.Keys.Select(n => (n, entityAttributes[n])).ToList();

                if (attributes is object)
                    await SetStateAsync(entityId, state, attributes.ToArray()).ConfigureAwait(false);
                else
                    await SetStateAsync(entityId, state).ConfigureAwait(false);
            }
        }

        private async Task<bool> RestoreAppState(INetDaemonAppBase appInstance)
        {
            try
            {
                // First do startup initialization to connect with this daemon instance
                await appInstance.StartUpAsync(this).ConfigureAwait(false);
                // The restore the state to load saved settings and if this app is enabled
                await appInstance.RestoreAppStateAsync().ConfigureAwait(false);

                if (!appInstance.IsEnabled)
                {
                    // We should not initialize this app, so dispose it and return
                    await appInstance.DisposeAsync().ConfigureAwait(false);
                    return false;
                }

                return true;
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Failed to load app {appInstance.Id}");
            }
            // Error return false
            return false;
        }
    }
}