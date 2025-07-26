using System.Reactive.Linq;
using FluentAssertions;
using NetDaemon.HassModel;
using NetDaemon.Tests.Integration.Helpers;
using Xunit;

namespace NetDaemon.Tests.Integration;

[Collection("HomeAssistant collection")]
public sealed class HaContextFactoryTest(HomeAssistantLifetime homeAssistantLifetime) : IAsyncDisposable
{
    [Fact]
    public async Task CreateAsync()
    {
        var testValue = Guid.CreateVersion7().ToString();

        // Arrange
        var haContext = await HaContextFactory.CreateAsync($"ws://localhost:{homeAssistantLifetime.Port}/api/websocket",
            homeAssistantLifetime.AccessToken!);

        var inputText = haContext.Entity("input_text.test_result");
        var nextStateChange = inputText.StateChanges().FirstAsync();

        // Act
        inputText.CallService("set_value", new { value = testValue });

        // Assert
        var stateChange = await nextStateChange.Timeout(TimeSpan.FromSeconds(2)); // make the test fail if it takes too long

        stateChange.New!.State.Should().Be(testValue, "We should have received the state change after calling the service");
        inputText.State.Should().Be(testValue, "The state should be updated in the cache after the state_change event is received");
    }

    public async ValueTask DisposeAsync()
    {
        await homeAssistantLifetime.DisposeAsync();
    }
}
