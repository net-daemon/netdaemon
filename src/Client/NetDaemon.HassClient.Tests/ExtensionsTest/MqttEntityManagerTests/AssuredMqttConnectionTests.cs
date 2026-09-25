using System.Collections.Concurrent;
using Microsoft.Extensions.Time.Testing;
using MQTTnet;
using MQTTnet.Packets;
using NetDaemon.Extensions.MqttEntityManager;

namespace NetDaemon.HassClient.Tests.ExtensionsTest.MqttEntityManagerTests;

public class AssuredMqttConnectionTests
{
    private static readonly TimeSpan DefaultWaitTimeout = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan RetryWaitTimeout = TimeSpan.FromSeconds(10);

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

    [Fact]
    public async Task RetriesAfterTransientConnectionFailure()
    {
        var setup = new AssuredMqttConnectionSetup();
        setup.FailConnectAttempts(1);
        using var conn = setup.CreateConnection();

        var topicFilter = new MqttTopicFilterBuilder()
            .WithTopic("homeassistant/domain/switch/set")
            .Build();

        var message = new MqttApplicationMessageBuilder()
            .WithTopic("homeassistant/domain/switch/state")
            .WithPayload("ON")
            .Build();

        await conn.SubscribeAsync(topicFilter);
        await conn.PublishAsync(message);
        setup.CompleteConnect();

        var subscribedTopic = await setup.WaitForSubscribedTopicAsync(RetryWaitTimeout);
        var publishedMessage = await setup.WaitForPublishedMessageAsync(RetryWaitTimeout);

        setup.ConnectAttempts.Should().BeGreaterThan(1);
        subscribedTopic.Should().Be("homeassistant/domain/switch/set");
        publishedMessage.Should().Be(message);
    }

    [Fact]
    public async Task FailedSubscriptionIsRetriedWhileConnectionRemainsOpen()
    {
        var setup = new AssuredMqttConnectionSetup(useFakeTime: true);
        setup.FailSubscribeAttempts("homeassistant/domain/sensor/set", 1);
        using var conn = setup.CreateConnection();

        await conn.SubscribeAsync(new MqttTopicFilterBuilder()
            .WithTopic("homeassistant/domain/sensor/set")
            .Build());
        setup.CompleteConnect();

        await setup.WaitForSubscribeAttemptsAsync("homeassistant/domain/sensor/set", 1);
        setup.Advance(TimeSpan.FromSeconds(5));

        await setup.WaitForSubscribeAttemptsAsync("homeassistant/domain/sensor/set", 2);
        setup.IsConnected.Should().BeTrue();
    }

    [Fact]
    public async Task RejectedSubscriptionResultIsRetriedWhileConnectionRemainsOpen()
    {
        var setup = new AssuredMqttConnectionSetup(useFakeTime: true);
        setup.RejectSubscribeAttempts("homeassistant/domain/sensor/set", 1);
        using var conn = setup.CreateConnection();

        await conn.SubscribeAsync(new MqttTopicFilterBuilder()
            .WithTopic("homeassistant/domain/sensor/set")
            .Build());
        setup.CompleteConnect();

        await setup.WaitForSubscribeAttemptsAsync("homeassistant/domain/sensor/set", 1);
        setup.Advance(TimeSpan.FromSeconds(1));

        await setup.WaitForSubscribeAttemptsAsync("homeassistant/domain/sensor/set", 2);
        setup.IsConnected.Should().BeTrue();
    }

    [Fact]
    public async Task SubscriptionIsRestoredAfterEstablishedConnectionIsLost()
    {
        var setup = new AssuredMqttConnectionSetup(useFakeTime: true);
        using var conn = setup.CreateConnection();
        setup.CompleteConnect();
        await setup.WaitForConnectedAsync();

        await conn.SubscribeAsync(new MqttTopicFilterBuilder()
            .WithTopic("homeassistant/domain/sensor/set")
            .Build());
        await setup.WaitForSubscribeAttemptsAsync("homeassistant/domain/sensor/set", 1);

        setup.Disconnect();
        setup.Advance(TimeSpan.FromSeconds(1));

        await setup.WaitForConnectAttemptsAsync(2);
        await setup.WaitForSubscribeAttemptsAsync("homeassistant/domain/sensor/set", 2);
        setup.IsConnected.Should().BeTrue();
    }

