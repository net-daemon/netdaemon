using System.Collections.Concurrent;
using System.Reactive.Linq;
using NetDaemon.AppModel;

namespace NetDaemon.Runtime.Internal;

internal class AppStateManager : IAppStateManager, IHandleHomeAssistantAppStateUpdates, IDisposable
{
    private readonly IAppStateRepository _appStateRepository;
    private readonly IHostEnvironment _hostEnvironment;
    private readonly CancellationTokenSource _cancelTokenSource = new();
    private readonly ConcurrentDictionary<string, ApplicationState> _stateCache = new();

    public AppStateManager(
        IAppStateRepository appStateRepository,
        IHostEnvironment hostEnvironment
    )
    {
        _appStateRepository = appStateRepository;
        _hostEnvironment = hostEnvironment;
    }

    public async Task InitializeAsync(IHomeAssistantConnection haConnection, IAppModelContext appContext)
    {
        _stateCache.Clear();
        if (appContext.Applications.Count > 0 && !_hostEnvironment.IsDevelopment())
            await _appStateRepository.RemoveNotUsedStatesAsync(appContext.Applications.Select(a => a.Id).ToList()!,
                _cancelTokenSource.Token);

        var hassEvents = await haConnection.SubscribeToHomeAssistantEventsAsync(null, _cancelTokenSource.Token)
            .ConfigureAwait(false);
        
        hassEvents
            .Where(n => n.EventType == "state_changed")
            .Select(async s =>
            {
                var changedEvent = s.ToStateChangedEvent() ?? throw new InvalidOperationException();

                if (changedEvent.NewState is null || changedEvent.OldState is null)
                    // Ignore if entity just created or deleted
                    return;
                if (changedEvent.NewState.State == changedEvent.OldState.State)
                    // We only care about changed state
                    return;
                
                foreach (var app in appContext.Applications)
                {
                    var entityId =
                        EntityMapperHelper.ToEntityIdFromApplicationId(app.Id ??
                            throw new InvalidOperationException(), _hostEnvironment.IsDevelopment());
                    if (entityId != changedEvent.NewState.EntityId) continue;

                    if (changedEvent.NewState?.State == "on")
                    {
                        await app.EnableAsync().ConfigureAwait(false);
                    }
                    else
                    {
                        await app.DisableAsync().ConfigureAwait(false);
                    }
                    break;
                }
            }).Subscribe();
    }

    public async Task<ApplicationState> GetStateAsync(string applicationId)
    {
        if (_stateCache.TryGetValue(applicationId, out var applicationState)) return applicationState;

        return await _appStateRepository.GetOrCreateAsync(applicationId, _cancelTokenSource.Token)
            .ConfigureAwait(false)
            ? ApplicationState.Enabled
            : ApplicationState.Disabled;
    }

    public async Task SaveStateAsync(string applicationId, ApplicationState state)
    {
        _stateCache[applicationId] = state;

        var isEnabled = await _appStateRepository.GetOrCreateAsync(applicationId, _cancelTokenSource.Token)
            .ConfigureAwait(false);

        // Only update state if it is different from current
        if (
            (state == ApplicationState.Enabled && !isEnabled) ||
            (state == ApplicationState.Disabled && isEnabled)
            )
        {
            await _appStateRepository.UpdateAsync(applicationId, isEnabled, _cancelTokenSource.Token)
                .ConfigureAwait(false);
        }
    }

    public void Dispose()
    {
        _cancelTokenSource.Dispose();
    }
}
