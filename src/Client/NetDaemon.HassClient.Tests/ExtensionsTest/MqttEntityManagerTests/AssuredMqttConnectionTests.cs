using MQTTnet;
using MQTTnet.Packets;
using NetDaemon.Extensions.MqttEntityManager;

namespace NetDaemon.HassClient.Tests.ExtensionsTest.MqttEntityManagerTests;

public class AssuredMqttConnectionTests
{
    [Fact]
    public async Task QueuesPublishUntilConnected()
    {
        var setup = new AssuredMqttConnectionSetup();
        using var conn = setup.CreateConnection();

        var message = new MqttApplicationMessageBuilder()
            .WithTopic("topic")
            .WithPayload("payload")
            .Build();

        await conn.PublishAsync(message);
        setup.PublishedMessages.Should().BeEmpty();

        setup.CompleteConnect();

        var publishedMessage = await setup.WaitForPublishedMessageAsync();
        publishedMessage.Should().Be(message);
    }

    [Fact]
    public async Task ReplaysSubscriptionsAfterConnect()
    {
        var setup = new AssuredMqttConnectionSetup();
        using var conn = setup.CreateConnection();

        var topicFilter = new MqttTopicFilterBuilder()
            .WithTopic("homeassistant/domain/sensor/set")
            .Build();

        await conn.SubscribeAsync(topicFilter);
        setup.SubscribedTopics.Should().BeEmpty();

        setup.CompleteConnect();

        var subscribedTopic = await setup.WaitForSubscribedTopicAsync();
        subscribedTopic.Should().Be("homeassistant/domain/sensor/set");
    }

    [Fact]
    public async Task PublishesImmediatelyWhenAlreadyConnected()
    {
        var setup = new AssuredMqttConnectionSetup();
        using var conn = setup.CreateConnection();
        setup.CompleteConnect();
        await setup.WaitForConnectedAsync();

        var message = new MqttApplicationMessageBuilder()
            .WithTopic("topic")
            .WithPayload("payload")
            .Build();

        await conn.PublishAsync(message);

        var publishedMessage = await setup.WaitForPublishedMessageAsync();
        publishedMessage.Should().Be(message);
    }

    private sealed class AssuredMqttConnectionSetup
    {
        private readonly TaskCompletionSource _connectCompletion = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource _connected = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource<MqttApplicationMessage> _publishedMessage = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource<string> _subscribedTopic = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly Mock<IMqttClient> _mqttClient = new();
        private readonly Mock<IMqttFactory> _mqttFactory = new();
        private readonly Mock<IMqttClientOptionsFactory> _mqttClientOptionsFactory = new();
        private readonly Mock<IOptions<MqttConfiguration>> _mqttConfigurationOptions = new();

        public AssuredMqttConnectionSetup()
        {
            var mqttConfiguration = new MqttConfiguration
            {
                Host = "localhost",
                UserName = "id"
            };

            var clientOptions = new MqttClientOptionsBuilder()
                .WithTcpServer(mqttConfiguration.Host, mqttConfiguration.Port)
                .Build();

            _mqttConfigurationOptions.SetupGet(o => o.Value)
                .Returns(mqttConfiguration);

            _mqttClientOptionsFactory.Setup(f => f.CreateClientOptions(mqttConfiguration))
                .Returns(clientOptions);

            _mqttFactory.Setup(f => f.CreateMqttClient())
                .Returns(_mqttClient.Object);

            _mqttClient.SetupGet(c => c.IsConnected)
                .Returns(() => IsConnected);

            _mqttClient.Setup(c => c.ConnectAsync(clientOptions, It.IsAny<CancellationToken>()))
                .Returns(async () =>
                {
                    await _connectCompletion.Task;
                    IsConnected = true;
                    _connected.TrySetResult();
                    return new MqttClientConnectResult
                    {
                        ResultCode = MqttClientConnectResultCode.Success
                    };
                });

            _mqttClient.Setup(c => c.PublishAsync(It.IsAny<MqttApplicationMessage>(), It.IsAny<CancellationToken>()))
                .Callback<MqttApplicationMessage, CancellationToken>((message, _) =>
                {
                    PublishedMessages.Add(message);
                    _publishedMessage.TrySetResult(message);
                })
                .ReturnsAsync(() => new MqttClientPublishResult(null, MqttClientPublishReasonCode.Success, string.Empty, []));

            _mqttClient.Setup(c => c.SubscribeAsync(It.IsAny<MqttClientSubscribeOptions>(), It.IsAny<CancellationToken>()))
                .Callback<MqttClientSubscribeOptions, CancellationToken>((options, _) =>
                {
                    foreach (var topic in options.TopicFilters.Select(topicFilter => topicFilter.Topic))
                    {
                        SubscribedTopics.Add(topic);
                        _subscribedTopic.TrySetResult(topic);
                    }
                })
                .ReturnsAsync(() => new MqttClientSubscribeResult(1, [], string.Empty, []));
        }

        public bool IsConnected { get; private set; }

        public List<MqttApplicationMessage> PublishedMessages { get; } = [];

        public List<string> SubscribedTopics { get; } = [];

        public AssuredMqttConnection CreateConnection()
        {
            return new AssuredMqttConnection(
                new Mock<ILogger<AssuredMqttConnection>>().Object,
                _mqttClientOptionsFactory.Object,
                _mqttFactory.Object,
                _mqttConfigurationOptions.Object);
        }

        public void CompleteConnect()
        {
            _connectCompletion.TrySetResult();
        }

        public async Task WaitForConnectedAsync()
        {
            await _connected.Task.WaitAsync(TimeSpan.FromSeconds(5));
        }

        public async Task<MqttApplicationMessage> WaitForPublishedMessageAsync()
        {
            return await _publishedMessage.Task.WaitAsync(TimeSpan.FromSeconds(5));
        }

        public async Task<string> WaitForSubscribedTopicAsync()
        {
            return await _subscribedTopic.Task.WaitAsync(TimeSpan.FromSeconds(5));
        }
    }
}
