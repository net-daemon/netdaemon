
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Extensions;
using Microsoft.Extensions.DependencyInjection;
using NetDaemon.Extensions.MqttEntityManager;
using NetDaemon.HassModel;
using NetDaemon.HassModel.Entities;
using NetDaemon.Tests.Integration.Helpers;
using Xunit;

namespace NetDaemon.Tests.Integration;

public class MqttEntityManagerTests : NetDaemonIntegrationBase
{
    private readonly IHaContext _haContext;
    private readonly IMqttEntityManager _mqttEntityManager;

    public MqttEntityManagerTests(HomeAssistantLifetime homeAssistantLifetime) : base(homeAssistantLifetime)
    {
    //     _haContext = Services.GetRequiredService<IHaContext>();
    //     _mqttEntityManager = Services.GetRequiredService<IMqttEntityManager>();
    }
    //
    // [Fact]
    // public async Task CreateSensor_ShouldBeVisibleInHomeAssistant()
    // {
    //     const string entityId = "sensor.my_test_sensor";
    //
    //     await _mqttEntityManager.CreateAsync(entityId);
    //
    //     // Wait for the entity to be created
    //     var state = await _haContext.Entity(entityId).StateChanges().FirstAsync().ToTask();
    //     state.Should().NotBeNull();
    //
    //     // Clean up
    //     await _mqttEntityManager.RemoveAsync(entityId);
    // }
    //
    // [Fact]
    // public async Task RemoveSensor_ShouldBeRemovedFromHomeAssistant()
    // {
    //     const string entityId = "sensor.my_test_sensor_to_remove";
    //
    //     await _mqttEntityManager.CreateAsync(entityId);
    //
    //     // Wait for the entity to be created
    //     var state = await _haContext.Entity(entityId).StateChanges().FirstAsync().ToTask();
    //     state.Should().NotBeNull();
    //
    //     await _mqttEntityManager.RemoveAsync(entityId);
    //
    //     // Wait for the entity to be removed
    //     var removedState = await _haContext.Entity(entityId).StateChanges().FirstOrDefaultAsync(s => s.New == null).ToTask();
    //     removedState.Should().NotBeNull();
    // }
    //
    // [Fact]
    // public async Task Reconnect_ShouldRecreateSensor()
    // {
    //     const string entityId = "sensor.my_test_sensor_for_reconnect";
    //
    //     await _mqttEntityManager.CreateAsync(entityId, new EntityCreationOptions(Name: "Test Sensor"));
    //
    //     // Wait for the entity to be created and become available
    //     var state = await _haContext.Entity(entityId).StateChanges().FirstAsync(s => s.New?.State == "online").ToTask();
    //     state.Should().NotBeNull();
    //
    //     // Dispose the connection to simulate a connection loss
    //     var assuredConnection = (AssuredMqttConnection)Services.GetRequiredService<IAssuredMqttConnection>();
    //     assuredConnection.Dispose();
    //
    //     // Wait for the entity to become unavailable
    //     var unavailableState = await _haContext.Entity(entityId).StateChanges().FirstAsync(s => s.New?.State == "unavailable").ToTask();
    //     unavailableState.Should().NotBeNull();
    //
    //     // Trigger a reconnect by requesting a new connection
    //     _ = Services.GetRequiredService<IAssuredMqttConnection>();
    //
    //     // Wait for the entity to become available again
    //     var availableState = await _haContext.Entity(entityId).StateChanges().FirstAsync(s => s.New?.State == "online").ToTask();
    //     availableState.Should().NotBeNull();
    //
    //     // Clean up
    //     await _mqttEntityManager.RemoveAsync(entityId);
    // }
}
