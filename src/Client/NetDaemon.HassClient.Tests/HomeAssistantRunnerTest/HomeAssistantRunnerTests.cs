using NNetDaemon.HassClient.Tests.HomeAssistantRunnerTest;

namespace NetDaemon.HassClient.Tests.HomeAssistantRunnerTest;


public class HomeAssistantRunnerTests
{
    private readonly HomeAssistantClientMock _clientMock = new();

    private readonly Mock<ILogger<IHomeAssistantRunner>> _logMock = new();

    private HomeAssistantRunner DefaultRunner { get; }

    public HomeAssistantRunnerTests()
    {

        DefaultRunner = new(_clientMock.Object, _logMock.Object);
    }
    [Fact]
    public async Task TestSuccessfulShouldPostConnection()
    {
        using var cancelSource = new CancellationTokenSource();

        var connectionTask = DefaultRunner.OnConnect
            .Timeout(TimeSpan.FromMilliseconds(TestSettings.DefaultTimeout), Observable.Return(default(IHomeAssistantConnection?)))
            .FirstAsync()
            .ToTask();

        var runnerTask = DefaultRunner.RunAsync("host", 0, false, "token", TimeSpan.FromMilliseconds(100), cancelSource.Token);

        var connection = await connectionTask.ConfigureAwait(false);
        DefaultRunner.CurrentConnection.Should().NotBeNull();
        try
        {
            cancelSource.Cancel();
            await runnerTask.ConfigureAwait(false);
        }
        catch (OperationCanceledException) { } // ignore cancel error
        await DefaultRunner.DisposeAsync().ConfigureAwait(false);
        connection.Should().NotBeNull();
        Assert.Null(DefaultRunner.CurrentConnection);
    }

    [Fact]
    public async Task TestUnSuccessfulConnectionShouldPostCorrectDisconnectError()
    {
        using var cancelSource = new CancellationTokenSource();
        _clientMock.Setup(n =>
            n.ConnectAsync(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<bool>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()
            )
        ).Throws(new WebSocketException("What ever"));

        var disconnectionTask = DefaultRunner.OnDisconnect
            .Timeout(TimeSpan.FromMilliseconds(TestSettings.DefaultTimeout), Observable.Return(default(DisconnectReason)))
            .FirstAsync()
            .ToTask();

        var runnerTask = DefaultRunner.RunAsync("host", 0, false, "token", TimeSpan.FromMilliseconds(100), cancelSource.Token);

        var reason = await disconnectionTask.ConfigureAwait(false);
        try
        {
            cancelSource.Cancel();
            await runnerTask.ConfigureAwait(false);
        }
        catch (OperationCanceledException) { } // ignore cancel error
        await DefaultRunner.DisposeAsync().ConfigureAwait(false);
        reason.Should().Be(DisconnectReason.Error);
    }

    [Fact]
    public async Task TestNotReadyConnectionShouldPostCorrectDisconnectError()
    {
        using var cancelSource = new CancellationTokenSource();
        _clientMock.Setup(n =>
            n.ConnectAsync(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<bool>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()
            )
        ).Throws(new HomeAssistantConnectionException(DisconnectReason.NotReady));

        var disconnectionTask = DefaultRunner.OnDisconnect
            .Timeout(TimeSpan.FromMilliseconds(TestSettings.DefaultTimeout), Observable.Return(default(DisconnectReason)))
            .FirstAsync()
            .ToTask();

        var runnerTask = DefaultRunner.RunAsync("host", 0, false, "token", TimeSpan.FromMilliseconds(100), cancelSource.Token);

        var reason = await disconnectionTask.ConfigureAwait(false);
        try
        {
            cancelSource.Cancel();
            await runnerTask.ConfigureAwait(false);
        }
        catch (OperationCanceledException) { } // ignore cancel error
        await DefaultRunner.DisposeAsync().ConfigureAwait(false);
        reason.Should().Be(DisconnectReason.NotReady);
    }

    [Fact]
    public async Task TestNotAuthorizedConnectionShouldPostCorrectDisconnectError()
    {
        using var cancelSource = new CancellationTokenSource();
        _clientMock.Setup(n =>
            n.ConnectAsync(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<bool>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()
            )
        ).Throws(new HomeAssistantConnectionException(DisconnectReason.Unauthorized));

        var disconnectionTask = DefaultRunner.OnDisconnect
            .Timeout(TimeSpan.FromMilliseconds(TestSettings.DefaultTimeout), Observable.Return(default(DisconnectReason)))
            .FirstAsync()
            .ToTask();

        var runnerTask = DefaultRunner.RunAsync("host", 0, false, "token", TimeSpan.FromMilliseconds(100), cancelSource.Token);

        var reason = await disconnectionTask.ConfigureAwait(false);
        try
        {
            cancelSource.Cancel();
            await runnerTask.ConfigureAwait(false);
        }
        catch (OperationCanceledException) { } // ignore cancel error
        await DefaultRunner.DisposeAsync().ConfigureAwait(false);
        reason.Should().Be(DisconnectReason.Unauthorized);
    }

    [Fact]
    public async Task TestClientDisconnectShouldPostCorrectDisconnectError()
    {
        using var cancelSource = new CancellationTokenSource();

        var disconnectionTask = DefaultRunner.OnDisconnect
            .Timeout(TimeSpan.FromMilliseconds(TestSettings.DefaultTimeout), Observable.Return(default(DisconnectReason)))
            .FirstAsync()
            .ToTask();

        var runnerTask = DefaultRunner.RunAsync("host", 0, false, "token", TimeSpan.FromMilliseconds(100), cancelSource.Token);

        // await DefaultRunner.DisposeAsync().ConfigureAwait(false);
        cancelSource.Cancel();
        var reason = await disconnectionTask.ConfigureAwait(false);
        try
        {
            await runnerTask.ConfigureAwait(false);
        }
        catch (OperationCanceledException) { } // ignore cancel error
        await DefaultRunner.DisposeAsync().ConfigureAwait(false);
        reason.Should().Be(DisconnectReason.Client);
    }

    [Fact]
    public async Task TestRemoteDisconnectShouldPostCorrectDisconnectError()
    {
        using var cancelSource = new CancellationTokenSource();
        _clientMock.Setup(n =>
            n.ConnectAsync(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<bool>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()
            )
        ).Throws<OperationCanceledException>();

        var disconnectionTask = DefaultRunner.OnDisconnect
            .Timeout(TimeSpan.FromMilliseconds(TestSettings.DefaultTimeout), Observable.Return(default(DisconnectReason)))
            .FirstAsync()
            .ToTask();

        var runnerTask = DefaultRunner.RunAsync("host", 0, false, "token", TimeSpan.FromMilliseconds(100), cancelSource.Token);

        var reason = await disconnectionTask.ConfigureAwait(false);
        try
        {
            cancelSource.Cancel();
            await runnerTask.ConfigureAwait(false);
        }
        catch (OperationCanceledException) { } // ignore cancel error
        await DefaultRunner.DisposeAsync().ConfigureAwait(false);
        reason.Should().Be(DisconnectReason.Remote);
    }

    [Fact]
    public async Task TestDisposeShouldDisconnectGracefully()
    {
        var runnerTask = DefaultRunner.RunAsync("host", 0, false, "token", TimeSpan.FromMilliseconds(100), CancellationToken.None);

        await Task.Delay(500);

        // Dispose should never throw exception here
        await DefaultRunner.DisposeAsync().ConfigureAwait(false);

        await runnerTask.ConfigureAwait(false);

    }
}