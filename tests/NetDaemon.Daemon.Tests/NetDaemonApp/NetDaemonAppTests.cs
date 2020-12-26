using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Reflection;
using System.Threading.Tasks;
using NetDaemon.Common;
using NetDaemon.Common.Fluent;
using Xunit;
using NetDaemon.Daemon.Fakes;
using NetDaemon.Common.Exceptions;

namespace NetDaemon.Daemon.Tests.NetDaemonApp
{
    public class AppTestApp : Common.NetDaemonApp { }

    public class AppTestApp2 : Common.NetDaemonApp { }

    public class NetDaemonApptests
    {
        private const string appTemplate = "  app: ";
        private readonly LoggerMock _logMock;
        private readonly Common.NetDaemonApp _app;
        private readonly Mock<INetDaemon> _netDaemonMock;

        public NetDaemonApptests()
        {
            _logMock = new LoggerMock();
            _netDaemonMock = new Mock<INetDaemon>();
            _netDaemonMock.SetupGet(n => n.Logger).Returns(_logMock.Logger);

            AppMock = new Mock<INetDaemonApp>();
            _app = new AppTestApp();
            _app.StartUpAsync(_netDaemonMock.Object);
            _app.Id = "app";
        }

        public Mock<INetDaemonApp> AppMock { get; }

        [Fact]
        public void CallServiceShouldCallCorrectDaemonCallService()
        {
            // ARRANGE and  ACT
            var expandoData = new FluentExpandoObject();
            dynamic data = expandoData;
            data.AnyData = "data";

            // ACT
            _app.CallService("domain", "service", data, false);

            // ASSERT
            _netDaemonMock.Verify(n => n.CallServiceAsync("domain", "service", expandoData, false));
        }

        [Fact]
        public void CamerasFuncShouldCallCorrectDaemonEntity()
        {
            // ARRANGE and  ACT
            _app.Cameras(n => n.EntityId == "camera.cam1");
            // ASSERT
            _netDaemonMock.Verify(n => n.Cameras(_app, It.IsAny<Func<IEntityProperties, bool>>()));
        }

        [Fact]
        public void CameraShouldCallCorrectDaemonEntity()
        {
            // ARRANGE and  ACT
            _app.Camera("camera.cam1");
            // ASSERT
            _netDaemonMock.Verify(n => n.Camera(_app, "camera.cam1"));
        }

        [Fact]
        public void CamerasShouldCallCorrectDaemonEntity()
        {
            // ARRANGE and  ACT
            _app.Cameras(new string[] { "camera.cam1" });
            // ASSERT
            _netDaemonMock.Verify(n => n.Cameras(_app, new string[] { "camera.cam1" }));
        }

        [Fact]
        public void EnitiesFuncShouldCallCorrectDaemonEntity()
        {
            // ARRANGE and  ACT
            _app.Entities(n => n.EntityId == "light.somelight");
            // ASSERT
            _netDaemonMock.Verify(n => n.Entities(_app, It.IsAny<Func<IEntityProperties, bool>>()));
        }

        [Fact]
        public void EntitiesShouldCallCorrectDaemonEntity()
        {
            // ARRANGE and  ACT
            _app.Entities(new string[] { "light.somelight" });
            // ASSERT
            _netDaemonMock.Verify(n => n.Entities(_app, new string[] { "light.somelight" }));
        }

        [Fact]
        public void EntityShouldCallCorrectDaemonEntity()
        {
            // ARRANGE and  ACT
            _app.Entity("light.somelight");
            // ASSERT
            _netDaemonMock.Verify(n => n.Entity(_app, "light.somelight"));
        }

        [Fact]
        public void GetStateShouldCallCorrectDaemonGetState()
        {
            // ARRANGE and  ACT
            _app.GetState("entityid");

            // ASSERT
            _netDaemonMock.Verify(n => n.GetState("entityid"));
        }

