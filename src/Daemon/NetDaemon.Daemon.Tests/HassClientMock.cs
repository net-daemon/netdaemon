using System.Collections.Concurrent;
using JoySoftware.HomeAssistant.Client;
using JoySoftware.HomeAssistant.NetDaemon.Common;
using Moq;
using Xunit;

namespace NetDaemon.Daemon.Tests
{
    public class HassClientMock : Mock<IHassClient>
    {
        internal ConcurrentDictionary<string, HassState> FakeStates = new ConcurrentDictionary<string, HassState>();

        public HassClientMock()
        {
            // Setup common mocks
            Setup(x => x.ConnectAsync(It.IsAny<string>(), It.IsAny<short>(), It.IsAny<bool>(),
                It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(true);
            SetupGet(x => x.States).Returns(FakeStates);

            SetupDefaultStates();
        }

        public static HassClientMock DefaultMock => new HassClientMock();

        /// <summary>
        ///     Returns a mock that will always return false when connect to Home Assistant
        /// </summary>
        public static HassClientMock MockConnectFalse
        {
            get
            {
                var mock = DefaultMock;
                mock.Setup(x => x.ConnectAsync(It.IsAny<string>(), It.IsAny<short>(), It.IsAny<bool>(),
                    It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(false);
                return mock;
            }
        }

        private void SetupDefaultStates()
        {
//            _states.a
        }

        public void AssertEqual(HassState hassState, EntityState entity)
        {
            Assert.Equal(hassState.EntityId, entity.EntityId);
            Assert.Equal(hassState.State, entity.State);
            Assert.Equal(hassState.LastChanged, entity.LastChanged);
            Assert.Equal(hassState.LastUpdated, entity.LastUpdated);

            foreach (var attribute in hassState.Attributes.Keys)
                Assert.Equal(hassState.Attributes[attribute], entity.Attributes[attribute]);
        }
    }
}