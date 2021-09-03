using Moq;
using NetDaemon.Daemon.Config;

namespace NetDaemon.Daemon.Tests.DaemonRunner
{
    public static class CommonTestMethods
    {
        public static Mock<IIoWrapper> GetIOWrapperMock(string text)
        {
            var ioWrapperMock = new Mock<IIoWrapper>();
            ioWrapperMock
                .Setup(x => x.ReadFile(It.IsAny<string>()))
                .Returns(() => text);

            return ioWrapperMock;
        }

        public static YamlConfigReader GetYamlConfigReader(string yamlTextToReturn)
        {
            return new YamlConfigReader(GetIOWrapperMock(yamlTextToReturn).Object);
        }

        public static YamlConfigReader GetYamlConfigReader()
        {
            return new YamlConfigReader(new IoWrapper());
        }
    }
}