        [Theory]
        [InlineData("int", 10)]
        [InlineData("str", "hello")]
        public async Task GlobalShouldReturnCorrectData(string key, object value)
        {
            await using var _app_two = new AppTestApp2();
            await _app_two.StartUpAsync(_netDaemonMock.Object).ConfigureAwait(false);
            _app_two.Id = "app2";

            // ARRANGE and  ACT
            _app.Global[key] = value;

            // ASSERT
            Assert.Equal(_app_two.Global[key], value);
        }

        [Theory]
        [InlineData(LogLevel.Information, "Log")]
        [InlineData(LogLevel.Warning, "LogWarning")]
        [InlineData(LogLevel.Error, "LogError")]
        [InlineData(LogLevel.Trace, "LogTrace")]
        [InlineData(LogLevel.Debug, "LogDebug")]
        public void LogMessageWithDifferentLogLevelsShoulCallCorrectLogger(LogLevel level, string methodName)
        {
            // ARRANGE
            const string? message = "message";

            // ACT
            MethodInfo? methodInfo = _app.GetType().GetMethod(methodName, new Type[] { typeof(string) });

            methodInfo?.Invoke(_app, new object[] { message });
            // ASSERT
            _logMock.AssertLogged(level, appTemplate + message, Times.Once());
        }

        [Theory]
        [InlineData(LogLevel.Information, "Log")]
        [InlineData(LogLevel.Warning, "LogWarning")]
        [InlineData(LogLevel.Error, "LogError")]
        [InlineData(LogLevel.Trace, "LogTrace")]
        [InlineData(LogLevel.Debug, "LogDebug")]
        public void LogMessageWithExceptionAndDifferentLogLevelsShoulCallCorrectLogger(LogLevel level, string methodName)
        {
            // ARRANGE
            const string? message = "message";
            var exception = new NetDaemonNullReferenceException("Null");
            // ACT
            var methodInfo = _app.GetType().GetMethod(methodName, new Type[] { typeof(Exception), typeof(string) });

            methodInfo?.Invoke(_app, new object[] { exception, message });
            // ASSERT
            _logMock.AssertLogged(level, exception, appTemplate + message, Times.Once());
        }

        [Theory]
        [InlineData(LogLevel.Information, "Log")]
        [InlineData(LogLevel.Warning, "LogWarning")]
        [InlineData(LogLevel.Error, "LogError")]
        [InlineData(LogLevel.Trace, "LogTrace")]
        [InlineData(LogLevel.Debug, "LogDebug")]
        public void LogMessageWithParamsAndDifferentLogLevelsShoulCallCorrectLogger(LogLevel level, string methodName)
        {
            // ARRANGE
            const string? message = "Hello {name}";

            // ACT
            var methodInfo = _app.GetType().GetMethod(methodName, new Type[] { typeof(string), typeof(object[]) });

            methodInfo?.Invoke(_app, new object[] { message, new object[] { "Bob" } });
            // ASSERT
            _logMock.AssertLogged(level, appTemplate + "Hello Bob", Times.Once());
        }

        [Theory]
        [InlineData(LogLevel.Information, "Log")]
        [InlineData(LogLevel.Warning, "LogWarning")]
        [InlineData(LogLevel.Error, "LogError")]
        [InlineData(LogLevel.Trace, "LogTrace")]
        [InlineData(LogLevel.Debug, "LogDebug")]
        public void LogMessageWithParamsExceptionAndDifferentLogLevelsShoulCallCorrectLogger(LogLevel level, string methodName)
        {
            // ARRANGE
            const string? message = "Hello {name}";
            var exception = new NetDaemonNullReferenceException("Null");
            // ACT
            var methodInfo = _app.GetType().GetMethod(methodName, new Type[] { typeof(Exception), typeof(string), typeof(object[]) });

            methodInfo?.Invoke(_app, new object[] { exception, message, new object[] { "Bob" } });
            // ASSERT
            _logMock.AssertLogged(level, exception, appTemplate + "Hello Bob", Times.Once());
        }
    }
}