using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reactive.Linq;
using System.Text;
using NetDaemon.AppModel;
using NetDaemon.HassModel.Common;
using NetDaemon.HassModel.Integration;

namespace NetDaemon.Runtime.Internal;

internal class AppStateManager : IAppStateManager, IHandleHomeAssistantAppStateUpdates
{
    private readonly IServiceProvider _provider;

    public AppStateManager(
        IServiceProvider provider
    )
    {
        _provider = provider;
    }

    public async Task<ApplicationState> GetStateAsync(string applicationId)
    {
        // Since IHaContext is scoped and StateManager is singleton we get the
        // IHaContext everytime we need to check state
        var scope = _provider.CreateScope();
        try
        {
            var haContext = scope.ServiceProvider.GetRequiredService<IHaContext>();
            var entityId = ToSafeHomeAssistantEntityIdFromApplicationId(applicationId);
            var appState = haContext.GetState(entityId);

            if (appState is null)
            {
                haContext.SetEntityState(entityId, "on");
                return ApplicationState.Enabled;
            }

            return appState.State == "on" ? ApplicationState.Enabled : ApplicationState.Disabled;
        }
        finally
        {
            if (scope is IAsyncDisposable serviceScopeAsyncDisposable)
                await serviceScopeAsyncDisposable.DisposeAsync().ConfigureAwait(false);
        }
    }

    public async Task SaveStateAsync(string applicationId, ApplicationState state)
    {
        // Since IHaContext is scoped and StateManager is singleton we get the
        // IHaContext everytime we need to check state
        var scope = _provider.CreateScope();
        try
        {
            var haContext = scope.ServiceProvider.GetRequiredService<IHaContext>();
            var entityId = ToSafeHomeAssistantEntityIdFromApplicationId(applicationId);

            switch (state)
            {
                case ApplicationState.Enabled:
                    haContext.SetEntityState(entityId, "on", new {app_state = "enabled"});
                    break;
                case ApplicationState.Running:
                    haContext.SetEntityState(entityId, "on", new {app_state = "running"});
                    break;
                case ApplicationState.Error:
                    haContext.SetEntityState(entityId, "on", new {app_state = "error"});
                    break;
                case ApplicationState.Disabled:
                    haContext.SetEntityState(entityId, "off", new {app_state = "disabled"});
                    break;
            }
        }
        finally
        {
            if (scope is IAsyncDisposable serviceScopeAsyncDisposable)
                await serviceScopeAsyncDisposable.DisposeAsync().ConfigureAwait(false);
        }
    }

    public void Initialize(IHomeAssistantConnection haConnection, IAppModelContext appContext)
    {
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
                    if (entityId == changedEvent.NewState.EntityId)
                    {
                        await app.SetStateAsync(
                            changedEvent?.NewState?.State == "on" ? ApplicationState.Enabled : ApplicationState.Disabled
                        );
                        break;
                    }
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

        return $"switch.netdaemon_{stringBuilder.ToString().ToLowerInvariant()}";
    }
}