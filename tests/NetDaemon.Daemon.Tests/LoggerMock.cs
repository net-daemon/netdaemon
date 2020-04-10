using Microsoft.Extensions.Logging;
using Moq;
using System;

namespace NetDaemon.Daemon.Tests
{
    public class LoggerMock
    {
        public LoggerMock()
        {
            // Setup the mock
            MockLoggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(MockLogger.Object);
        }

        public ILoggerFactory LoggerFactory => MockLoggerFactory.Object;
        public Mock<ILoggerFactory> MockLoggerFactory { get; } = new Mock<ILoggerFactory>();

        public ILogger Logger => MockLogger.Object;
        public Mock<ILogger> MockLogger { get; } = new Mock<ILogger>();

        /// <summary>
        ///     Assert if the log has been used at times
        /// </summary>
        /// <param name="level">The loglevel being checked</param>
        /// <param name="times">The Times it has been logged</param>
        public void AssertLogged(LogLevel level, Times times)
        {
            MockLogger.Verify(
                x => x.Log(
                    level,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), times);
        }

        /// <summary>
        ///     Assert if the log has been used at times
        /// </summary>
        /// <param name="level">The loglevel being checked</param>
        /// <param name="times">The Times it has been logged</param>
        public void AssertLogged(LogLevel level, string message, Times times)
        {
            MockLogger.Verify(
                x => x.Log(
                    level,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == message),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), times);
        }

        /// <summary>
        ///     Assert if the log has been used at times
        /// </summary>
        /// <param name="level">The loglevel being checked</param>
        /// <param name="times">The Times it has been logged</param>
        public void AssertLogged(LogLevel level, Exception exception, string message, Times times)
        {
            MockLogger.Verify(
                x => x.Log(
                    level,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == message),
                    exception,
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), times);
        }
    }
}