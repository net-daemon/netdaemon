using System;
using System.Dynamic;
using System.Threading.Tasks;
using Moq;
using NetDaemon.Common.Exceptions;
using NetDaemon.Daemon.Fakes;
using NetDaemon.Daemon.Storage;
using Xunit;

namespace NetDaemon.Daemon.Tests.Fluent
{
    public class FluentEventTests
    {
        [Fact]
        public async Task ACustomEventNullValueCallThrowsNetDaemonNullReferenceException()
        {
            // ARRANGE
            var hcMock = HassClientMock.DefaultMock;
            await using var daemonHost = new NetDaemonHost(hcMock.DefaultHassClientFactoryMock.Object, new Mock<IDataRepository>().Object);

            var app = new FluentTestApp
            {
                Id = "id"
            };

            daemonHost.InternalRunningAppInstances[app.Id] = app;
            await app.StartUpAsync(daemonHost).ConfigureAwait(false);

            var cancelSource = HassClientMock.GetSourceWithTimeout();

            Assert.Throws<NetDaemonNullReferenceException>(() => app
                .Event("CUSTOM_EVENT")
                    .Call(null).Execute());
        }

        [Fact]
        public async Task ACustomEventShouldDoCorrectCall()
        {
            // ARRANGE
            var hcMock = HassClientMock.DefaultMock;
            await using var daemonHost = new NetDaemonHost(hcMock.DefaultHassClientFactoryMock.Object, new Mock<IDataRepository>().Object);

            var app = new FluentTestApp
            {
                Id = "id"
            };

            daemonHost.InternalRunningAppInstances[app.Id] = app;
            await app.StartUpAsync(daemonHost).ConfigureAwait(false);

            dynamic dynObject = new ExpandoObject();
            dynObject.Test = "Hello World!";

            hcMock.AddCustomEvent("CUSTOM_EVENT", dynObject);

            var cancelSource = HassClientMock.GetSourceWithTimeout();
            var isCalled = false;
            string? message = "";

            app
                .Event("CUSTOM_EVENT")
                    .Call((_, data) =>
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
                // Expected behavior
            }

            Assert.True(isCalled);
            Assert.Equal("Hello World!", message);
        }
        [Fact]
        public async Task ACustomEventShouldUsingSelectorFuncDoCorrectCall()
        {
            // ARRANGE
            var hcMock = HassClientMock.DefaultMock;
            await using var daemonHost = new NetDaemonHost(hcMock.DefaultHassClientFactoryMock.Object, new Mock<IDataRepository>().Object);
            var app = new FluentTestApp
            {
                Id = "id"
            };

            daemonHost.InternalRunningAppInstances[app.Id] = app;
            await app.StartUpAsync(daemonHost).ConfigureAwait(false);

            dynamic dynObject = new ExpandoObject();
            dynObject.Test = "Hello World!";

            hcMock.AddCustomEvent("CUSTOM_EVENT", dynObject);

            var cancelSource = HassClientMock.GetSourceWithTimeout();
            var isCalled = false;
            string? message = "";

            app
                .Events(n => n.EventId == "CUSTOM_EVENT")
                    .Call((_, data) =>
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
                // Expected behavior
            }

            Assert.True(isCalled);
            Assert.Equal("Hello World!", message);
        }

        [Fact]
        public async Task ACustomEventShouldUsingSelectorUsingDataFuncDoCorrectCall()
        {
            // ARRANGE
            var hcMock = HassClientMock.DefaultMock;
            await using var daemonHost = new NetDaemonHost(hcMock.DefaultHassClientFactoryMock.Object, new Mock<IDataRepository>().Object);
            var app = new FluentTestApp
            {
                Id = "id"
            };

            daemonHost.InternalRunningAppInstances[app.Id] = app;
            await app.StartUpAsync(daemonHost).ConfigureAwait(false);

            dynamic dynObject = new ExpandoObject();
            dynObject.Test = "Hello World!";

            hcMock.AddCustomEvent("CUSTOM_EVENT", dynObject);

            var cancelSource = HassClientMock.GetSourceWithTimeout();
            var isCalled = false;
            string? message = "";

            app
                .Events(n => n.EventId == "CUSTOM_EVENT" && n?.Data?.Test == "Hello World!")
                    .Call((_, data) =>
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
                // Expected behavior
            }

            Assert.True(isCalled);
            Assert.Equal("Hello World!", message);
        }

        [Fact]
        public async Task ACustomEventShouldUsingSelectorUsingDataFuncNotCall()
        {
            // ARRANGE
            var hcMock = HassClientMock.DefaultMock;
            await using var daemonHost = new NetDaemonHost(hcMock.DefaultHassClientFactoryMock.Object, new Mock<IDataRepository>().Object);
            var app = new FluentTestApp
            {
                Id = "id"
            };

            daemonHost.InternalRunningAppInstances[app.Id] = app;
            await app.StartUpAsync(daemonHost).ConfigureAwait(false);

            dynamic dynObject = new ExpandoObject();
            dynObject.Test = "Hello World!";

            hcMock.AddCustomEvent("CUSTOM_EVENT", dynObject);

            var cancelSource = HassClientMock.GetSourceWithTimeout();
            var isCalled = false;
            string? message = "";

            app
                .Events(n => n.EventId == "CUSTOM_EVENT" && n?.Data?.Test == "Hello Test!")
                    .Call((_, data) =>
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
                // Expected behavior
            }

            Assert.False(isCalled);
        }

        [Fact]
        public async Task ACustomEventShouldUsingSelectorUsingDataNotExisstFuncNotCall()
        {
            // ARRANGE
            var hcMock = HassClientMock.DefaultMock;
            await using var daemonHost = new NetDaemonHost(hcMock.DefaultHassClientFactoryMock.Object, new Mock<IDataRepository>().Object);
            var app = new FluentTestApp
            {
                Id = "id"
            };

            daemonHost.InternalRunningAppInstances[app.Id] = app;
            await app.StartUpAsync(daemonHost).ConfigureAwait(false);
            dynamic dynObject = new ExpandoObject();
            dynObject.Test = "Hello World!";

            hcMock.AddCustomEvent("CUSTOM_EVENT", dynObject);

            var cancelSource = HassClientMock.GetSourceWithTimeout();
            var isCalled = false;
            string? message = "";

            app
                .Events(n => n.EventId == "CUSTOM_EVENT" && n?.Data?.NotExist == "Hello Test!")
                    .Call((_, data) =>
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
                // Expected behavior
            }

            Assert.False(isCalled);
        }

        [Fact]
        public async Task ACustomEventsShouldDoCorrectCall()
        {
            // ARRANGE
            var hcMock = HassClientMock.DefaultMock;
            await using var daemonHost = new NetDaemonHost(hcMock.DefaultHassClientFactoryMock.Object, new Mock<IDataRepository>().Object);
            var app = new FluentTestApp
            {
                Id = "id"
            };

            daemonHost.InternalRunningAppInstances[app.Id] = app;
            await app.StartUpAsync(daemonHost).ConfigureAwait(false);

            dynamic dynObject = new ExpandoObject();
            dynObject.Test = "Hello World!";

            hcMock.AddCustomEvent("CUSTOM_EVENT", dynObject);

            var cancelSource = HassClientMock.GetSourceWithTimeout();
            var isCalled = false;
            string? message = "";

            app
                .Events(new string[] { "CUSTOM_EVENT" })
                    .Call((_, data) =>
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
                // Expected behavior
            }

            Assert.True(isCalled);
            Assert.Equal("Hello World!", message);
        }
    }

    public class FluentTestApp : NetDaemon.Common.NetDaemonApp { }
}