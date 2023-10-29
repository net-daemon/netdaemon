using System.Collections.Concurrent;
using System.Reactive.Linq;
using NetDaemon.AppModel;

namespace NetDaemon.Runtime.Internal;

internal class AppStateManager(IAppStateRepository appStateRepository,
    IHostEnvironment hostEnvironment) : IAppStateManager, IHandleHomeAssistantAppStateUpdates, IDisposable
{
    private readonly CancellationTokenSource _cancelTokenSource = new();
    private readonly ConcurrentDictionary<string, ApplicationState> _stateCache = new();

    public async Task InitializeAsync(IHomeAssistantConnection haConnection, IAppModelContext appContext)
    {
        _stateCache.Clear();
        if (appContext.Applications.Count > 0 && !hostEnvironment.IsDevelopment())
            await appStateRepository.RemoveNotUsedStatesAsync(appContext.Applications.Select(a => a.Id).ToList()!,
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
                            throw new InvalidOperationException(), hostEnvironment.IsDevelopment());
                    if (entityId != changedEvent.NewState.EntityId) continue;

                    var appState = changedEvent.NewState?.State == "on"
                        ? ApplicationState.Enabled
                        : ApplicationState.Disabled;

                    await app.SetStateAsync(
                        appState
                    );
                    break;
                }
            }).Subscribe();
    }

    public async Task<ApplicationState> GetStateAsync(string applicationId)
    {
        if (_stateCache.TryGetValue(applicationId, out var applicationState)) return applicationState;

        return await appStateRepository.GetOrCreateAsync(applicationId, _cancelTokenSource.Token)
            .ConfigureAwait(false)
            ? ApplicationState.Enabled
            : ApplicationState.Disabled;
    }

    public async Task SaveStateAsync(string applicationId, ApplicationState state)
    {
        _stateCache[applicationId] = state;

        var isEnabled = await appStateRepository.GetOrCreateAsync(applicationId, _cancelTokenSource.Token)
            .ConfigureAwait(false);

        // Only update state if it is different from current
        if (
            (state == ApplicationState.Enabled && !isEnabled) ||
            (state == ApplicationState.Disabled && isEnabled)
            )
        {
            await appStateRepository.UpdateAsync(applicationId, isEnabled, _cancelTokenSource.Token)
                .ConfigureAwait(false);
        }
    }

    public void Dispose()
    {
        _cancelTokenSource.Dispose();
    }
}
