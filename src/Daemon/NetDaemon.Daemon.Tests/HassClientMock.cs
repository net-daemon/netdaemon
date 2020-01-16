using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
            FakeStates["light.correct_entity"] = new HassState()
            {
                EntityId = "light.correct_entity",
                Attributes = new Dictionary<string, object>()
                {
                    ["entity_id"] = "light.correct_entity",
                    ["test"] = 100
                },

            };

            FakeStates["light.correct_entity2"] = new HassState()
            {
                EntityId = "light.correct_entity2",
                Attributes = new Dictionary<string, object>()
                {
                    ["entity_id"] = "light.correct_entity2",
                    ["test"] = 101
                },

            };

            FakeStates["light.filtered_entity"] = new HassState()
            {
                EntityId = "light.filtered_entity",
                Attributes = new Dictionary<string, object>()
                {
                    ["entity_id"] = "light.filtered_entity",
                    ["test"] = 90
                },

            };
        }

        public void AssertEqual(HassState hassState, EntityState entity)
        {
            Assert.Equal(hassState.EntityId, entity.EntityId);
            Assert.Equal(hassState.State, entity.State);
            Assert.Equal(hassState.LastChanged, entity.LastChanged);
            Assert.Equal(hassState.LastUpdated, entity.LastUpdated);

            foreach (var attribute in hassState.Attributes.Keys)
                Assert.Equal(hassState.Attributes[attribute], ((IDictionary<string, object>) entity.Attribute)[attribute]);
        }
    }
}   