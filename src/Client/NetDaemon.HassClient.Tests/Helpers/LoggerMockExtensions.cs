namespace NetDaemon.HassClient.Tests.Helpers;

public static class LoggerMockExtensions
{
    public static Mock<ILogger<T>> VerifyWarningWasCalled<T>(this Mock<ILogger<T>> logger, string expectedMessage)
    {
        Func<object, Type, bool> state = (v, t) => v.ToString().CompareTo(expectedMessage) == 0;

        logger.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Warning),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => state(v, t)),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));
        return logger;
    }

    public static Mock<ILogger<T>> VerifyErrorWasCalled<T>(this Mock<ILogger<T>> logger, string expectedMessage)
    {
        Func<object, Type, bool> state = (v, t) => v.ToString().CompareTo(expectedMessage) == 0;

        logger.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => state(v, t)),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)));
        return logger;
    }
}
