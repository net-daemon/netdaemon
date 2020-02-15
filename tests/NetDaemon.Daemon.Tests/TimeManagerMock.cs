using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JoySoftware.HomeAssistant.NetDaemon.Daemon;
using Moq;

namespace NetDaemon.Daemon.Tests
{
    class TimeManagerMock : Mock<IManageTime>
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
