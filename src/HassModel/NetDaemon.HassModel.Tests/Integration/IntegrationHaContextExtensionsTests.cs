using NetDaemon.HassModel.Entities;
using NetDaemon.HassModel.Integration;
using NetDaemon.HassModel.Tests.TestHelpers;
using NetDaemon.HassModel.Tests.TestHelpers.HassClient;

namespace NetDaemon.HassModel.Tests.Integration;

public class IntegrationHaContextExtensionsTests
{
    [Fact]
    public void RegisterServiceCallBackTest()
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
    public void RegisterServiceCallBackShouldThrowInformativeExceptionWhenNetDaemonIntegrationNotInstalled()
    {
        var haContextMock = new HaContextMock();
        var callBackMock = new Mock<Action<CallBackData>>();

        // Setup mock to throw an exception that simulates NetDaemon integration not being installed
        haContextMock.Setup(m => m.CallService("netdaemon", "register_service", null, It.IsAny<object?>()))
            .Throws(new Exception("Service netdaemon not found"));

        var exception = Assert.Throws<InvalidOperationException>(() =>
            haContextMock.Object.RegisterServiceCallBack("ServiceName", callBackMock.Object));

        Assert.Contains("NetDaemon integration is not installed", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Please install the NetDaemon integration from HACS", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("https://github.com/net-daemon/netdaemon", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void RegisterServiceShouldThrowInformativeExceptionWithVariousErrorMessages()
    {
        var haContextMock = new HaContextMock();

        // Test different error message variations that indicate the integration is not installed
        var errorMessages = new[]
        {
            "Service netdaemon not found",
            "Service netdaemon does not exist", 
            "Service netdaemon unknown",
            "Service netdaemon not available",
            "Invalid service netdaemon.register_service",
            "Domain netdaemon not found",
            "NetDaemon service does not exist"
        };

        foreach (var errorMessage in errorMessages)
        {
            haContextMock.Reset();
            haContextMock.Setup(m => m.CallService("netdaemon", "register_service", null, It.IsAny<object?>()))
                .Throws(new Exception(errorMessage));

            var exception = Assert.Throws<InvalidOperationException>(() =>
                haContextMock.Object.RegisterService<CallBackData>("TestService"));

            Assert.Contains("NetDaemon integration is not installed", exception.Message, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void RegisterServiceShouldNotCatchUnrelatedExceptions()
    {
        var haContextMock = new HaContextMock();

        // Setup mock to throw an unrelated exception
        haContextMock.Setup(m => m.CallService("netdaemon", "register_service", null, It.IsAny<object?>()))
            .Throws(new ArgumentException("Some other error"));

        // Should not catch unrelated exceptions
        Assert.Throws<ArgumentException>(() =>
            haContextMock.Object.RegisterService<CallBackData>("TestService"));
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