    [Fact]
    public async Task PermanentPublishFailureDoesNotBlockLaterMessages()
    {
        var setup = new AssuredMqttConnectionSetup(useFakeTime: true);
        setup.FailPublishAttempts("bad/topic", 3, blockAfterFailures: true);
        using var conn = setup.CreateConnection();
        setup.CompleteConnect();
        await setup.WaitForConnectedAsync();

        await conn.PublishAsync(new MqttApplicationMessageBuilder().WithTopic("bad/topic").Build());
        await conn.PublishAsync(new MqttApplicationMessageBuilder().WithTopic("good/topic").Build());

        await setup.WaitForPublishAttemptsAsync("bad/topic", 1);
        setup.Advance(TimeSpan.FromSeconds(1));
        await setup.WaitForPublishAttemptsAsync("bad/topic", 2);
        setup.Advance(TimeSpan.FromSeconds(1));

        var publishedMessage = await setup.WaitForPublishedMessageAsync();
        publishedMessage.Topic.Should().Be("good/topic");
        setup.GetPublishAttempts("bad/topic").Should().Be(3);
    }

    [Fact]
    public async Task RejectedPublishResultDoesNotBlockLaterMessages()
    {
        var setup = new AssuredMqttConnectionSetup(useFakeTime: true);
        setup.RejectPublishAttempts("bad/topic", 3);
        using var conn = setup.CreateConnection();
        setup.CompleteConnect();
        await setup.WaitForConnectedAsync();

        await conn.PublishAsync(new MqttApplicationMessageBuilder().WithTopic("bad/topic").Build());
        await conn.PublishAsync(new MqttApplicationMessageBuilder().WithTopic("good/topic").Build());

        await setup.WaitForPublishAttemptsAsync("bad/topic", 1);
        setup.Advance(TimeSpan.FromSeconds(1));
        await setup.WaitForPublishAttemptsAsync("bad/topic", 2);
        setup.Advance(TimeSpan.FromSeconds(1));

        var publishedMessage = await setup.WaitForPublishedMessageAsync();
        publishedMessage.Topic.Should().Be("good/topic");
        setup.GetPublishAttempts("bad/topic").Should().Be(3);
    }

