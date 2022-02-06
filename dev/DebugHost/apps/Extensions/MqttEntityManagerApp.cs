#region

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NetDaemon.AppModel;
using NetDaemon.Extensions.MqttEntityManager;
using NetDaemon.Extensions.MqttEntityManager.Models;
using NetDaemon.HassModel;

#endregion

namespace DebugHost.apps.Extensions;

[NetDaemonApp]
[Focus]
public class MqttEntityManagerApp : IAsyncInitializable
{
    private readonly IHaContext _ha;
    private readonly ILogger<MqttEntityManagerApp> _logger;
    private readonly IMqttEntityManager _entityManager;

    public MqttEntityManagerApp(IHaContext ha, ILogger<MqttEntityManagerApp> logger, IMqttEntityManager entityManager)
    {
        _ha = ha;
        _logger = logger;
        _entityManager = entityManager;
    }

    [SuppressMessage("Naming", "CA1727:Use PascalCase for named placeholders", Justification = "<Pending>")]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types",
        Justification = "We need to log unexpected errors")]
    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _entityManager.RemoveAsync("binary_sensor.mqtt_test").ConfigureAwait(false);
            await _entityManager.RemoveAsync("binary_sensor.nd_test").ConfigureAwait(false);
            await _entityManager.RemoveAsync("binary_sensor.leith_tides").ConfigureAwait(false);
            await _entityManager.RemoveAsync("input_text.sample_text_helper").ConfigureAwait(false);


            // Basic entity creation
            await _entityManager.CreateAsync("sensor.basic_sensor").ConfigureAwait(false);

            // Overriding the default unique ID and name
            await _entityManager.CreateAsync("sensor.my_id",
                    new EntityCreationOptions(UniqueId: "my_id", Name: "A special kind of sensor"))
                .ConfigureAwait(false);

            // Switches require a device class
            await _entityManager.CreateAsync("switch.helto_switch",
                    new EntityCreationOptions(Name: "Helto switch", DeviceClass: "switch"))
                .ConfigureAwait(false);

            // Change state of the new sensor, set an attribute to right now
            await _entityManager.UpdateAsync("sensor.my_id", "shiny", new { updated = DateTime.UtcNow })
                .ConfigureAwait(false);

            // Walk through a more complete set of examples, checking HA to verify that each operation completed
            _logger.LogInformation("Creating Entity {domain}.{entityId}", "binary_sensor", "manager_test");
            var createOptions = new EntityCreationOptions(DeviceClass: "motion", Name: "Manager Test");
            await _entityManager.CreateAsync("binary_sensor.manager_test", createOptions).ConfigureAwait(false);
            // Using Delay to give Mqtt and HA enough time to process events.
            // Only needed for the example as we immediately read the entity and it may not yet exist
            await Task.Delay(250, cancellationToken).ConfigureAwait(false);

            var entity = _ha.Entity("binary_sensor.manager_test");
            _logger.LogInformation("Entity {domain}.{entityId} State: {state}", "binary_sensor", "manager_test",
                entity.State);

            await _entityManager.UpdateAsync("binary_sensor.manager_test", "ON", new { attribute1 = "attr1" })
                .ConfigureAwait(false);
            await Task.Delay(250, cancellationToken).ConfigureAwait(false);
            _logger.LogInformation("Entity {domain}.{entityId} State: {state} Attributes: {attributes}",
                "binary_sensor", "manager_test", entity.State, entity.Attributes);

            await _entityManager.RemoveAsync("binary_sensor.manager_test").ConfigureAwait(false);
            await Task.Delay(250, cancellationToken).ConfigureAwait(false);
            var removed = _ha.Entity("binary_sensor.manager_test").State == null;
            _logger.LogInformation("Removed Entity: {removed}", removed);

            // Remove other entities
            await _entityManager.RemoveAsync("sensor.basic_sensor").ConfigureAwait(false);
            await _entityManager.RemoveAsync("sensor.helto_switch").ConfigureAwait(false);
            await _entityManager.RemoveAsync("sensor.my_id").ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
        }
    }
}