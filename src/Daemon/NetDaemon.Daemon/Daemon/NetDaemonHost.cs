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

        private readonly List<(string, string, Func<dynamic?, Task>)> _daemonServiceCallFunctions
            = new List<(string, string, Func<dynamic?, Task>)>();

        /// <summary>
        ///     Currently running tasks for handling new events from HomeAssistant
        /// </summary>
        private readonly List<Task> _eventHandlerTasks = new List<Task>();

        private readonly EventObservable _eventObservables = new EventObservable();

        private readonly IHassClient _hassClient;

        private readonly IHttpHandler? _httpHandler;

        private readonly IDataRepository? _repository;

        private readonly ConcurrentDictionary<string, NetDaemonApp> _runningAppInstances =
            new ConcurrentDictionary<string, NetDaemonApp>();

        private readonly Scheduler _scheduler;

        private readonly StateChangeObservable _stateObservables = new StateChangeObservable();

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

        private CancellationToken _cancelToken;

        private IDictionary<string, object> _dataCache = new Dictionary<string, object>();

        private bool _stopped;

        public NetDaemonHost(
            IHassClient? hassClient,
            IDataRepository? repository,
            ILoggerFactory? loggerFactory = null,
            IHttpHandler? httpHandler = null)
        {
            loggerFactory ??= DefaultLoggerFactory;
            _httpHandler = httpHandler;
            Logger = loggerFactory.CreateLogger<NetDaemonHost>();
            _hassClient = hassClient ?? throw new ArgumentNullException("HassClient can't be null!");
            _scheduler = new Scheduler(loggerFactory: loggerFactory);
            _repository = repository;

            Logger.LogInformation("Instance NetDaemonHost");
        }

        public bool Connected { get; private set; }

        public IRxEvent EventChanges => _eventObservables;

        public IHttpHandler Http
        {
            get
            {
                _ = _httpHandler ?? throw new NullReferenceException("HttpHandler can not be null!");
                return _httpHandler;
            }
        }

        public ILogger Logger { get; }

        public IScheduler Scheduler => _scheduler;

        public IEnumerable<EntityState> State => InternalState.Select(n => n.Value);

        public IRxStateChange StateChanges => _stateObservables;

        // For testing
        internal ConcurrentDictionary<string, NetDaemonApp> InternalRunningAppInstances => _runningAppInstances;

        private static ILoggerFactory DefaultLoggerFactory => LoggerFactory.Create(builder =>
                                {
                                    builder
                                        .ClearProviders()
                                        .AddConsole();
                                });

        public void CallService(string domain, string service, dynamic? data = null)
        {
            if (_serviceCallMessageChannel.Writer.TryWrite((domain, service, data)) == false)
                throw new ApplicationException("Servicecall queue full!");
        }

        public async Task CallServiceAsync(string domain, string service, dynamic? data = null, bool waitForResponse = false)
        {
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
        public ICamera Camera(INetDaemonApp app, params string[] entityIds) => new CameraManager(entityIds, this, app);

        /// <inheritdoc/>
        public ICamera Cameras(INetDaemonApp app, IEnumerable<string> entityIds) => new CameraManager(entityIds, this, app);

        /// <inheritdoc/>
        public ICamera Cameras(INetDaemonApp app, Func<IEntityProperties, bool> func)
        {
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

        /// <inheritdoc/>
        public void ClearAppInstances()
        {
            _runningAppInstances.Clear();
        }

        public async ValueTask DisposeAsync()
        {
            Logger.LogInformation("Disposing Instance NetDaemonHost");
            await Stop().ConfigureAwait(false);
        }

        public IEntity Entities(INetDaemonApp app, Func<IEntityProperties, bool> func)
        {
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

        public IEntity Entities(INetDaemonApp app, IEnumerable<string> entityIds) => new EntityManager(entityIds, this, app);

        public IEntity Entity(INetDaemonApp app, params string[] entityIds) => new EntityManager(entityIds, this, app);

        /// <inheritdoc/>
        public NetDaemonApp? GetApp(string appInstanceId)
        {
            return _runningAppInstances.ContainsKey(appInstanceId) ?
                _runningAppInstances[appInstanceId] : null;
        }

        public async ValueTask<T> GetDataAsync<T>(string id)
        {
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
            return InternalState.TryGetValue(entity, out EntityState? returnValue)
                ? returnValue
                : null;
        }

        /// <inheritdoc/>
        public IFluentInputSelect InputSelect(INetDaemonApp app, params string[] inputSelectParams) =>
            new InputSelectManager(inputSelectParams, this, app);

        /// <inheritdoc/>
        public IFluentInputSelect InputSelects(INetDaemonApp app, IEnumerable<string> inputSelectParams) =>
            new InputSelectManager(inputSelectParams, this, app);

        /// <inheritdoc/>
        public IFluentInputSelect InputSelects(INetDaemonApp app, Func<IEntityProperties, bool> func)
        {
            IEnumerable<string> x = State.Where(func).Select(n => n.EntityId);
            return new InputSelectManager(x, this, app);
        }

        /// <inheritdoc/>
        public void ListenCompanionServiceCall(string service, Func<dynamic?, Task> action)
            => _daemonServiceCallFunctions.Add(("netdaemon", service.ToLowerInvariant(), action));

        public void ListenServiceCall(string domain, string service, Func<dynamic?, Task> action)
                                                                                                                                                                                                                                                                                                                                    => _daemonServiceCallFunctions.Add((domain.ToLowerInvariant(), service.ToLowerInvariant(), action));
        /// <inheritdoc/>
        public IMediaPlayer MediaPlayer(INetDaemonApp app, params string[] entityIds) => new MediaPlayerManager(entityIds, this, app);

        /// <inheritdoc/>
        public IMediaPlayer MediaPlayers(INetDaemonApp app, IEnumerable<string> entityIds) => new MediaPlayerManager(entityIds, this, app);

        /// <inheritdoc/>
        public IMediaPlayer MediaPlayers(INetDaemonApp app, Func<IEntityProperties, bool> func)
        {
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
        public void RegisterAppInstance(string appInstance, NetDaemonApp app)
        {
            _runningAppInstances[appInstance] = app;
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
            _cancelToken = cancellationToken;

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

        public IScript RunScript(INetDaemonApp app, params string[] entityId) => new EntityManager(entityId, this, app);

        public Task SaveDataAsync<T>(string id, T data)
        {
            _ = _repository as IDataRepository ??
                throw new NullReferenceException($"{nameof(_repository)} can not be null!");

            if (data == null)
                throw new ArgumentNullException(nameof(data));

            _dataCache[id] = data;
            return _repository!.Save(id, data);
        }

        public async Task<bool> SendEvent(string eventId, dynamic? data = null)
        {
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
            await SetStateAsync(
                "netdaemon.status",
                "Connected", // State will alawys be connected, otherwise state could not be set.
                ("number_of_loaded_apps", numberOfLoadedApps),
                ("number_of_running_apps", numberOfRunningApps),
                ("version", GetType().Assembly.GetName().Version?.ToString() ?? "N/A")).ConfigureAwait(false);
        }

        public void SetState(string entityId, dynamic state, dynamic? attributes = null)
        {
            if (_setStateMessageChannel.Writer.TryWrite((entityId, state, attributes)) == false)
                throw new ApplicationException("Servicecall queue full!");
        }

        public async Task<EntityState?> SetStateAsync(string entityId, dynamic state,
                    params (string name, object val)[] attributes)
        {
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

        public void Speak(string entityId, string message) => _ttsMessageChannel.Writer.TryWrite((entityId, message));

        public async Task Stop()
        {
            try
            {
                StopDaemonActivities();
                Logger.LogInformation("Try stopping Instance NetDaemonHost");
                if (_stopped)
                {
                    return;
                }

                StopDaemonActivities();

                await _scheduler.Stop().ConfigureAwait(false);

                await _hassClient.CloseAsync().ConfigureAwait(false);

                InternalState.Clear();
                // InternalStateActions.Clear();

                _stopped = true;

                // Do a hard collect here to free resource
                GC.Collect();
                GC.WaitForPendingFinalizers();
                Logger.LogInformation("Stopped Instance NetDaemonHost");
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error stopping NetDaemon");
            }
        }

        public void StopDaemonActivities()
        {
            foreach (var eventObservable in _eventObservables.Observers)
            {
                try
                {
                    eventObservable.OnCompleted();
                }
                catch (Exception e)
                {
                    Logger.LogWarning(e, "Error complete the event observables");
                }
            }

            foreach (var stateObservable in _stateObservables.Observers)
                try
                {
                    stateObservable.OnCompleted();
                }
                catch (Exception e)
                {
                    Logger.LogWarning(e, "Error complete the event observables");
                }

            _eventObservables.Clear();
            _stateObservables.Clear();

            // _stateActions.Clear();

            _hassAreas.Clear();
            _hassDevices.Clear();
            _hassEntities.Clear();
            _runningAppInstances.Clear();
            _daemonServiceCallFunctions.Clear();
            _eventHandlerTasks.Clear();
            _dataCache.Clear();
        }

        /// <inheritdoc/>
        public async Task StopDaemonActivitiesAsync()
        {
            StopDaemonActivities();

            await _scheduler.Restart().ConfigureAwait(false);
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
        protected virtual async Task HandleNewEvent(HassEvent hassEvent, CancellationToken token)
        {
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
                        Logger.LogWarning($"Can not fix state typing for new: {stateData?.NewState?.State?.GetType()}:{stateData?.NewState?.State}, old:  {stateData?.OldState?.State?.GetType()}:{stateData?.OldState?.State}");
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
                        foreach ((string pattern, Func<string, EntityState?, EntityState?, Task> func) in app.Value.StateActions.Values)
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

                    // Call the observable with no blocking
                    foreach (var observer in ((StateChangeObservable)StateChanges).Observers)
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
                        foreach (var (domain, service, func) in app.Value.ServiceCallFunctions)
                        {
                            if (domain == serviceCallData.Domain &&
                                service == serviceCallData.Service)
                            {
                                tasks.Add(func(serviceCallData.Data));
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

                    // Call the observable with no blocking
                    foreach (var observer in ((EventObservable)EventChanges).Observers)
                    {
                        tasks.Add(Task.Run(() =>
                        {
                            try
                            {
                                observer.OnNext(
                                    new RxEvent(serviceCallData.Service, serviceCallData.Domain,
                                                         serviceCallData.ServiceData));
                            }
                            catch (Exception e)
                            {
                                observer.OnError(e);
                                Logger.LogError(e, "Fail to OnNext on event observer (service_call)");
                            }
                        }));
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
                        foreach ((string ev, Func<string, dynamic, Task> func) in app.Value.EventActions)
                        {
                            if (ev == hassEvent.EventType)
                            {
                                tasks.Add(func(ev, hassEvent.Data));
                            }
                        }
                        foreach ((Func<FluentEventProperty, bool> selectFunc, Func<string, dynamic, Task> func) in app.Value.EventFunctions)
                        {
                            if (selectFunc(new FluentEventProperty { EventId = hassEvent.EventType, Data = hassEvent.Data }))
                            {
                                tasks.Add(func(hassEvent.EventType, hassEvent.Data));
                            }
                        }
                    }

                    // Call the observable with no blocking
                    foreach (var observer in ((EventObservable)EventChanges).Observers)
                    {
                        tasks.Add(Task.Run(() =>
                        {
                            try
                            {
                                observer.OnNext(
                                    new RxEvent(hassEvent.EventType, null, hassEvent.Data));
                            }
                            catch (Exception e)
                            {
                                observer.OnError(e);
                                Logger.LogError(e, "Fail to OnNext on event observer (event)");
                            }
                        }));
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

        private async Task HandleAsyncServiceCalls(CancellationToken cancellationToken)
        {
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
    }

    #region IObservable<(EntityState, EntityState)> implementation

    public class StateChangeObservable : IRxStateChange
    {
        private readonly ConcurrentDictionary<IObserver<(EntityState, EntityState)>, IObserver<(EntityState, EntityState)>>
            _stateObserversTuples = new ConcurrentDictionary<IObserver<(EntityState, EntityState)>, IObserver<(EntityState, EntityState)>>();

        public IEnumerable<IObserver<(EntityState, EntityState)>> Observers => _stateObserversTuples.Values;

        public void Clear() => _stateObserversTuples.Clear();

        public IDisposable Subscribe(IObserver<(EntityState Old, EntityState New)> observer)
        {
            if (!_stateObserversTuples.ContainsKey(observer))
                _stateObserversTuples.TryAdd(observer, observer);

            return new UnsubscriberEntityStateTuple(_stateObserversTuples, observer);
        }
        private class UnsubscriberEntityStateTuple : IDisposable
        {
            private readonly IObserver<(EntityState, EntityState)> _observer;
            private readonly ConcurrentDictionary<IObserver<(EntityState, EntityState)>, IObserver<(EntityState, EntityState)>> _observers;

            public UnsubscriberEntityStateTuple(
                ConcurrentDictionary<IObserver<(EntityState, EntityState)>, IObserver<(EntityState, EntityState)>> observers, IObserver<(EntityState, EntityState)> observer)
            {
                this._observers = observers;
                this._observer = observer;
            }

            public void Dispose()
            {
                if (_observer is object)
                {
                    _observers.TryRemove(_observer, out _);
                }
                System.Console.WriteLine($"StateSubscribers:{_observers.Count}");
            }
        }
    }

    #endregion IObservable<(EntityState, EntityState)> implementation

    #region IObservable<RxEvent> implementation

    public class EventObservable : IRxEvent
    {
        private readonly ConcurrentDictionary<IObserver<RxEvent>, IObserver<RxEvent>> _stateObserversTuples = new ConcurrentDictionary<IObserver<RxEvent>, IObserver<RxEvent>>();

        public IEnumerable<IObserver<RxEvent>> Observers => _stateObserversTuples.Values;

        public void Clear() => _stateObserversTuples.Clear();

        public IDisposable Subscribe(IObserver<RxEvent> observer)
        {
            if (!_stateObserversTuples.ContainsKey(observer))
                _stateObserversTuples.TryAdd(observer, observer);

            return new UnsubscriberEntityStateTuple(_stateObserversTuples, observer);
        }
        private class UnsubscriberEntityStateTuple : IDisposable
        {
            private readonly IObserver<RxEvent> _observer;
            private readonly ConcurrentDictionary<IObserver<RxEvent>, IObserver<RxEvent>> _observers;

            public UnsubscriberEntityStateTuple(ConcurrentDictionary<IObserver<RxEvent>, IObserver<RxEvent>> observers, IObserver<RxEvent> observer)
            {
                this._observers = observers;
                this._observer = observer;
            }

            public void Dispose()
            {
                if (_observer is object)
                {
                    _observers.TryRemove(_observer, out _);
                }
                System.Console.WriteLine($"EventSubscribers:{_observers.Count}");
            }
        }
    }

    #endregion IObservable<RxEvent> implementation
}