    [Fact]
    public async Task ConnectAttemptUsesConfiguredOperationTimeout()
    {
        var setup = new AssuredMqttConnectionSetup(clientTimeout: TimeSpan.FromMilliseconds(50));
        setup.HangConnectUntilCancelled();
        using var conn = setup.CreateConnection();

        await setup.WaitForConnectCancellationAsync();

        setup.ConnectAttempts.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ReconnectDoesNotOverlapInFlightSubscription()
    {
        var setup = new AssuredMqttConnectionSetup(useFakeTime: true);
        using var conn = setup.CreateConnection();
        setup.CompleteConnect();
        await setup.WaitForConnectedAsync();
        setup.BlockSubscribe();

        var subscribeTask = conn.SubscribeAsync(new MqttTopicFilterBuilder()
            .WithTopic("homeassistant/domain/sensor/set")
            .Build());
        await setup.WaitForSubscribeStartedAsync();

        setup.Disconnect();
        setup.Advance(TimeSpan.FromSeconds(1));
        await setup.WaitForDisconnectObservedAsync();

        setup.ConnectAttempts.Should().Be(1);

        setup.ReleaseSubscribe();
        await subscribeTask;
        await setup.WaitForConnectAttemptsAsync(2);
    }

    private sealed class AssuredMqttConnectionSetup
    {
        private readonly TaskCompletionSource _connectCompletion = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource _connected = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly Channel<MqttApplicationMessage> _publishedMessages = Channel.CreateUnbounded<MqttApplicationMessage>();
        private readonly Channel<string> _subscribedTopics = Channel.CreateUnbounded<string>();
        private readonly Mock<IMqttClient> _mqttClient = new();
        private readonly Mock<IMqttFactory> _mqttFactory = new();
        private readonly Mock<IMqttClientOptionsFactory> _mqttClientOptionsFactory = new();
        private readonly Mock<IOptions<MqttConfiguration>> _mqttConfigurationOptions = new();
        private int _connectAttempts;
        private int _connectFailuresBeforeSuccess;
        private readonly ConcurrentDictionary<string, int> _publishAttempts = new();
        private readonly ConcurrentDictionary<string, int> _publishFailuresBeforeSuccess = new();
        private readonly ConcurrentDictionary<string, int> _publishRejectionsBeforeSuccess = new();
        private readonly ConcurrentDictionary<string, bool> _blockPublishAfterFailures = new();
        private readonly ConcurrentDictionary<string, int> _subscribeAttempts = new();
        private readonly ConcurrentDictionary<string, int> _subscribeFailuresBeforeSuccess = new();
        private readonly ConcurrentDictionary<string, int> _subscribeRejectionsBeforeSuccess = new();
        private readonly TaskCompletionSource _connectCancellation = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private TaskCompletionSource _disconnectObserved = CreateSignal();
        private TaskCompletionSource? _subscribeBlocker;
        private TaskCompletionSource? _subscribeStarted;
        private bool _hangConnectUntilCancelled;

        public AssuredMqttConnectionSetup(bool useFakeTime = false, TimeSpan? clientTimeout = null)
        {
            TimeProvider = useFakeTime ? new FakeTimeProvider() : TimeProvider.System;
            var mqttConfiguration = new MqttConfiguration
            {
                Host = "localhost",
                UserName = "id"
            };

            var clientOptions = new MqttClientOptionsBuilder()
                .WithTcpServer(mqttConfiguration.Host, mqttConfiguration.Port)
                .WithTimeout(clientTimeout ?? TimeSpan.FromSeconds(10))
                .Build();

            _mqttConfigurationOptions.SetupGet(o => o.Value)
                .Returns(mqttConfiguration);

            _mqttClientOptionsFactory.Setup(f => f.CreateClientOptions(mqttConfiguration))
                .Returns(clientOptions);

            _mqttFactory.Setup(f => f.CreateMqttClient())
                .Returns(_mqttClient.Object);

            _mqttClient.SetupGet(c => c.IsConnected)
                .Returns(() =>
                {
                    if (!IsConnected)
                    {
                        Volatile.Read(ref _disconnectObserved).TrySetResult();
                    }

                    return IsConnected;
                });

            _mqttClient.Setup(c => c.ConnectAsync(clientOptions, It.IsAny<CancellationToken>()))
                .Returns<MqttClientOptions, CancellationToken>(async (_, cancellationToken) =>
                {
                    Interlocked.Increment(ref _connectAttempts);
                    if (_hangConnectUntilCancelled)
                    {
                        try
                        {
                            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
                        }
                        catch (OperationCanceledException)
                        {
                            _connectCancellation.TrySetResult();
                            throw;
                        }
                    }

                    if (Interlocked.CompareExchange(ref _connectFailuresBeforeSuccess, 0, 0) > 0)
                    {
                        Interlocked.Decrement(ref _connectFailuresBeforeSuccess);
                        throw new InvalidOperationException("MQTT broker is not ready yet.");
                    }

                    await _connectCompletion.Task;
                    IsConnected = true;
                    _connected.TrySetResult();
                    return new MqttClientConnectResult
                    {
                        ResultCode = MqttClientConnectResultCode.Success
                    };
                });

            _mqttClient.Setup(c => c.PublishAsync(It.IsAny<MqttApplicationMessage>(), It.IsAny<CancellationToken>()))
                .Returns<MqttApplicationMessage, CancellationToken>(async (message, cancellationToken) =>
                {
                    var attempts = _publishAttempts.AddOrUpdate(message.Topic, 1, (_, count) => count + 1);
                    var failures = _publishFailuresBeforeSuccess.GetValueOrDefault(message.Topic);
                    if (attempts <= failures)
                    {
                        throw new InvalidOperationException("MQTT broker rejected the message.");
                    }

                    if (attempts <= _publishRejectionsBeforeSuccess.GetValueOrDefault(message.Topic))
                    {
                        return new MqttClientPublishResult(
                            null,
                            MqttClientPublishReasonCode.UnspecifiedError,
                            "Rejected by broker.",
                            []);
                    }

                    if (_blockPublishAfterFailures.GetValueOrDefault(message.Topic))
                    {
                        await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
                    }

                    PublishedMessages.Add(message);
                    await _publishedMessages.Writer.WriteAsync(message, cancellationToken);
                    return new MqttClientPublishResult(null, MqttClientPublishReasonCode.Success, string.Empty, []);
                });

            _mqttClient.Setup(c => c.SubscribeAsync(It.IsAny<MqttClientSubscribeOptions>(), It.IsAny<CancellationToken>()))
                .Returns<MqttClientSubscribeOptions, CancellationToken>(async (options, cancellationToken) =>
                {
                    foreach (var topic in options.TopicFilters.Select(topicFilter => topicFilter.Topic))
                    {
                        _subscribeStarted?.TrySetResult();
                        if (_subscribeBlocker is not null)
                        {
                            await _subscribeBlocker.Task.WaitAsync(cancellationToken);
                        }

                        var attempts = _subscribeAttempts.AddOrUpdate(topic, 1, (_, count) => count + 1);
                        if (attempts <= _subscribeFailuresBeforeSuccess.GetValueOrDefault(topic))
                        {
                            throw new InvalidOperationException("MQTT broker rejected the subscription.");
                        }

                        if (attempts <= _subscribeRejectionsBeforeSuccess.GetValueOrDefault(topic))
                        {
                            return new MqttClientSubscribeResult(
                                1,
                                [new MqttClientSubscribeResultItem(options.TopicFilters.Single(), MqttClientSubscribeResultCode.UnspecifiedError)],
                                "Rejected by broker.",
                                []);
                        }

                        SubscribedTopics.Add(topic);
                        await _subscribedTopics.Writer.WriteAsync(topic, cancellationToken);
                    }

                    return new MqttClientSubscribeResult(
                        1,
                        options.TopicFilters.Select(topicFilter =>
                            new MqttClientSubscribeResultItem(topicFilter, MqttClientSubscribeResultCode.GrantedQoS0)).ToArray(),
                        string.Empty,
                        []);
                });
        }

        public bool IsConnected { get; private set; }

        public int ConnectAttempts => _connectAttempts;

        public TimeProvider TimeProvider { get; }

        public void Advance(TimeSpan duration)
        {
            ((FakeTimeProvider)TimeProvider).Advance(duration);
        }

        public List<MqttApplicationMessage> PublishedMessages { get; } = [];

        public List<string> SubscribedTopics { get; } = [];

        public AssuredMqttConnection CreateConnection()
        {
            return new AssuredMqttConnection(
                new Mock<ILogger<AssuredMqttConnection>>().Object,
                _mqttClientOptionsFactory.Object,
                _mqttFactory.Object,
                _mqttConfigurationOptions.Object,
                TimeProvider);
        }

        public void CompleteConnect()
        {
            _connectCompletion.TrySetResult();
        }

        public void FailConnectAttempts(int count)
        {
            _connectFailuresBeforeSuccess = count;
        }

        public void HangConnectUntilCancelled()
        {
            _hangConnectUntilCancelled = true;
        }

        public void FailPublishAttempts(string topic, int count, bool blockAfterFailures = false)
        {
            _publishFailuresBeforeSuccess[topic] = count;
            _blockPublishAfterFailures[topic] = blockAfterFailures;
        }

        public void RejectPublishAttempts(string topic, int count)
        {
            _publishRejectionsBeforeSuccess[topic] = count;
        }

        public void FailSubscribeAttempts(string topic, int count)
        {
            _subscribeFailuresBeforeSuccess[topic] = count;
        }

        public void RejectSubscribeAttempts(string topic, int count)
        {
            _subscribeRejectionsBeforeSuccess[topic] = count;
        }

        public void BlockSubscribe()
        {
            _subscribeStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            _subscribeBlocker = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        }

        public void ReleaseSubscribe()
        {
            _subscribeBlocker?.TrySetResult();
        }

        public void Disconnect()
        {
            Volatile.Write(ref _disconnectObserved, CreateSignal());
            IsConnected = false;
        }

        public async Task WaitForConnectedAsync(TimeSpan? timeout = null)
        {
            await _connected.Task.WaitAsync(timeout ?? DefaultWaitTimeout);
        }

        public async Task<MqttApplicationMessage> WaitForPublishedMessageAsync(TimeSpan? timeout = null)
        {
            return await _publishedMessages.Reader.ReadAsync().AsTask().WaitAsync(timeout ?? DefaultWaitTimeout);
        }

        public async Task<string> WaitForSubscribedTopicAsync(TimeSpan? timeout = null)
        {
            return await _subscribedTopics.Reader.ReadAsync().AsTask().WaitAsync(timeout ?? DefaultWaitTimeout);
        }

        public int GetPublishAttempts(string topic)
        {
            return _publishAttempts.GetValueOrDefault(topic);
        }

        public async Task WaitForConnectCancellationAsync()
        {
            await _connectCancellation.Task.WaitAsync(DefaultWaitTimeout);
        }

        public async Task WaitForDisconnectObservedAsync()
        {
            await Volatile.Read(ref _disconnectObserved).Task.WaitAsync(DefaultWaitTimeout);
        }

        public async Task WaitForConnectAttemptsAsync(int count)
        {
            await WaitForConditionAsync(() => ConnectAttempts >= count);
        }

        public async Task WaitForPublishAttemptsAsync(string topic, int count)
        {
            await WaitForConditionAsync(() => _publishAttempts.GetValueOrDefault(topic) >= count);
        }

        public async Task WaitForSubscribeAttemptsAsync(string topic, int count)
        {
            await WaitForConditionAsync(() => _subscribeAttempts.GetValueOrDefault(topic) >= count);
        }

        public async Task WaitForSubscribeStartedAsync()
        {
            await (_subscribeStarted ?? throw new InvalidOperationException("Subscribe is not blocked."))
                .Task.WaitAsync(DefaultWaitTimeout);
        }

        private static async Task WaitForConditionAsync(Func<bool> condition)
        {
            using var timeout = new CancellationTokenSource(DefaultWaitTimeout);
            while (!condition())
            {
                await Task.Delay(TimeSpan.FromMilliseconds(10), timeout.Token);
            }
        }

        private static TaskCompletionSource CreateSignal()
        {
            return new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        }
    }
}
