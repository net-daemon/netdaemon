using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Reflection;
using System.Threading.Tasks;
using NetDaemon.Common;
using Xunit;
using NetDaemon.Daemon.Fakes;
using NetDaemon.Common.Exceptions;
using NetDaemon.Common.Reactive;

namespace NetDaemon.Daemon.Tests.NetDaemonApp
{
    class TestRxApp : NetDaemonRxApp { }

    public class NetDaemonApptests : IAsyncLifetime, IDisposable
    {
        private const string appTemplate = "  app: ";
        private readonly LoggerMock _logMock;
        private readonly Common.NetDaemonAppBase _app = new TestRxApp();
        private readonly Mock<INetDaemon> _netDaemonMock;
        private bool disposedValue;

        public NetDaemonApptests()
        {
            _logMock = new LoggerMock();
            _netDaemonMock = new Mock<INetDaemon>();
            _netDaemonMock.SetupGet(n => n.Logger).Returns(_logMock.Logger);

            _app.StartUpAsync(_netDaemonMock.Object);
            _app.Id = "app";
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

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        public async Task DisposeAsync()
        {
            await _app.DisposeAsync().ConfigureAwait(false);
        }
    }
}