using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NetDaemon.AppModel;
using NetDaemon.Extensions.MqttEntityManager;
using NetDaemon.HassModel;
using NetDaemon.HassModel.Entities;
using NetDaemon.Tests.Integration.Helpers;
using Xunit;

namespace NetDaemon.Tests.Integration;

/// <summary>
/// Test app that exposes a switch through the MQTT entity manager.
/// </summary>
[NetDaemonApp]
public sealed class MqttIntegrationSwitchApp : IAsyncInitializable, IDisposable
{
    /// <summary>
    /// The Home Assistant entity id created by the MQTT integration test app.
    /// </summary>
    public const string EntityId = "switch.netdaemon_mqtt_test_switch";
    private const string PayloadOn = "ON";
    private const string PayloadOff = "OFF";

    private readonly IMqttEntityManager _entityManager;
    private IDisposable? _commandSubscription;

    /// <summary>
    /// Initializes a new instance of the <see cref="MqttIntegrationSwitchApp"/> class.
    /// </summary>
    /// <param name="entityManager">The MQTT entity manager.</param>
    public MqttIntegrationSwitchApp(IMqttEntityManager entityManager)
    {
        _entityManager = entityManager;
    }

    /// <inheritdoc />
    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        await _entityManager.CreateAsync(
            EntityId,
            new EntityCreationOptions(
                UniqueId: "netdaemon_mqtt_test_switch",
                Name: "NetDaemon MQTT Test Switch",
                PayloadOn: PayloadOn,
                PayloadOff: PayloadOff)).ConfigureAwait(false);

        await _entityManager.SetStateAsync(EntityId, PayloadOff).ConfigureAwait(false);

        var commands = await _entityManager.PrepareCommandSubscriptionAsync(EntityId).ConfigureAwait(false);
        _commandSubscription = commands.Subscribe(command => _ = HandleCommandAsync(command));
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _commandSubscription?.Dispose();
    }

    private async Task HandleCommandAsync(string command)
    {
        if (command is PayloadOn or PayloadOff)
        {
            await _entityManager.SetStateAsync(EntityId, command).ConfigureAwait(false);
        }
    }
}

/// <summary>
/// Integration tests for MQTT-backed Home Assistant entities.
/// </summary>
public class MqttIntegrationTests : NetDaemonIntegrationBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MqttIntegrationTests"/> class.
    /// </summary>
    /// <param name="homeAssistantLifetime">The shared Home Assistant test lifetime.</param>
    public MqttIntegrationTests(HomeAssistantLifetime homeAssistantLifetime) : base(homeAssistantLifetime)
    {
    }

    /// <summary>
    /// Verifies that an MQTT switch can be turned on and off through Home Assistant.
    /// </summary>
    [Fact]
    public async Task MqttSwitch_ShouldTurnOnAndOffThroughHomeAssistant()
    {
        var haContext = Services.GetRequiredService<IHaContext>();

        await WaitForStateAsync(haContext, MqttIntegrationSwitchApp.EntityId, "off");

        haContext.CallService(
            "switch",
            "turn_on",
            ServiceTarget.FromEntities(MqttIntegrationSwitchApp.EntityId));

        await WaitForStateAsync(haContext, MqttIntegrationSwitchApp.EntityId, "on");

        haContext.CallService(
            "switch",
            "turn_off",
            ServiceTarget.FromEntities(MqttIntegrationSwitchApp.EntityId));

        await WaitForStateAsync(haContext, MqttIntegrationSwitchApp.EntityId, "off");
    }

    private static async Task WaitForStateAsync(IHaContext haContext, string entityId, string expectedState)
    {
        var timeout = DateTimeOffset.UtcNow.AddSeconds(30);

        while (DateTimeOffset.UtcNow < timeout)
        {
            if (haContext.GetState(entityId)?.State == expectedState)
            {
                return;
            }

            await Task.Delay(TimeSpan.FromMilliseconds(250)).ConfigureAwait(false);
        }

        haContext.GetState(entityId)?.State.Should().Be(expectedState);
    }
}
