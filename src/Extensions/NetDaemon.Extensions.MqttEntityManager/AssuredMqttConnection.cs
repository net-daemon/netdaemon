using System.Collections.Concurrent;
using System.Globalization;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Packets;
using NetDaemon.Extensions.MqttEntityManager.Exceptions;

namespace NetDaemon.Extensions.MqttEntityManager;

/// <summary>
/// Wrapper to assure an MQTT connection.
/// </summary>
internal class AssuredMqttConnection : IAssuredMqttConnection, IDisposable
{
    private static readonly TimeSpan DefaultReconnectDelay = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan DefaultConnectedPollDelay = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan PublishRetryDelay = TimeSpan.FromSeconds(1);
    private const int MaxConnectedPublishAttempts = 3;

    private readonly ILogger<AssuredMqttConnection> _logger;
    private readonly IMqttClient _mqttClient;
    private readonly MqttClientOptions _clientOptions;
    private readonly TimeProvider _timeProvider;
    private readonly Channel<MqttApplicationMessage> _publishQueue = Channel.CreateUnbounded<MqttApplicationMessage>(
        new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });
    private readonly ConcurrentDictionary<string, MqttTopicFilter> _subscriptions = new();
    private readonly ConcurrentDictionary<string, MqttTopicFilter> _pendingSubscriptions = new();
    private readonly CancellationTokenSource _stopping = new();
    private readonly SemaphoreSlim _clientOperationGate = new(1, 1);
    private readonly object _connectionStateLock = new();
    private readonly Task _connectionTask;
    private readonly Task _publishTask;
    private TaskCompletionSource _connectedSignal = CreateConnectionSignal();
    private bool _disposed;
    private volatile bool _hasConnected;

    /// <summary>
    /// Initializes a new instance of the <see cref="AssuredMqttConnection"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="mqttClientOptionsFactory">The MQTT client options factory.</param>
    /// <param name="mqttFactory">The MQTT factory wrapper.</param>
    /// <param name="mqttConfig">The MQTT configuration.</param>
    /// <param name="timeProvider">The clock used for connection retry delays.</param>
    public AssuredMqttConnection(
        ILogger<AssuredMqttConnection> logger,
        IMqttClientOptionsFactory mqttClientOptionsFactory,
        IMqttFactory mqttFactory,
        IOptions<MqttConfiguration> mqttConfig,
        TimeProvider? timeProvider = null)
    {
        _logger = logger;
        _timeProvider = timeProvider ?? TimeProvider.System;

        _logger.LogTrace("MQTT initiating connection");
        _clientOptions = mqttClientOptionsFactory.CreateClientOptions(mqttConfig.Value);
        _mqttClient = mqttFactory.CreateMqttClient();

        _mqttClient.ConnectedAsync += MqttClientOnConnectedAsync;
        _mqttClient.DisconnectedAsync += MqttClientOnDisconnectedAsync;
        _mqttClient.ApplicationMessageReceivedAsync += MqttClientOnApplicationMessageReceivedAsync;

        _connectionTask = Task.Run(() => MaintainConnectionAsync(_stopping.Token));
        _publishTask = Task.Run(() => PublishQueuedMessagesAsync(_stopping.Token));
    }

    /// <inheritdoc />
    public event Func<MqttApplicationMessageReceivedEventArgs, Task>? ApplicationMessageReceivedAsync;

    /// <inheritdoc />
    public async Task PublishAsync(MqttApplicationMessage message)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        await _publishQueue.Writer.WriteAsync(message, _stopping.Token).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task SubscribeAsync(MqttTopicFilter topicFilter)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (string.IsNullOrEmpty(topicFilter.Topic))
        {
            throw new ArgumentException("MQTT topic filter must specify a topic.", nameof(topicFilter));
        }

        _subscriptions[topicFilter.Topic] = topicFilter;
        _pendingSubscriptions[topicFilter.Topic] = topicFilter;

        if (_mqttClient.IsConnected)
        {
            await TrySubscribePendingAsync(topicFilter.Topic, topicFilter, _stopping.Token).ConfigureAwait(false);
        }
    }

    private async Task MaintainConnectionAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                if (_mqttClient.IsConnected)
                {
                    await SubscribePendingAsync(cancellationToken).ConfigureAwait(false);
                    await Task.Delay(DefaultConnectedPollDelay, _timeProvider, cancellationToken).ConfigureAwait(false);
                    continue;
                }

                MarkConnectionLost();

                _logger.LogTrace("Connecting to MQTT broker at {Host}:{Port}/{UserName}",
                    GetConfiguredHost(), GetConfiguredPort(), _clientOptions.Credentials?.GetUserName(_clientOptions));

                var connectResult = await ExecuteClientOperationAsync(
                    operationToken => _mqttClient.ConnectAsync(_clientOptions, operationToken),
                    cancellationToken).ConfigureAwait(false);

                if (connectResult.ResultCode != MqttClientConnectResultCode.Success)
                {
                    _logger.LogTrace("MQTT connection rejected: {ResultCode} {ReasonString}",
                        connectResult.ResultCode, connectResult.ReasonString);
                    await Task.Delay(DefaultReconnectDelay, _timeProvider, cancellationToken).ConfigureAwait(false);
                    continue;
                }

                _logger.LogTrace("MQTT client is ready");
                _hasConnected = true;
                MarkConnected();
                MarkAllSubscriptionsPending();
                await SubscribePendingAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogTrace(ex, "MQTT connection attempt failed");
                await DelayReconnectAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }

    private async Task PublishQueuedMessagesAsync(CancellationToken cancellationToken)
    {
        await foreach (var message in _publishQueue.Reader.ReadAllAsync(cancellationToken).ConfigureAwait(false))
        {
            var connectedFailureCount = 0;
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    if (!_mqttClient.IsConnected)
                    {
                        await WaitForConnectionAsync(cancellationToken).ConfigureAwait(false);
                        continue;
                    }

                    var published = await ExecuteClientOperationAsync(async operationToken =>
                    {
                        if (!_mqttClient.IsConnected)
                        {
                            return false;
                        }

                        var publishResult = await _mqttClient.PublishAsync(message, operationToken).ConfigureAwait(false);
                        if (!publishResult.IsSuccess)
                        {
                            throw new MqttPublishException(
                                $"MQTT broker rejected the message with {publishResult.ReasonCode}: {publishResult.ReasonString}");
                        }

                        return true;
                    }, cancellationToken).ConfigureAwait(false);

                    if (!published)
                    {
                        MarkConnectionLost();
                        continue;
                    }

                    break;
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    if (!_mqttClient.IsConnected)
                    {
                        connectedFailureCount = 0;
                        MarkConnectionLost();
                        _logger.LogDebug(ex, "Failed to publish MQTT message. The message will be retried after reconnect.");
                        await WaitForConnectionAsync(cancellationToken).ConfigureAwait(false);
                        continue;
                    }

                    connectedFailureCount++;
                    if (connectedFailureCount >= MaxConnectedPublishAttempts)
                    {
                        _logger.LogError(ex,
                            "Discarding MQTT message for topic {Topic} after {AttemptCount} failed publish attempts while connected.",
                            message.Topic, connectedFailureCount);
                        break;
                    }

                    _logger.LogDebug(ex,
                        "Failed to publish MQTT message while connected. Retrying attempt {NextAttempt} of {MaxAttempts} after a delay.",
                        connectedFailureCount + 1, MaxConnectedPublishAttempts);
                    await Task.Delay(PublishRetryDelay, _timeProvider, cancellationToken).ConfigureAwait(false);
                }
            }
        }
    }

    private async Task SubscribePendingAsync(CancellationToken cancellationToken)
    {
        foreach (var (topic, topicFilter) in _pendingSubscriptions)
        {
            if (!_mqttClient.IsConnected)
            {
                return;
            }

            await TrySubscribePendingAsync(topic, topicFilter, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task TrySubscribePendingAsync(
        string topic,
        MqttTopicFilter topicFilter,
        CancellationToken cancellationToken)
    {
        try
        {
            var subscribed = await ExecuteClientOperationAsync(async operationToken =>
            {
                if (!_mqttClient.IsConnected ||
                    !_pendingSubscriptions.TryGetValue(topic, out var currentTopicFilter) ||
                    !ReferenceEquals(currentTopicFilter, topicFilter))
                {
                    return false;
                }

                var subscribeOptions = new MqttClientSubscribeOptionsBuilder()
                    .WithTopicFilter(topicFilter)
                    .Build();

                var subscribeResult = await _mqttClient.SubscribeAsync(subscribeOptions, operationToken).ConfigureAwait(false);
                var accepted = subscribeResult.Items.Count > 0 && subscribeResult.Items.All(IsSuccessfulSubscribeResult);
                if (!accepted)
                {
                    _logger.LogDebug(
                        "MQTT broker rejected subscription to topic {Topic}: {ResultCodes} {ReasonString}",
                        topic,
                        string.Join(", ", subscribeResult.Items.Select(item => item.ResultCode)),
                        subscribeResult.ReasonString);
                }

                return accepted && _mqttClient.IsConnected;
            }, cancellationToken).ConfigureAwait(false);

            if (subscribed &&
                _pendingSubscriptions.TryGetValue(topic, out var currentTopicFilter) &&
                ReferenceEquals(currentTopicFilter, topicFilter))
            {
                _pendingSubscriptions.TryRemove(topic, out _);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to subscribe to MQTT topic {Topic}. The subscription will be retried.", topic);
        }
    }

    private async Task<T> ExecuteClientOperationAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken)
    {
        await _clientOperationGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            using var operationTimeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            operationTimeout.CancelAfter(_clientOptions.Timeout);
            return await operation(operationTimeout.Token).ConfigureAwait(false);
        }
        finally
        {
            _clientOperationGate.Release();
        }
    }

    private void MarkAllSubscriptionsPending()
    {
        foreach (var (topic, topicFilter) in _subscriptions)
        {
            _pendingSubscriptions[topic] = topicFilter;
        }
    }

    private static bool IsSuccessfulSubscribeResult(MqttClientSubscribeResultItem item)
    {
        return item.ResultCode is MqttClientSubscribeResultCode.GrantedQoS0
            or MqttClientSubscribeResultCode.GrantedQoS1
            or MqttClientSubscribeResultCode.GrantedQoS2;
    }

    private Task MqttClientOnDisconnectedAsync(MqttClientDisconnectedEventArgs arg)
    {
        if (_disposed || _stopping.IsCancellationRequested)
        {
            return Task.CompletedTask;
        }

        if (_hasConnected)
        {
            _logger.LogDebug("MQTT disconnected: {Reason}", BuildErrorResponse(arg));
        }
        else
        {
            _logger.LogTrace("MQTT disconnected before the initial connection completed: {Reason}", BuildErrorResponse(arg));
        }

        MarkConnectionLost();
        return Task.CompletedTask;
    }

    private Task MqttClientOnConnectedAsync(MqttClientConnectedEventArgs arg)
    {
        _hasConnected = true;
        _logger.LogDebug("MQTT connected: {ResultCode}", arg.ConnectResult.ResultCode);
        MarkConnected();
        return Task.CompletedTask;
    }

    private async Task MqttClientOnApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs arg)
    {
        var handlers = ApplicationMessageReceivedAsync;
        if (handlers is null)
        {
            return;
        }

        foreach (Func<MqttApplicationMessageReceivedEventArgs, Task> handler in handlers.GetInvocationList())
        {
            await handler(arg).ConfigureAwait(false);
        }
    }

    private static string BuildErrorResponse(MqttClientDisconnectedEventArgs arg)
    {
        var sb = new System.Text.StringBuilder();

        sb.AppendLine(CultureInfo.InvariantCulture, $"{arg.Exception?.Message} ({arg.Reason})");
        var ex = arg.Exception?.InnerException;
        while (ex != null)
        {
            sb.AppendLine(ex.Message);
            ex = ex.InnerException;
        }

        return sb.ToString();
    }

    private static TaskCompletionSource CreateConnectionSignal()
    {
        return new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    }

    private void MarkConnected()
    {
        lock (_connectionStateLock)
        {
            _connectedSignal.TrySetResult();
        }
    }

    private void MarkConnectionLost()
    {
        lock (_connectionStateLock)
        {
            if (_connectedSignal.Task.IsCompleted)
            {
                _connectedSignal = CreateConnectionSignal();
            }
        }

        MarkAllSubscriptionsPending();
    }

    private async Task WaitForConnectionAsync(CancellationToken cancellationToken)
    {
        while (!_mqttClient.IsConnected)
        {
            Task connectedTask;
            lock (_connectionStateLock)
            {
                if (_mqttClient.IsConnected)
                {
                    return;
                }

                connectedTask = _connectedSignal.Task;
            }

            await connectedTask.WaitAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task DelayReconnectAsync(CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(DefaultReconnectDelay, _timeProvider, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
    }

    private string? GetConfiguredHost()
    {
        return (_clientOptions.ChannelOptions as MqttClientTcpOptions)?.RemoteEndpoint switch
        {
            System.Net.DnsEndPoint dnsEndPoint => dnsEndPoint.Host,
            System.Net.IPEndPoint ipEndPoint => ipEndPoint.Address.ToString(),
            _ => null
        };
    }

    private int? GetConfiguredPort()
    {
        return (_clientOptions.ChannelOptions as MqttClientTcpOptions)?.RemoteEndpoint switch
        {
            System.Net.DnsEndPoint dnsEndPoint => dnsEndPoint.Port,
            System.Net.IPEndPoint ipEndPoint => ipEndPoint.Port,
            _ => null
        };
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _logger.LogTrace("MQTT disconnecting");
        _stopping.Cancel();
        _publishQueue.Writer.TryComplete();

        try
        {
            Task.WaitAll([_connectionTask, _publishTask], TimeSpan.FromSeconds(1));
        }
        catch (AggregateException)
        {
        }

        _stopping.Dispose();
        _clientOperationGate.Dispose();

        if (_mqttClient is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}
