using System.Net.WebSockets;
using System.Text;
using NetDaemon.Client.Internal.Net;

namespace NetDaemon.PerformanceBenchmarks.Support;

internal sealed class BenchmarkWebSocketClient : IWebSocketClient
{
    private readonly Queue<byte[]> _responses = new();
    private byte[]? _currentResponse;
    private int _currentResponseOffset;

    public WebSocketState State { get; private set; } = WebSocketState.Open;

    public WebSocketCloseStatus? CloseStatus { get; private set; }

    public ValueTask DisposeAsync()
    {
        State = WebSocketState.Closed;
        return ValueTask.CompletedTask;
    }

    public Task ConnectAsync(Uri uri, CancellationToken cancellationToken) => Task.CompletedTask;

    public Task CloseAsync(WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken)
    {
        CloseStatus = closeStatus;
        State = WebSocketState.Closed;
        return Task.CompletedTask;
    }

    public Task CloseOutputAsync(WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken)
    {
        CloseStatus = closeStatus;
        State = WebSocketState.Closed;
        return Task.CompletedTask;
    }

    public Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken)
    {
        SentBytes += buffer.Count;
        SentMessages++;
        return Task.CompletedTask;
    }

    public ValueTask SendAsync(ReadOnlyMemory<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken)
    {
        SentBytes += buffer.Length;
        SentMessages++;
        return ValueTask.CompletedTask;
    }

    public ValueTask<ValueWebSocketReceiveResult> ReceiveAsync(Memory<byte> buffer, CancellationToken cancellationToken)
    {
        _currentResponse ??= _responses.Dequeue();

        var remaining = _currentResponse.Length - _currentResponseOffset;
        var count = Math.Min(remaining, buffer.Length);
        _currentResponse.AsMemory(_currentResponseOffset, count).CopyTo(buffer);
        _currentResponseOffset += count;

        var endOfMessage = _currentResponseOffset == _currentResponse.Length;
        if (endOfMessage)
        {
            _currentResponse = null;
            _currentResponseOffset = 0;
        }

        return ValueTask.FromResult(new ValueWebSocketReceiveResult(count, WebSocketMessageType.Text, endOfMessage));
    }

    public int SentMessages { get; private set; }

    public int SentBytes { get; private set; }

    public void EnqueueJson(string json) => _responses.Enqueue(Encoding.UTF8.GetBytes(json));
}
