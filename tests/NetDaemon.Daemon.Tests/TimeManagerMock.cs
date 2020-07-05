using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NetDaemon.Daemon.Tests
{
    internal class TimeManagerMock : Mock<IManageTime>
    {
        private readonly DateTime _time = DateTime.Now;

        public TimeManagerMock(DateTime time)
        {
            _time = time;
            SetupGet(n => n.Current).Returns(_time);
            Setup(n => n.Delay(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .Returns(async (TimeSpan s, CancellationToken c) => await Task.Delay(s, c));
        }

        public TimeManagerMock()
        {
            SetupGet(n => n.Current).Returns(_time);
            Setup(n => n.Delay(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .Returns(async (TimeSpan s, CancellationToken c) => await Task.Delay(s, c));
        }
    }
}