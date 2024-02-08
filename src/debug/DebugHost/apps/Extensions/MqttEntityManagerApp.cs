#region

using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using NetDaemon.AppModel;
using NetDaemon.Extensions.MqttEntityManager;
using NetDaemon.HassModel;

#endregion

namespace DebugHost.apps.Extensions;

[NetDaemonApp]
public class MqttEntityManagerApp : IAsyncInitializable
{
    private readonly IHaContext _ha;
    private readonly ILogger<MqttEntityManagerApp> _logger;
    private readonly IMqttEntityManager _entityManager;
    private Task? _exerciseTask;

    public MqttEntityManagerApp(IHaContext ha, ILogger<MqttEntityManagerApp> logger, IMqttEntityManager entityManager)
    {
        _ha = ha;
        _logger = logger;
        _entityManager = entityManager;
    }

    [SuppressMessage("Design", "CA1031:Do not catch general exception types",
        Justification = "We need to log unexpected errors")]
#pragma warning disable 1998 // We may be debugging here, so don't block the initialize function
    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        _exerciseTask = Task.Run(() => ExercisorAsync(cancellationToken), cancellationToken);
    }
#pragma warning disable 1998

    private async Task ExercisorAsync(CancellationToken cancellationToken)
    {
        try
        {
            //**************************
            //  Quick entity creation tests
            // **NOTE THAT THESE ENTITIES ARE REMOVED AT THE END OF THIS method
            //**************************
            const string hotDogSensorId = "binary_sensor.s2";
            const string rainNextHour4Id = "sensor.rain_next_hour4";
            const string basicSensorId = "sensor.basic_sensor";
            const string overrideSensorId = "sensor.my_id";
            const string helToSwitchId = "switch.hel_to_switch";
            const string stateChangeId = "sensor.my_id";
            const string binarySensorId = "binary_sensor.manager_test";

            // Create a binary sensor and set its state
            // Note the use of custom payloads...
            await _entityManager.CreateAsync(hotDogSensorId, new EntityCreationOptions(
                Name: "HotDog sensor",
                PayloadAvailable: "hot",
                PayloadNotAvailable: "cold"
            )).ConfigureAwait(false);

            await _entityManager.SetStateAsync(hotDogSensorId, "cold").ConfigureAwait(false);

            // Create a humidity sensor with custom measurement and apply a sequence of values
            await _entityManager.CreateAsync(rainNextHour4Id, new EntityCreationOptions(
                    Name: "Rain Next Hour4",
                    DeviceClass: "humidity",
                    PayloadAvailable: "up",
                    PayloadNotAvailable: "down"
                ),
                new { unit_of_measurement = "mm/h" }
            ).ConfigureAwait(false);

            await _entityManager.SetAvailabilityAsync(rainNextHour4Id, "up").ConfigureAwait(false);
            await _entityManager.SetStateAsync(rainNextHour4Id, "3").ConfigureAwait(false);
            await _entityManager.SetStateAsync(rainNextHour4Id, "2").ConfigureAwait(false);
            await _entityManager.SetStateAsync(rainNextHour4Id, "1").ConfigureAwait(false);

            // Basic entity creation
            await _entityManager.CreateAsync(basicSensorId).ConfigureAwait(false);

            // Overriding the default unique ID and name
            await _entityManager.CreateAsync(overrideSensorId,
                    new EntityCreationOptions(UniqueId: "my_id", Name: "A special kind of sensor"))
                .ConfigureAwait(false);

            // Switches require a device class
            await _entityManager.CreateAsync(helToSwitchId,
                    new EntityCreationOptions(DeviceClass: "switch", Name: "HelTo switch"))
                .ConfigureAwait(false);

            // Change state of the new sensor, set an attribute to right now
            await _entityManager.SetStateAsync(stateChangeId, "dull")
                .ConfigureAwait(false);
            await _entityManager.SetAttributesAsync(stateChangeId, new { updated = DateTime.UtcNow })
                .ConfigureAwait(false);

            // Walk through a more complete set of examples, checking HA to verify that each operation completed
            _logger.LogInformation("Creating Entity {EntityId}", binarySensorId);
            var createOptions = new EntityCreationOptions(DeviceClass: "motion", Name: "Manager Test");
            await _entityManager.CreateAsync(binarySensorId, createOptions).ConfigureAwait(false);
            // Using Delay to give Mqtt and HA enough time to process events.
            // Only needed for the example as we immediately read the entity and it may not yet exist
            await Task.Delay(250, cancellationToken).ConfigureAwait(false);

            var entity = _ha.Entity(binarySensorId);
            _logger.LogInformation("Entity {EntityId} State: {State}",binarySensorId, entity.State);

            await _entityManager.SetStateAsync(binarySensorId, "ON")
                .ConfigureAwait(false);
            await _entityManager.SetAttributesAsync(binarySensorId, new { attribute1 = "attr1" })
                .ConfigureAwait(false);
            await Task.Delay(250, cancellationToken).ConfigureAwait(false);
            _logger.LogInformation("Entity {EntityId} State: {State} Attributes: {Attributes}",
                binarySensorId, entity.State, entity.Attributes);

            await _entityManager.RemoveAsync(binarySensorId).ConfigureAwait(false);
            await Task.Delay(250, cancellationToken).ConfigureAwait(false);
            var removed = _ha.Entity(binarySensorId).State == null;
            _logger.LogInformation("Removed Entity: {Removed}", removed);

            // Advanced example - create a device called "Car Charger" and attach five sensors to it
            await CreateDeviceAndMultipleSensors();

            // Remove other entities
            // SET BREAKPOINT HERE if you want to check the entities in Home Assistant
            await _entityManager.RemoveAsync(hotDogSensorId).ConfigureAwait(false);
            await _entityManager.RemoveAsync(rainNextHour4Id).ConfigureAwait(false);
            await _entityManager.RemoveAsync(basicSensorId).ConfigureAwait(false);
            await _entityManager.RemoveAsync(overrideSensorId).ConfigureAwait(false);
            await _entityManager.RemoveAsync(helToSwitchId).ConfigureAwait(false);
            await _entityManager.RemoveAsync(stateChangeId).ConfigureAwait(false);

            await RemoveDeviceAndMultipleSensors();
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            throw;
        }
    }

    private async Task CreateDeviceAndMultipleSensors()
    {
        // This device will have five sensors. We tie all of the sensors together
        // by sharing the same `identifiers` list with each sensor.
        var identifiers = new[] { "car_charger" };

        // It is important that all sensors share the same State Topic so that
        // we can update all values in one go.
        // You will see that in each sensor, the `value_template` defines how
        // we extract the sensor value from the multiple update.
        var stateTopic = "homeassistant/sensor/car_charger/state";

        // First we define the device that will own all the sensors. This is passed
        // when we create the first of the sensors.
        var device = new { identifiers = identifiers, name = "Car Charger", model = "ABC X1", manufacturer = "Voltium", sw_version = 1.22 };

        // Create the first sensor for temperature. This requires a unique entity ID
        // and value_template, but needs to include the shared state topic and device info
        await _entityManager.CreateAsync("sensor.car_charger_temperature", new EntityCreationOptions
        {
            Name = "Temperature",
            DeviceClass = "temperature",
        }, new
        {
            unit_of_measurement = "\u00b0C",
            state_topic = stateTopic,   // Note the override of the state topic
            value_template = "{{ value_json.temperature }}", // and value from state
            device             // Links the sensors together
        }).ConfigureAwait(false);

        // The next sensor is charging progress, so again has a unique entity ID and value
        // template, and also shares the state topic and device info
        await _entityManager.CreateAsync("sensor.car_charger_progress", new EntityCreationOptions
        {
            Name = "Progress"
        }, new
        {
            unit_of_measurement = "%",
            icon = "mdi:progress-clock",
            state_topic = stateTopic,
            value_template = "{{ value_json.progress }}",
            device
        }).ConfigureAwait(false);

        // Then a voltage sensor
        await _entityManager.CreateAsync("sensor.car_charger_voltage", new EntityCreationOptions
        {
            Name = "Voltage",
            DeviceClass = "voltage"
        }, new
        {
            unit_of_measurement = "V",
            state_topic = stateTopic,
            value_template = "{{ value_json.voltage }}",
            device
        }).ConfigureAwait(false);

        // ...followed by a battery sensor. This has a special meaning in Home Assistant
        // as entities with the device class of `battery` can be used in automations to
        // identify which are running low
        await _entityManager.CreateAsync("sensor.car_charger_battery", new EntityCreationOptions
        {
            Name = "Battery",
            DeviceClass = "battery"
        }, new
        {
            unit_of_measurement = "%",
            state_topic = stateTopic,
            value_template = "{{ value_json.battery }}",
            device
        }).ConfigureAwait(false);

        // and finally, a mode sensor that can be represented as a string
        await _entityManager.CreateAsync("sensor.car_charger_mode", new EntityCreationOptions
        {
            Name = "Mode",
        }, new
        {
            icon = "mdi:list-status",
            state_topic = stateTopic,
            value_template = "{{ value_json.mode }}",
            device
        }).ConfigureAwait(false);

        // Now that we have everything set up we can post an update to the shared state topic.
        // This needs to be a JSON string comprising all of the values we want to set so let's
        // start with a dynamic object and then JSON it
        var newState = new
        {
            temperature = 47,
            progress = 75,
            voltage = 23,
            battery = 19,
            mode = "Charging"
        };

        await _entityManager.SetStateAsync("sensor.car_charger", JsonSerializer.Serialize(newState))
            .ConfigureAwait(false);
    }

    private async Task RemoveDeviceAndMultipleSensors()
    {
        await _entityManager.RemoveAsync("sensor.car_charger").ConfigureAwait(false);
        await _entityManager.RemoveAsync("sensor.car_charger_battery").ConfigureAwait(false);
        await _entityManager.RemoveAsync("sensor.car_charger_mode").ConfigureAwait(false);
        await _entityManager.RemoveAsync("sensor.car_charger_progress").ConfigureAwait(false);
        await _entityManager.RemoveAsync("sensor.car_charger_temperature").ConfigureAwait(false);
        await _entityManager.RemoveAsync("sensor.car_charger_voltage").ConfigureAwait(false);

    }
}
