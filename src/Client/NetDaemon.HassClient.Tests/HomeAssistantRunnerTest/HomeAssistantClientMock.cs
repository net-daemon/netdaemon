using NetDaemon.HassClient.Tests.HomeAssistantClientTest;

namespace NNetDaemon.HassClient.Tests.HomeAssistantRunnerTest;

internal class HomeAssistantClientMock : Mock<IHomeAssistantClient>
{
    private readonly HomeAssistantConnectionMock _haConnectionMock = new();

    public HomeAssistantClientMock()
    {
        // Return a mock connection as default
        Setup(n =>
            n.ConnectAsync(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<bool>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()
            )
        ).Returns(
            (string _, int _, bool _, string _, CancellationToken _) =>
            {
                return Task.FromResult(_haConnectionMock.Object);
            }
        );

        _haConnectionMock.Setup(n =>
            n.ProcessHomeAssistantEventsAsync(It.IsAny<CancellationToken>())
        ).Returns(
            async (CancellationToken cancelToken) =>
            {
                await Task.Delay(TestSettings.DefaultTimeout, cancelToken).ConfigureAwait(false);
            }
        );
    }

}
