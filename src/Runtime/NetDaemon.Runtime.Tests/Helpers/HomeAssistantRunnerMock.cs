using System.Reactive.Subjects;
using NetDaemon.AppModel;

namespace NetDaemon.Runtime.Tests.Helpers;

internal class HomeAssistantRunnerMock : Mock<IHomeAssistantRunner>
{
    public HomeAssistantClientMock ClientMock { get; }
    public Subject<IHomeAssistantConnection> ConnectMock { get; }
    public Subject<DisconnectReason> DisconnectMock { get; }
    public HomeAssistantRunnerMock(CancellationToken cancelToken)
    {
        ConnectMock = new();
        DisconnectMock = new();
        ClientMock = new();
        SetupGet(n => n.CurrentConnection).Returns(ClientMock.ConnectionMock.Object);
        SetupGet(n => n.OnConnect).Returns(ConnectMock);
        SetupGet(n => n.OnDisconnect).Returns(DisconnectMock);

        Setup(n => n.RunAsync(
            It.IsAny<string>(),
            It.IsAny<int>(),
            It.IsAny<bool>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<TimeSpan>(),
            It.IsAny<CancellationToken>())).Returns(
            async () =>
            {
                await Task.Delay(-1, cancelToken);
            }
            );
    }
}

internal class HomeAssistantClientMock : Mock<IHomeAssistantClient>
{
    public HomeAssistantConnectionMock ConnectionMock { get; }

    public HomeAssistantClientMock()
    {
        ConnectionMock = new();
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
                return Task.FromResult(ConnectionMock.Object);
            }
        );

        ConnectionMock.Setup(n =>
            n.ProcessHomeAssistantEventsAsync(It.IsAny<CancellationToken>())
        ).Returns(
            async (CancellationToken cancelToken) =>
            {
                await Task.Delay(TestSettings.DefaultTimeout, cancelToken).ConfigureAwait(false);
            }
        );
    }
}

internal class HomeAssistantConnectionMock : Mock<IHomeAssistantConnection>
{
    public Subject<HassEvent> HomeAssistantEventMock { get; }

    public HomeAssistantConnectionMock()
    {
        HomeAssistantEventMock = new();
        SetupGet(n => n.OnHomeAssistantEvent).Returns(HomeAssistantEventMock);
    }

    internal void AddStateChangeEvent(HassState oldState, HassState newState)
    {
        var data = new HassStateChangedEventData
        {
            EntityId = newState.EntityId,
            NewState = newState,
            OldState = oldState
        };

        try
        {
            HomeAssistantEventMock.OnNext(
                new HassEvent
                {
                    EventType = "state_changed",
                    DataElement = data.ToJsonElement()
                }
            );
        }
        catch (Exception e)
        {
            System.Console.WriteLine(e.Message);
        }
    }

}

internal class FakeOptions<T> : IOptions<T> where T: class, new()
{
    public FakeOptions(T settings)
    {
        Value = settings;
    }

    public T Value { get; init; }
}