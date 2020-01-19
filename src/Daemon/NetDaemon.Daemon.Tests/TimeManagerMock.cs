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
        public DateTime CurrentTime { get; set; } = DateTime.Now;

        public TimeManagerMock()
        {
            SetupGet(n => n.Current).Returns(CurrentTime);
            Setup(n => n.Delay(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .Returns( async (TimeSpan s, CancellationToken c) => await Task.Delay(s, c));
        }
    }
}
