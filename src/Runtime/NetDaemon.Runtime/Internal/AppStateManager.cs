using System.Collections.Concurrent;
using System.Reactive.Linq;
using NetDaemon.AppModel;

namespace NetDaemon.Runtime.Internal;

internal class AppStateManager : IAppStateManager, IHandleHomeAssistantAppStateUpdates, IDisposable
{
    private readonly IAppStateRepository _appStateRepository;
    private readonly CancellationTokenSource _cancelTokenSource = new();
    private readonly ConcurrentDictionary<string, ApplicationState> _stateCache = new();

    public AppStateManager(
        IAppStateRepository appStateRepository
    )
    {
        _appStateRepository = appStateRepository;
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

        switch (state)
        {
            case ApplicationState.Enabled when !isEnabled:
            case ApplicationState.Disabled when isEnabled:
                await _appStateRepository.UpdateAsync(applicationId, isEnabled, _cancelTokenSource.Token)
                    .ConfigureAwait(false);
                break;
        }
    }

    public void Dispose()
    {
        _cancelTokenSource.Dispose();
    }

    public async Task InitializeAsync(IHomeAssistantConnection haConnection, IAppModelContext appContext)
    {
        _stateCache.Clear();
        if (appContext.Applications.Count > 0)
            await _appStateRepository.RemoveNotUsedStatesAsync(appContext.Applications.Select(a => a.Id).ToList()!,
                _cancelTokenSource.Token);

        haConnection.OnHomeAssistantEvent
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
                        EntityMapperHelper.ToSafeHomeAssistantEntityIdFromApplicationId(app.Id ??
                            throw new InvalidOperationException());
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
}
