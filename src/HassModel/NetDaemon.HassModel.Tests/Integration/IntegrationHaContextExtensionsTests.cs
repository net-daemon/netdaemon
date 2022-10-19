using NetDaemon.HassModel.Entities;
using NetDaemon.HassModel.Integration;
using NetDaemon.HassModel.Tests.TestHelpers;
using NetDaemon.HassModel.Tests.TestHelpers.HassClient;

namespace NetDaemon.HassModel.Tests.Integration;

public class IntegrationHaContextExtensionsTests
{
    [Fact]
    public void RegsterServcieCallBackTest()
    {
        var haContextMock = new HaContextMock();
        var callBackMock = new Mock<Action<CallBackData>>();

        haContextMock.Object.RegisterServiceCallBack("ServiceName", callBackMock.Object);

        haContextMock.Verify(m => m.CallService("netdaemon", "register_service", null, It.IsAny<object?>()),
            Times.Once);

        haContextMock.EventsSubject.OnNext(new Event
        {
            EventType = "call_service",
            DataElement = new
            {
                domain = "netdaemon",
                service = "ServiceName",
                service_data = new {mode = "heat", temperature = 20.5}
            }.AsJsonElement()
        });

        callBackMock.Verify(m => m.Invoke(new CallBackData("heat", 20.5)));
    }

    [Fact]
    public void SetApplicationStateNotExistShouldCallCreateTest()
    {
        var haContextMock = new HaContextMock();
        haContextMock.Setup(n => n.GetState("sensor.mysensor")).Returns(default(EntityState?));
        haContextMock.Object.SetEntityState(
            "sensor.mysensor",
            "on",
            new
            {
                attr = "hello"
            });

        haContextMock.Verify(m => m.CallService("netdaemon", "entity_create", null, It.IsAny<object?>()), Times.Once);
    }

    [Fact]
    public void SetApplicationStateNotExistShouldCallUpdateTest()
    {
        var haContextMock = new HaContextMock();
        haContextMock.Setup(n => n.GetState("sensor.mysensor")).Returns(new EntityState());
        haContextMock.Object.SetEntityState(
            "sensor.mysensor",
            "on",
            new
            {
                attr = "hello"
            });

        haContextMock.Verify(m => m.CallService("netdaemon", "entity_update", null, It.IsAny<object?>()), Times.Once);
    }

    public record CallBackData(string mode, double temperature);
}