using Microsoft.Extensions.Logging;
using Moq;
using System;

namespace NetDaemon.Daemon.Fakes
{
    /// <summary>
    ///     Logger mock
    /// </summary>
    public class LoggerMock
    {
        /// <summary>
        ///     Public constructor
        /// </summary>
        public LoggerMock()
        {
            // Setup the mock
            MockLoggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(MockLogger.Object);
        }

        /// <summary>
        ///     Logger factory mock
        /// </summary>
        public ILoggerFactory LoggerFactory => MockLoggerFactory.Object;
        /// <summary>
        ///     Mock version of the logger factory mock
        /// </summary>
        public Mock<ILoggerFactory> MockLoggerFactory { get; } = new();

        /// <summary>
        ///     Logger of mock
        /// </summary>
        public ILogger Logger => MockLogger.Object;
        /// <summary>
        ///     Mock version of logger mock
        /// </summary>
        public Mock<ILogger> MockLogger { get; } = new();

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
                    It.Is<It.IsAnyType>((_, __) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception?, string>>((_, _) => true)), times);
        }

        /// <summary>
        ///     Assert if the log has been used at times
        /// </summary>
        /// <param name="level">The loglevel being checked</param>
        /// <param name="message">Message to send</param>
        /// <param name="times">The Times it has been logged</param>
        public void AssertLogged(LogLevel level, string message, Times times)
        {
            MockLogger.Verify(
                x => x.Log(
                    level,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, _) => v.ToString() == message),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception?, string>>((_, _) => true)), times);
        }

        /// <summary>
        ///     Assert if the log has been used at times
        /// </summary>
        /// <param name="level">The loglevel being checked</param>
        /// <param name="exception">Exception to log</param>
        /// <param name="message">Message to send</param>
        /// <param name="times">The Times it has been logged</param>
        public void AssertLogged(LogLevel level, Exception exception, string message, Times times)
        {
            MockLogger.Verify(
                x => x.Log(
                    level,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, _) => v.ToString() == message),
                    exception,
                    It.Is<Func<It.IsAnyType, Exception?, string>>((_, _) => true)), times);
        }
    }
}