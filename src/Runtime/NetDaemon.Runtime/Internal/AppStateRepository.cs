using System.Net;
using NetDaemon.Client.HomeAssistant.Model;
using NetDaemon.Client.Internal.Exceptions;

namespace NetDaemon.Runtime.Internal;

internal class AppStateRepository : IAppStateRepository
{
    private readonly IHomeAssistantRunner _runner;

    public AppStateRepository(IHomeAssistantRunner runner)
    {
        _runner = runner;
    }

    public async Task<bool> GetOrCreateAsync(string applicationId, CancellationToken token)
    {
        var haConnection = _runner.CurrentConnection ??
                           throw new InvalidOperationException();

        var entityId = EntityMapperHelper.ToSafeHomeAssistantEntityIdFromApplicationId(applicationId);

        try
        {
            var state = await haConnection.GetEntityStateAsync(entityId, token)
                .ConfigureAwait(false);
            return state?.State == "on";
        }
        catch (HomeAssistantApiCallException e)
        {
            // Missing entity will throw a http status not found
            if (e.Code != HttpStatusCode.NotFound) throw;
            // The app state input_boolean does not exist, lets create a helper
            var name = entityId[14..]; // remove the "input_boolean." part
            await haConnection.CreateInputBooleanHelperAsync(name, token);
            await UpdateAsync(applicationId, true, token);
            return true;
        }
    }

    public async Task UpdateAsync(string applicationId, bool enabled, CancellationToken token)
    {
        var haConnection = _runner.CurrentConnection ??
                           throw new InvalidOperationException();

        var entityId = EntityMapperHelper.ToSafeHomeAssistantEntityIdFromApplicationId(applicationId);

        await haConnection.CallServiceAsync("input_boolean", enabled ? "turn_on" : "turn_off",
            new HassTarget {EntityIds = new[] {entityId}},
            cancelToken: token).ConfigureAwait(false);
    }

    public async Task RemoveNotUsedStatesAsync(IReadOnlyCollection<string> applicationIds, CancellationToken token)
    {
        if (applicationIds.Count == 0)
            return;

        var haConnection = _runner.CurrentConnection ??
                           throw new InvalidOperationException();
        var helpers = await haConnection.ListInputBooleanHelpersAsync(token).ConfigureAwait(false);

        var entityIds = applicationIds.Select(EntityMapperHelper.ToSafeHomeAssistantEntityIdFromApplicationId);

        var notUsedHelperIds = helpers.Where(n =>
            !entityIds.Contains($"input_boolean.{n.Name}") && n.Id.StartsWith("netdaemon_"));

        foreach (var helper in notUsedHelperIds)
            await haConnection.DeleteInputBooleanHelperAsync(helper.Id, token).ConfigureAwait(false);
    }
}
