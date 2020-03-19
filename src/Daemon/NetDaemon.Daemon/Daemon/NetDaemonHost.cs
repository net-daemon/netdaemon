using JoySoftware.HomeAssistant.Client;
using JoySoftware.HomeAssistant.NetDaemon.Common;
using JoySoftware.HomeAssistant.NetDaemon.Daemon.Storage;
using Microsoft.Extensions.Logging;
using System;
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
    public class NetDaemonHost : INetDaemonHost
    {
        /// <summary>
        /// The intervall used when disconnected
        /// </summary>
        private const int _reconnectIntervall = 30000;

        internal readonly Channel<(string, string)> _ttsMessageQueue =
            Channel.CreateBounded<(string, string)>(20);

        // Used for testing
        internal int InternalDelayTimeForTts = 2500;

        // internal so we can use for unittest
        internal IDictionary<string, EntityState> InternalState = new Dictionary<string, EntityState>();

        private readonly IList<(string pattern, Func<string, dynamic, Task> action)> _eventActions =
                    new List<(string pattern, Func<string, dynamic, Task> action)>();

        private readonly List<(Func<FluentEventProperty, bool>, Func<string, dynamic, Task>)> _eventFunctionList =
                    new List<(Func<FluentEventProperty, bool>, Func<string, dynamic, Task>)>();

        private readonly List<Task> _eventHandlerTasks = new List<Task>();

        private readonly IHassClient _hassClient;

        private readonly Scheduler _scheduler;

        private readonly IDataRepository? _repository;

        private readonly IList<(string pattern, Func<string, EntityState?, EntityState?, Task> action)> _stateActions =
            new List<(string pattern, Func<string, EntityState?, EntityState?, Task> action)>();

        private readonly List<string> _supportedDomainsForTurnOnOff = new List<string>
        {
            "light",
            "switch"
        };

        private bool _stopped;

        private readonly List<(string, string, Func<dynamic?, Task>)> _serviceCallFunctionList
            = new List<(string, string, Func<dynamic?, Task>)>();

        private readonly List<(string, string, Func<dynamic?, Task>)> _companionServiceCallFunctionList
            = new List<(string, string, Func<dynamic?, Task>)>();

        public NetDaemonHost(IHassClient? hassClient, IDataRepository? repository, ILoggerFactory? loggerFactory = null)
        {
            loggerFactory ??= DefaultLoggerFactory;
            Logger = loggerFactory.CreateLogger<NetDaemonHost>();
            _hassClient = hassClient ?? throw new ArgumentNullException("HassClient can't be null!");
            _scheduler = new Scheduler(loggerFactory: loggerFactory);
            _repository = repository;
        }

        public bool Connected { get; private set; }

        public ILogger Logger { get; }

        public IScheduler Scheduler => _scheduler;

        public IEnumerable<EntityState> State => InternalState?.Values!;

        private static ILoggerFactory DefaultLoggerFactory => LoggerFactory.Create(builder =>
                                {
                                    builder
                                        .ClearProviders()
                                        .AddConsole();
                                });

        public Task CallService(string domain, string service, dynamic? data = null, bool waitForResponse = false) => _hassClient.CallService(domain, service, data, false);

        public IEntity Entities(Func<IEntityProperties, bool> func)
        {
            try
            {
                IEnumerable<IEntityProperties> x = State.Where(func);

                return new EntityManager(x.Select(n => n.EntityId).ToArray(), this);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Failed to select entities using func");
                throw;
            }
        }

        public IEntity Entities(IEnumerable<string> entityIds) => new EntityManager(entityIds, this);

        public IEntity Entity(params string[] entityIds) => new EntityManager(entityIds, this);

        public IFluentEvent Event(params string[] eventParams) => new FluentEventManager(eventParams, this);

        public IFluentEvent Events(Func<FluentEventProperty, bool> func) => new FluentEventManager(func, this);

        public IFluentEvent Events(IEnumerable<string> eventParams) => new FluentEventManager(eventParams, this);

        public EntityState? GetState(string entity)
        {
            return InternalState.TryGetValue(entity, out EntityState? returnValue)
                ? returnValue
                : null;
        }

        public ILight Light(params string[] entity)
        {
            var entityList = new List<string>(entity.Length);
            foreach (string e in entity)
            {
                // Add the domain light if missing domain in id
                entityList.Add(!e.Contains('.') ? string.Concat("light.", e) : e);
            }

            return new EntityManager(entityList.ToArray(), this);
        }

        public IEntity Lights(Func<IEntityProperties, bool> func)
        {
            IEnumerable<IEntityProperties> x = State.Where(func).Where(n => n.EntityId.Contains("light."));

            return new EntityManager(x.Select(n => n.EntityId).ToArray(), this);
        }

        public void ListenEvent(string ev, Func<string, dynamic, Task> action) => _eventActions.Add((ev, action));

        public void ListenEvent(Func<FluentEventProperty, bool> funcSelector, Func<string, dynamic, Task> func) => _eventFunctionList.Add((funcSelector, func));

        public void ListenServiceCall(string domain, string service, Func<dynamic?, Task> action)
            => _serviceCallFunctionList.Add((domain.ToLowerInvariant(), service.ToLowerInvariant(), action));

        /// <summary>
        /// </summary>
        /// <remarks>
        ///     Valid patterns are:
        ///     light.thelight   - En entity id
        ///     light           - No dot means a domain
        ///     empty           - All events
        /// </remarks>
        /// <param name="pattern">Event pattern</param>
        /// <param name="action">The action to call when event is missing</param>
        public void ListenState(string pattern,
            Func<string, EntityState?, EntityState?, Task> action) => _stateActions.Add((pattern, action));

        public IMediaPlayer MediaPlayer(params string[] entityIds) => new EntityManager(entityIds, this);

        public IMediaPlayer MediaPlayers(IEnumerable<string> entityIds) => new EntityManager(entityIds, this);

        public IMediaPlayer MediaPlayers(Func<IEntityProperties, bool> func)
        {
            try
            {
                IEnumerable<IEntityProperties> x = State.Where(func);

                return new EntityManager(x.Select(n => n.EntityId).ToArray(), this);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Failed to select mediaplayers func");
                throw;
            }
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
            string? hassioToken = Environment.GetEnvironmentVariable("HASSIO_TOKEN");

            if (_hassClient == null)
            {
                throw new NullReferenceException("HassClient cant be null when running daemon, check constructor!");
            }

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    bool connectResult;

                    if (hassioToken != null)
                    {
                        // We are running as hassio add-on
                        connectResult = await _hassClient.ConnectAsync(new Uri("ws://supervisor/core/websocket"),
                            hassioToken, true).ConfigureAwait(false);
                    }
                    else
                    {
                        connectResult = await _hassClient.ConnectAsync(host, port, ssl, token, true).ConfigureAwait(false);
                    }

                    if (!connectResult)
                    {
                        Connected = false;
                        Logger.LogWarning("Home assistant is unavailable, retrying in 30 seconds...");
                        await _hassClient.CloseAsync().ConfigureAwait(false);
                        await Task.Delay(_reconnectIntervall, cancellationToken).ConfigureAwait(false);

                        continue;
                    }

                    // Setup TTS
                    Task handleTextToSpeechMessagesTask = HandleTextToSpeechMessages(cancellationToken);

                    await _hassClient.SubscribeToEvents().ConfigureAwait(false);

                    Connected = true;
                    InternalState = _hassClient.States.Values.Select(n => n.ToDaemonEntityState())
                        .ToDictionary(n => n.EntityId);

                    Logger.LogInformation(
                        hassioToken != null
                            ? "Successfully connected to Home Assistant in hassio add-on"
                            : $"Successfully connected to Home Assistant on host {host}:{port}");

                    while (!cancellationToken.IsCancellationRequested)
                    {
                        HassEvent changedEvent = await _hassClient.ReadEventAsync().ConfigureAwait(false);
                        if (changedEvent != null)
                        {
                            // Remove all completed Tasks
                            _eventHandlerTasks.RemoveAll(x => x.IsCompleted);
                            _eventHandlerTasks.Add(HandleNewEvent(changedEvent, cancellationToken));
                        }
                        else
                        {
                            // Will only happen when doing unit tests
                            await Task.Delay(1000, cancellationToken).ConfigureAwait(false);
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        // Normal behaviour do nothing
                        await _scheduler.Stop().ConfigureAwait(false);
                    }
                }
                catch (Exception e)
                {
                    Connected = false;
                    Logger.LogError(e, "Error, during operation");
                }
                finally
                {
                    try
                    {
                        await _hassClient.CloseAsync().ConfigureAwait(false);
                    }
                    catch
                    {
                    }

                    Connected = false;
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        await Task.Delay(_reconnectIntervall, cancellationToken).ConfigureAwait(false);
                    }
                }
            }

            await _scheduler.Stop().ConfigureAwait(false);
        }

        public IScript RunScript(params string[] entityId) => new EntityManager(entityId, this);

        public async Task<bool> SendEvent(string eventId, dynamic? data = null) => await _hassClient.SendEvent(eventId, data).ConfigureAwait(false);

        public async Task<EntityState?> SetState(string entityId, dynamic state,
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
                    InternalState[entityState.EntityId] = entityState;
                    return entityState;
                }

                return null;
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Failed to set state");
                throw;
            }
        }

        public void Speak(string entityId, string message) => _ttsMessageQueue.Writer.TryWrite((entityId, message));

        public async Task Stop()
        {
            if (_hassClient == null)
            {
                throw new NullReferenceException("HassClient cant be null when running daemon, check constructor!");
            }

            if (_stopped)
            {
                return;
            }

            await _hassClient.CloseAsync().ConfigureAwait(false);
            await _scheduler.Stop().ConfigureAwait(false);

            _stopped = true;
        }

        public ITime Timer() => new Common.TimeManager(this);

        public Task ToggleAsync(string entityId, params (string name, object val)[] attributeNameValuePair)
        {
            // Get the domain if supported, else domain is homeassistant
            string domain = GetDomainFromEntity(entityId);
            // Use it if it is supported else use default "homeassistant" domain
            domain = _supportedDomainsForTurnOnOff.Contains(domain) ? domain : "homeassistant";

            // Use expando object as all other methods
            dynamic attributes = attributeNameValuePair.ToDynamic();
            // and add the entity id dynamically
            attributes.entity_id = entityId;

            return _hassClient.CallService(domain, "toggle", attributes, false);
        }

        public Task TurnOffAsync(string entityId, params (string name, object val)[] attributeNameValuePair)
        {
            // Get the domain if supported, else domain is homeassistant
            string domain = GetDomainFromEntity(entityId);
            // Use it if it is supported else use default "homeassistant" domain
            domain = _supportedDomainsForTurnOnOff.Contains(domain) ? domain : "homeassistant";

            // Use expando object as all other methods
            dynamic attributes = attributeNameValuePair.ToDynamic();
            // and add the entity id dynamically
            attributes.entity_id = entityId;

            return _hassClient.CallService(domain, "turn_off", attributes, false);
        }

        public Task TurnOnAsync(string entityId, params (string name, object val)[] attributeNameValuePair)
        {
            // Use default domain "homeassistant" if supported is missing
            string domain = GetDomainFromEntity(entityId);
            // Use it if it is supported else use default "homeassistant" domain
            domain = _supportedDomainsForTurnOnOff.Contains(domain) ? domain : "homeassistant";

            // Convert the value pairs to dynamic type
            dynamic attributes = attributeNameValuePair.ToDynamic();
            // and add the entity id dynamically
            attributes.entity_id = entityId;

            return _hassClient.CallService(domain, "turn_on", attributes, false);
        }

        protected virtual async Task HandleNewEvent(HassEvent hassEvent, CancellationToken token)
        {
            if (hassEvent.EventType == "state_changed")
            {
                try
                {
                    var stateData = (HassStateChangedEventData?)hassEvent.Data;

                    if (stateData == null)
                    {
                        throw new NullReferenceException("StateData is null!");
                    }

                    if (stateData.NewState == null)
                    {
                        // This is an entity that is removed and have no new state so just return;
                        return;
                    }

                    InternalState[stateData.EntityId] = stateData.NewState!.ToDaemonEntityState();

                    var tasks = new List<Task>();
                    foreach ((string pattern, Func<string, EntityState?, EntityState?, Task> func) in _stateActions)
                    {
                        if (string.IsNullOrEmpty(pattern))
                        {
                            tasks.Add(func(stateData.EntityId,
                                stateData.NewState?.ToDaemonEntityState(),
                                stateData.OldState?.ToDaemonEntityState()
                            ));
                        }
                        else if (stateData.EntityId.StartsWith(pattern))
                        {
                            tasks.Add(func(stateData.EntityId,
                                stateData.NewState?.ToDaemonEntityState(),
                                stateData.OldState?.ToDaemonEntityState()
                            ));
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
                    throw;
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
                    var serviceCallFunctionList = _companionServiceCallFunctionList.Union(_serviceCallFunctionList);

                    foreach (var (domain, service, func) in serviceCallFunctionList)
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
                    throw;
                }
            }
            else
            {
                try
                {
                    var tasks = new List<Task>();
                    foreach ((string ev, Func<string, dynamic, Task> func) in _eventActions)
                    {
                        if (ev == hassEvent.EventType)
                        {
                            tasks.Add(func(ev, hassEvent.Data));
                        }
                    }
                    foreach ((Func<FluentEventProperty, bool> selectFunc, Func<string, dynamic, Task> func) in _eventFunctionList)
                    {
                        if (selectFunc(new FluentEventProperty { EventId = hassEvent.EventType, Data = hassEvent.Data }))
                        {
                            tasks.Add(func(hassEvent.EventType, hassEvent.Data));
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
                    throw;
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

        private async Task HandleTextToSpeechMessages(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    (string entityId, string message) = await _ttsMessageQueue.Reader.ReadAsync(cancellationToken).ConfigureAwait(false);

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

        private IDictionary<string, object> _dataCache = new Dictionary<string, object>();

        public Task SaveDataAsync<T>(string id, T data)
        {
            _ = _repository as IDataRepository ??
                throw new NullReferenceException($"{nameof(_repository)} can not be null!");

            if (data == null)
                throw new ArgumentNullException(nameof(data));

            _dataCache[id] = data;
            return _repository!.Save(id, data);
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

        /// <inheritdoc/>
        public IFluentInputSelect InputSelect(params string[] inputSelectParams) =>
            new InputSelectManager(inputSelectParams, this);

        /// <inheritdoc/>
        public IFluentInputSelect InputSelects(IEnumerable<string> inputSelectParams) =>
            new InputSelectManager(inputSelectParams, this);

        /// <inheritdoc/>
        public IFluentInputSelect InputSelects(Func<IEntityProperties, bool> func)
        {
            IEnumerable<string> x = State.Where(func).Select(n => n.EntityId);
            return new InputSelectManager(x, this);
        }

        /// <inheritdoc/>
        public async Task StopDaemonActivitiesAsync()
        {
            _eventActions.Clear();
            _eventFunctionList.Clear();
            _stateActions.Clear();
            _serviceCallFunctionList.Clear();

            await _scheduler.Stop();
        }

        /// <inheritdoc/>
        public void ListenCompanionServiceCall(string service, Func<dynamic?, Task> action)
            => _companionServiceCallFunctionList.Add(("netdaemon", service.ToLowerInvariant(), action));

        /// <inheritdoc/>
        public async Task SetDaemonStateAsync(int numberOfLoadedApps, int numberOfRunningApps)
        {
            await SetState(
                "netdaemon.status",
                "Connected", // State will alawys be connected, otherwise state could not be set.
                ("number_of_loaded_apps", numberOfLoadedApps),
                ("number_of_running_apps", numberOfRunningApps),
                ("version", GetType().Assembly.GetName().Version?.ToString() ?? "N/A"));
        }
    }
}