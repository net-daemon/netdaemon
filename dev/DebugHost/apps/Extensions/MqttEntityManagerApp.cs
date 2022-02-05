#region

using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NetDaemon.AppModel;
using NetDaemon.Extensions.MqttEntityManager;
using NetDaemon.HassModel;

#endregion

namespace DebugHost.apps.Extensions;

[NetDaemonApp]
[Focus]
public class MqttEntityManagerApp : IAsyncInitializable
{
    private readonly IHaContext                    _ha;
    private readonly ILogger<MqttEntityManagerApp> _logger;
    private readonly IMqttEntityManager            _manager;

    public MqttEntityManagerApp(IHaContext ha, ILogger<MqttEntityManagerApp> logger, IMqttEntityManager manager)
    {
        _ha      = ha;
        _logger  = logger;
        _manager = manager;
    }

    [SuppressMessage("Naming", "CA1727:Use PascalCase for named placeholders", Justification = "<Pending>")]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "We need to log unexpected errors")]
    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Creating Entity {domain}.{entityId}", "binary_sensor", "manager_test");
            await _manager.CreateAsync("binary_sensor", "manager_test", "motion", "Manager Test").ConfigureAwait(false);
            // Using Delay to give Mqtt and HA enough time to process events.
            // Only needed for the example as we immediately read the entity and it may not yet exist
            await Task.Delay(250, cancellationToken).ConfigureAwait(false);

            var entity = _ha.Entity("binary_sensor.manager_test");
            _logger.LogInformation("Entity {domain}.{entityId} State: {state}", "binary_sensor", "manager_test", entity.State);

            await _manager.UpdateAsync("binary_sensor", "manager_test", "ON", JsonSerializer.Serialize(new { attribute1 = "attr1" }))
                .ConfigureAwait(false);
            await Task.Delay(250, cancellationToken).ConfigureAwait(false);
            _logger.LogInformation("Entity {domain}.{entityId} State: {state} Attributes: {attributes}",
                "binary_sensor", "manager_test", entity.State, entity.Attributes);

            await _manager.RemoveAsync("binary_sensor", "manager_test").ConfigureAwait(false);
            await Task.Delay(250, cancellationToken).ConfigureAwait(false);
            var removed = _ha.Entity("binary_sensor.manager_test").State == null;
            _logger.LogInformation("Removed Entity: {removed}", removed);
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
        }
    }
}