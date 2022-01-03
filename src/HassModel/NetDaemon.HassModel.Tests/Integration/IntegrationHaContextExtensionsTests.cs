using System;
using Moq;
using NetDaemon.HassModel.Common;
using NetDaemon.HassModel.Integration;
using NetDaemon.HassModel.Tests.Entities;
using NetDaemon.HassModel.Tests.TestHelpers;
using NetDaemon.HassModel.Tests.TestHelpers.HassClient;
using Xunit;

namespace NetDaemon.HassModel.Tests.Integration;

public class IntegrationHaContextExtensionsTests
{
    [Fact]
    public void RegsterServcieCallBackTest()
    {
        var haContextMock = new HaContextMock();
        var callBackMock = new Mock<Action<CallBackData>>();

        haContextMock.Object.RegisterServiceCallBack("ServiceName", callBackMock.Object);

        haContextMock.Verify(m => m.CallService("netdaemon", "register_service", null, It.IsAny<object?>()), Times.Once);

        haContextMock.EventsSubject.OnNext(new Event()
        {
            EventType = "call_service",
            DataElement = new
            {
                domain = "netdaemon",
                service = "ServiceName",
                service_data = new { mode = "heat", temperature = 20.5 }
            }.AsJsonElement()
        });

        callBackMock.Verify(m => m.Invoke(new CallBackData("heat", 20.5)));
    }

    public record CallBackData(string mode, double temperature);
}