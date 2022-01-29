using System.Threading;
using System.Threading.Tasks;
using NetDaemon.Client.Common;
using Xunit;

namespace NetDaemon.Tests.Integration.Helpers;

/// <summary>
///     This fixture is ran once per test session and will
///     make sure Home Assistant is running and we have a
///     valid connection before tests start
/// </summary>
public class MakeSureNetDaemonIsRunningFixture : IAsyncLifetime
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly IHomeAssistantRunner _homeAssistantRunner;

    public MakeSureNetDaemonIsRunningFixture(
        IHomeAssistantRunner homeAssistantRunner
    )
    {
        _homeAssistantRunner = homeAssistantRunner;
    }

    public async Task InitializeAsync()
    {
        var connectionRetries = 0;
        while (!_cancellationTokenSource.Token.IsCancellationRequested)
        {
            if (_homeAssistantRunner is not null)
                break;
            if (connectionRetries++ > 20) // 40 seconds and we give up
                return;
            await Task.Delay(2000, _cancellationTokenSource.Token).ConfigureAwait(false);
        }

        // Introduce a small delay so everything is connected and logged
        await Task.Delay(2000, _cancellationTokenSource.Token).ConfigureAwait(false);
    }

    public Task DisposeAsync()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
        return Task.CompletedTask;
    }
}
