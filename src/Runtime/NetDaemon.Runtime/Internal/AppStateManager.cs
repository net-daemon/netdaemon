using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;
using System.Reactive.Linq;
using System.Text;
using NetDaemon.AppModel;
using NetDaemon.Client.HomeAssistant.Model;
using NetDaemon.Client.Internal.Exceptions;

namespace NetDaemon.Runtime.Internal;

internal class AppStateManager : IAppStateManager, IHandleHomeAssistantAppStateUpdates, IDisposable
{
    private readonly CancellationTokenSource _cancelTokenSource = new();
    private readonly IServiceProvider _provider;
    private readonly ConcurrentDictionary<string, ApplicationState> _stateCache = new();

    public AppStateManager(
        IServiceProvider provider
    )
    {
        _provider = provider;
    }

    public async Task<ApplicationState> GetStateAsync(string applicationId)
    {
        var entityId = ToSafeHomeAssistantEntityIdFromApplicationId(applicationId);
        if (_stateCache.TryGetValue(entityId, out var applicationState)) return applicationState;

        return (await GetOrCreateStateForApp(entityId).ConfigureAwait(false))?.State == "on"
            ? ApplicationState.Enabled
            : ApplicationState.Disabled;
    }

    public async Task SaveStateAsync(string applicationId, ApplicationState state)
    {
        var haConnection = _provider.GetRequiredService<IHomeAssistantConnection>() ??
                           throw new InvalidOperationException();
        var entityId = ToSafeHomeAssistantEntityIdFromApplicationId(applicationId);

        _stateCache[entityId] = state;

        var currentState = (await GetOrCreateStateForApp(entityId).ConfigureAwait(false))?.State
                           ?? throw new InvalidOperationException();

        switch (state)
        {
            case ApplicationState.Enabled when currentState == "off":
                await haConnection.CallServiceAsync("input_boolean", "turn_on",
                    new HassTarget {EntityIds = new[] {entityId}},
                    cancelToken: _cancelTokenSource.Token);
                break;
            case ApplicationState.Disabled when currentState == "on":
                await haConnection.CallServiceAsync("input_boolean", "turn_off",
                    new HassTarget {EntityIds = new[] {entityId}},
                    cancelToken: _cancelTokenSource.Token);
                break;
        }
    }

    public void Initialize(IHomeAssistantConnection haConnection, IAppModelContext appContext)
    {
        ClearExistingCacheOnNewConnection();
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
                        ToSafeHomeAssistantEntityIdFromApplicationId(app.Id ?? throw new InvalidOperationException());
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


    /// <summary>
    ///     Converts any unicode string to a safe Home Assistant name
    /// </summary>
    /// <param name="applicationId">The unicode string to convert</param>
    [SuppressMessage("Microsoft.Globalization", "CA1308")]
    [SuppressMessage("", "CA1062")]
    public static string ToSafeHomeAssistantEntityIdFromApplicationId(string applicationId)
    {
        var normalizedString = applicationId.Normalize(NormalizationForm.FormD);
        StringBuilder stringBuilder = new(applicationId.Length);

        foreach (var c in normalizedString)
            switch (CharUnicodeInfo.GetUnicodeCategory(c))
            {
                case UnicodeCategory.LowercaseLetter:
                case UnicodeCategory.UppercaseLetter:
                case UnicodeCategory.DecimalDigitNumber:
                    stringBuilder.Append(c);
                    break;

                case UnicodeCategory.SpaceSeparator:
                case UnicodeCategory.ConnectorPunctuation:
                case UnicodeCategory.DashPunctuation:
                    stringBuilder.Append('_');
                    break;
            }

        return $"input_boolean.netdaemon_{stringBuilder.ToString().ToLowerInvariant()}";
    }

    private void ClearExistingCacheOnNewConnection()
    {
        _stateCache.Clear();
    }

    private async Task<HassState?> GetOrCreateStateForApp(string entityId)
    {
        var haConnection = _provider.GetRequiredService<IHomeAssistantConnection>() ??
                           throw new InvalidOperationException();
        try
        {
            var state = await haConnection.GetEntityStateAsync(entityId, _cancelTokenSource.Token)
                .ConfigureAwait(false);
            return state;
        }
        catch (HomeAssistantApiCallException e)
        {
            // Missing entity will throw a http status not found
            if (e.Code != HttpStatusCode.NotFound) throw;
            // The app state input_boolean does not exist, lets create a helper
            var name = entityId[14..]; // remove the "input_boolean." part
            await haConnection.CreateInputBooleanHelperAsync(name, _cancelTokenSource.Token);
            _stateCache[entityId] = ApplicationState.Enabled;
            await haConnection.CallServiceAsync("input_boolean", "turn_on",
                new HassTarget {EntityIds = new[] {entityId}},
                cancelToken: _cancelTokenSource.Token).ConfigureAwait(false);
            return new HassState {State = "on"};
        }
    }

    public void Dispose()
    {
        _cancelTokenSource.Dispose();
    }
}
