using JoySoftware.HomeAssistant.NetDaemon.Daemon;
using JoySoftware.HomeAssistant.NetDaemon.Daemon.Storage;
using Moq;
using System;
using System.Dynamic;
using System.Threading.Tasks;
using Xunit;

namespace NetDaemon.Daemon.Tests
{
    public class FluentEventTests
    {
        [Fact]
        public async Task ACustomEventNullValueCallThrowsNullReferenceException()
        {
            // ARRANGE
            var hcMock = HassClientMock.DefaultMock;
            await using var daemonHost = new NetDaemonHost(new Mock<IInstanceDaemonApp>().Object, hcMock.Object, new Mock<IDataRepository>().Object);
            var app = new FluentTestApp();
            app.Id = "id";

            daemonHost.InternalRunningAppInstances[app.Id] = app;
            await app.StartUpAsync(daemonHost);

            var cancelSource = hcMock.GetSourceWithTimeout();

            Assert.Throws<NullReferenceException>(() => app
                .Event("CUSTOM_EVENT")
                    .Call(null).Execute());
        }

        [Fact]
        public async Task ACustomEventShouldDoCorrectCall()
        {
            // ARRANGE
            var hcMock = HassClientMock.DefaultMock;
            await using var daemonHost = new NetDaemonHost(new Mock<IInstanceDaemonApp>().Object, hcMock.Object, new Mock<IDataRepository>().Object);

            var app = new FluentTestApp();
            app.Id = "id";

            daemonHost.InternalRunningAppInstances[app.Id] = app;
            await app.StartUpAsync(daemonHost);

            dynamic dynObject = new ExpandoObject();
            dynObject.Test = "Hello World!";

            hcMock.AddCustomEvent("CUSTOM_EVENT", dynObject);

            var cancelSource = hcMock.GetSourceWithTimeout();
            var isCalled = false;
            string? message = "";

            app
                .Event("CUSTOM_EVENT")
                    .Call((ev, data) =>
                    {
                        isCalled = true;
                        message = data?.Test;
                        return Task.CompletedTask;
                    }).Execute();

            try
            {
                await daemonHost.Run("host", 8123, false, "token", cancelSource.Token).ConfigureAwait(false);
            }
            catch (TaskCanceledException)
            {
                // Expected behaviour
            }

            Assert.True(isCalled);
            Assert.Equal("Hello World!", message);
        }
        [Fact]
        public async Task ACustomEventShouldUsingSelectorFuncDoCorrectCall()
        {
            // ARRANGE
            var hcMock = HassClientMock.DefaultMock;
            await using var daemonHost = new NetDaemonHost(new Mock<IInstanceDaemonApp>().Object, hcMock.Object, new Mock<IDataRepository>().Object);
            var app = new FluentTestApp();
            app.Id = "id";

            daemonHost.InternalRunningAppInstances[app.Id] = app;
            await app.StartUpAsync(daemonHost);

            dynamic dynObject = new ExpandoObject();
            dynObject.Test = "Hello World!";

            hcMock.AddCustomEvent("CUSTOM_EVENT", dynObject);

            var cancelSource = hcMock.GetSourceWithTimeout();
            var isCalled = false;
            string? message = "";

            app
                .Events(n => n.EventId == "CUSTOM_EVENT")
                    .Call((ev, data) =>
                    {
                        isCalled = true;
                        message = data?.Test;
                        return Task.CompletedTask;
                    }).Execute();

            try
            {
                await daemonHost.Run("host", 8123, false, "token", cancelSource.Token).ConfigureAwait(false);
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
            await using var daemonHost = new NetDaemonHost(new Mock<IInstanceDaemonApp>().Object, hcMock.Object, new Mock<IDataRepository>().Object);
            var app = new FluentTestApp();
            app.Id = "id";

            daemonHost.InternalRunningAppInstances[app.Id] = app;
            await app.StartUpAsync(daemonHost);

            dynamic dynObject = new ExpandoObject();
            dynObject.Test = "Hello World!";

            hcMock.AddCustomEvent("CUSTOM_EVENT", dynObject);

            var cancelSource = hcMock.GetSourceWithTimeout();
            var isCalled = false;
            string? message = "";

            app
                .Events(n => n.EventId == "CUSTOM_EVENT" && n?.Data?.Test == "Hello World!")
                    .Call((ev, data) =>
                    {
                        isCalled = true;
                        message = data?.Test;
                        return Task.CompletedTask;
                    }).Execute();

            try
            {
                await daemonHost.Run("host", 8123, false, "token", cancelSource.Token).ConfigureAwait(false);
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
            await using var daemonHost = new NetDaemonHost(new Mock<IInstanceDaemonApp>().Object, hcMock.Object, new Mock<IDataRepository>().Object);
            var app = new FluentTestApp();
            app.Id = "id";

            daemonHost.InternalRunningAppInstances[app.Id] = app;
            await app.StartUpAsync(daemonHost);

            dynamic dynObject = new ExpandoObject();
            dynObject.Test = "Hello World!";

            hcMock.AddCustomEvent("CUSTOM_EVENT", dynObject);

            var cancelSource = hcMock.GetSourceWithTimeout();
            var isCalled = false;
            string? message = "";

            app
                .Events(n => n.EventId == "CUSTOM_EVENT" && n?.Data?.Test == "Hello Test!")
                    .Call((ev, data) =>
                    {
                        isCalled = true;
                        message = data?.Test;
                        return Task.CompletedTask;
                    }).Execute();

            try
            {
                await daemonHost.Run("host", 8123, false, "token", cancelSource.Token).ConfigureAwait(false);
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
            await using var daemonHost = new NetDaemonHost(new Mock<IInstanceDaemonApp>().Object, hcMock.Object, new Mock<IDataRepository>().Object);
            var app = new FluentTestApp();
            app.Id = "id";

            daemonHost.InternalRunningAppInstances[app.Id] = app;
            await app.StartUpAsync(daemonHost);
            dynamic dynObject = new ExpandoObject();
            dynObject.Test = "Hello World!";

            hcMock.AddCustomEvent("CUSTOM_EVENT", dynObject);

            var cancelSource = hcMock.GetSourceWithTimeout();
            var isCalled = false;
            string? message = "";

            app
                .Events(n => n.EventId == "CUSTOM_EVENT" && n?.Data?.NotExist == "Hello Test!")
                    .Call((ev, data) =>
                    {
                        isCalled = true;
                        message = data?.Test;
                        return Task.CompletedTask;
                    }).Execute();

            try
            {
                await daemonHost.Run("host", 8123, false, "token", cancelSource.Token).ConfigureAwait(false);
            }
            catch (TaskCanceledException)
            {
                // Expected behaviour
            }

            Assert.False(isCalled);
        }

        [Fact]
        public async Task ACustomEventsShouldDoCorrectCall()
        {
            // ARRANGE
            var hcMock = HassClientMock.DefaultMock;
            await using var daemonHost = new NetDaemonHost(new Mock<IInstanceDaemonApp>().Object, hcMock.Object, new Mock<IDataRepository>().Object);
            var app = new FluentTestApp();
            app.Id = "id";

            daemonHost.InternalRunningAppInstances[app.Id] = app;
            await app.StartUpAsync(daemonHost);

            dynamic dynObject = new ExpandoObject();
            dynObject.Test = "Hello World!";

            hcMock.AddCustomEvent("CUSTOM_EVENT", dynObject);

            var cancelSource = hcMock.GetSourceWithTimeout();
            var isCalled = false;
            string? message = "";

            app
                .Events(new string[] { "CUSTOM_EVENT" })
                    .Call((ev, data) =>
                    {
                        isCalled = true;
                        message = data?.Test;
                        return Task.CompletedTask;
                    }).Execute();

            try
            {
                await daemonHost.Run("host", 8123, false, "token", cancelSource.Token).ConfigureAwait(false);
            }
            catch (TaskCanceledException)
            {
                // Expected behaviour
            }

            Assert.True(isCalled);
            Assert.Equal("Hello World!", message);
        }
    }

    public class FluentTestApp : NetDaemon.Common.NetDaemonApp { }
}