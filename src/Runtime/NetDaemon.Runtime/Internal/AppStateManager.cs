using System.Collections.Concurrent;
using System.Reactive.Linq;
using NetDaemon.AppModel;

namespace NetDaemon.Runtime.Internal;

internal class AppStateManager(IAppStateRepository appStateRepository,
    IHostEnvironment hostEnvironment) : IAppStateManager, IHandleHomeAssistantAppStateUpdates, IDisposable
{
    private readonly CancellationTokenSource _cancelTokenSource = new();

    public async Task InitializeAsync(IHomeAssistantConnection haConnection, IAppModelContext appContext)
    {
        if (appContext.Applications.Count > 0 && !hostEnvironment.IsDevelopment())
            await appStateRepository.RemoveNotUsedStatesAsync(appContext.Applications.Select(a => a.Id).OfType<string>().ToList(),
                _cancelTokenSource.Token);

        var hassEvents = await haConnection.SubscribeToHomeAssistantEventsAsync(null, _cancelTokenSource.Token)
            .ConfigureAwait(false);

        var appByEntityId = appContext.Applications
            .Where(a => a.Id is not null)
            .ToDictionary(app => EntityMapperHelper.ToEntityIdFromApplicationId(app.Id!, hostEnvironment.IsDevelopment()));

        hassEvents
            .Where(n => n.EventType == "state_changed")
            .Select(async s =>
            {
                // first check if we actually care about this state change event before deserializing the whole event
                var eventEntityId = s.DataElement?.GetProperty("entity_id").GetString();

                if (eventEntityId is not null && appByEntityId.TryGetValue(eventEntityId, out var app))
                {
                    var changedEvent = s.ToStateChangedEvent() ?? throw new InvalidOperationException();

                    if (changedEvent.NewState is null || changedEvent.OldState is null)
                        // Ignore if entity just created or deleted
                        return;
                    if (changedEvent.NewState.State == changedEvent.OldState.State)
                        // We only care about changed state
                        return;

                    var appState = changedEvent.NewState?.State == "on"
                        ? ApplicationState.Enabled
                        : ApplicationState.Disabled;

                    await app.SetStateAsync(
                        appState);
                }
            }).Subscribe();
    }

    public async Task<ApplicationState> GetStateAsync(string applicationId)
    {
        return await appStateRepository.GetOrCreateAsync(applicationId, _cancelTokenSource.Token)
            .ConfigureAwait(false)
            ? ApplicationState.Enabled
            : ApplicationState.Disabled;
    }

    public async Task SaveStateAsync(string applicationId, ApplicationState state)
    {
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
