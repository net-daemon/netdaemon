using System;
using System.Dynamic;
using System.Threading;
using System.Threading.Tasks;
using JoySoftware.HomeAssistant.NetDaemon.Daemon;
using Moq;
using Xunit;

namespace NetDaemon.Daemon.Tests
{
    public class FluentEventTests
    {
        [Fact]
        public async Task ACustomEventShouldDoCorrectCall()
        {
            // ARRANGE
            var hcMock = HassClientMock.DefaultMock;
            var daemonHost = new NetDaemonHost(hcMock.Object);
            dynamic dynObject = new ExpandoObject();
            dynObject.Test = "Hello World!";

            hcMock.AddCustomEvent("CUSTOM_EVENT", dynObject);

            var cancelSource = hcMock.GetSourceWithTimeout();
            var isCalled = false;
            var message = "";
            
            daemonHost
                .Event("CUSTOM_EVENT")
                    .Call(async (ev, data) => {
                        isCalled = true;
                        message = data.Test;
                    }).Execute();
       
            try
            {
                await daemonHost.Run("host", 8123, false, "token", cancelSource.Token);
            }
            catch (TaskCanceledException)
            {
                // Expected behaviour
            }

            Assert.True(isCalled);
            Assert.Equal("Hello World!", message);
        }

        [Fact]
        public async Task ACustomEventNullValueCallThrowsNullReferenceException()
        {
            // ARRANGE
            var hcMock = HassClientMock.DefaultMock;
            var daemonHost = new NetDaemonHost(hcMock.Object);

            var cancelSource = hcMock.GetSourceWithTimeout();

            Assert.Throws<NullReferenceException>(() => daemonHost
                .Event("CUSTOM_EVENT")
                    .Call(null).Execute());

        }

        [Fact]
        public async Task ACustomEventShouldUsingSelectorFuncDoCorrectCall()
        {
            // ARRANGE
            var hcMock = HassClientMock.DefaultMock;
            var daemonHost = new NetDaemonHost(hcMock.Object);
            dynamic dynObject = new ExpandoObject();
            dynObject.Test = "Hello World!";

            hcMock.AddCustomEvent("CUSTOM_EVENT", dynObject);

            var cancelSource = hcMock.GetSourceWithTimeout();
            var isCalled = false;
            var message = "";

            daemonHost
                .Events(n=>n.EventId=="CUSTOM_EVENT")
                    .Call(async (ev, data) => {
                        isCalled = true;
                        message = data.Test;
                    }).Execute();

            try
            {
                await daemonHost.Run("host", 8123, false, "token", cancelSource.Token);
            }
            catch (TaskCanceledException)
            {
                // Expected behaviour
            }

            Assert.True(isCalled);
            Assert.Equal("Hello World!", message);
        }

        [Fact]
        public async Task ACustomEventShouldUsingSelectorUsingDataFuncDoCorrectCall()
        {
            // ARRANGE
            var hcMock = HassClientMock.DefaultMock;
            var daemonHost = new NetDaemonHost(hcMock.Object);
            dynamic dynObject = new ExpandoObject();
            dynObject.Test = "Hello World!";

            hcMock.AddCustomEvent("CUSTOM_EVENT", dynObject);

            var cancelSource = hcMock.GetSourceWithTimeout();
            var isCalled = false;
            var message = "";

            daemonHost
                .Events(n => n.EventId == "CUSTOM_EVENT" && n.Data.Test == "Hello World!")
                    .Call(async (ev, data) => {
                        isCalled = true;
                        message = data.Test;
                    }).Execute();

            try
            {
                await daemonHost.Run("host", 8123, false, "token", cancelSource.Token);
            }
            catch (TaskCanceledException)
            {
                // Expected behaviour
            }

            Assert.True(isCalled);
            Assert.Equal("Hello World!", message);
        }

        [Fact]
        public async Task ACustomEventShouldUsingSelectorUsingDataFuncNotCall()
        {
            // ARRANGE
            var hcMock = HassClientMock.DefaultMock;
            var daemonHost = new NetDaemonHost(hcMock.Object);
            dynamic dynObject = new ExpandoObject();
            dynObject.Test = "Hello World!";

            hcMock.AddCustomEvent("CUSTOM_EVENT", dynObject);

            var cancelSource = hcMock.GetSourceWithTimeout();
            var isCalled = false;
            var message = "";

            daemonHost
                .Events(n => n.EventId == "CUSTOM_EVENT" && n.Data.Test == "Hello Test!")
                    .Call(async (ev, data) => {
                        isCalled = true;
                        message = data.Test;
                    }).Execute();

            try
            {
                await daemonHost.Run("host", 8123, false, "token", cancelSource.Token);
            }
            catch (TaskCanceledException)
            {
                // Expected behaviour
            }

            Assert.False(isCalled);
        }

        [Fact]
        public async Task ACustomEventShouldUsingSelectorUsingDataNotExisstFuncNotCall()
        {
            // ARRANGE
            var hcMock = HassClientMock.DefaultMock;
            var daemonHost = new NetDaemonHost(hcMock.Object);
            dynamic dynObject = new ExpandoObject();
            dynObject.Test = "Hello World!";

            hcMock.AddCustomEvent("CUSTOM_EVENT", dynObject);

            var cancelSource = hcMock.GetSourceWithTimeout();
            var isCalled = false;
            var message = "";

            daemonHost
                .Events(n => n.EventId == "CUSTOM_EVENT" && n.Data.NotExist == "Hello Test!")
                    .Call(async (ev, data) => {
                        isCalled = true;
                        message = data.Test;
                    }).Execute();

            try
            {
                await daemonHost.Run("host", 8123, false, "token", cancelSource.Token);
            }
            catch (TaskCanceledException)
            {
                // Expected behaviour
            }

            Assert.False(isCalled);
        }
    }
}
