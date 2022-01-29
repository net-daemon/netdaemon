namespace NetDaemon.Extensions.Tts.Internal;

internal record TtsMessage
{
    public string EntityId { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string Service { get; init; } = string.Empty;
    public object? Options { get; init; } = string.Empty;
    public string? Language { get; init; }
}
