using System.Reactive.Linq;
using FluentAssertions;
using NetDaemon.HassModel;
using NetDaemon.Tests.Integration.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace NetDaemon.Tests.Integration;

[Collection("HomeAssistant collection")]
public sealed class HaContextFactoryTest(HomeAssistantLifetime homeAssistantLifetime, ITestOutputHelper outputHelper) : IAsyncDisposable
{
    // TODO: remove
    [Fact]
    public async Task CreateAsync()
    {
        var testValue = Guid.CreateVersion7().ToString();

        var haContext = await HaContextFactory.CreateAsync($"ws://localhost:{homeAssistantLifetime.Port}/api/websocket",
            homeAssistantLifetime.AccessToken!);

        var inputText = haContext.Entity("input_text.test_result");
        outputHelper.WriteLine("Initial State = '" + inputText.State + "'");


        var nextStateChange = inputText.StateChanges().FirstAsync();

        // Act
        inputText.CallService("set_value", new { value = testValue });
        await Task.Yield();

        // Assert
        var stateChange = nextStateChange.Timeout(TimeSpan.FromSeconds(30)).GetAwaiter().GetResult();

        stateChange.New!.State.Should().Be(testValue, "We should have received the state change after calling the service");
        inputText.State.Should().Be(testValue, "The state should be updated in the cache after the state_change event is received");
    }


    private static T RunWithoutSynchronizationContext<T>(Func<T> func)
    {
        // Capture the current synchronization context so we can restore it later.
        // We don't have to be afraid of other threads here as this is a ThreadStatic.
        var synchronizationContext = SynchronizationContext.Current;
        try
        {
            SynchronizationContext.SetSynchronizationContext(null);
            return func();
        }
        finally
        {
            SynchronizationContext.SetSynchronizationContext(synchronizationContext);
        }
    }

    public async ValueTask DisposeAsync()
    {
        await homeAssistantLifetime.DisposeAsync();
    }
}
