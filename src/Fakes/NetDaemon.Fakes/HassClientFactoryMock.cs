using Moq;

namespace NetDaemon.Daemon.Fakes
{
    /// <summary>
    ///     Mocking the HassClientFactory
    /// </summary>
    public class HassClientFactoryMock : Mock<IHassClientFactory>
    {
        private readonly HassClientMock _hassClientMock;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="hassClientMock">The hass client mock to return or null</param>
        public HassClientFactoryMock(HassClientMock? hassClientMock = null)
        {
            _hassClientMock = hassClientMock ?? new HassClientMock();
            Setup(n => n.New()).Returns(_hassClientMock.Object);
        }
    }
}