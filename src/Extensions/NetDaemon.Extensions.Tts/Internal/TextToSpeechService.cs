using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;

namespace NetDaemon.Extensions.Tts.Internal;

[SuppressMessage("", "CA1812", Justification = "Instanced in DI")]
internal class TextToSpeechService : ITextToSpeechService, IAsyncDisposable
{
    private readonly IServiceProvider _provider;
    private readonly ILogger<ITextToSpeechService> _logger;
    private readonly Channel<TtsMessage> _ttsMessageChannel = Channel.CreateBounded<TtsMessage>(200);
    private readonly Task _processTtsTask;
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    public TextToSpeechService(IServiceProvider provider, ILogger<ITextToSpeechService> logger)
    {
        _provider = provider;
        _logger = logger;
        _processTtsTask = Task.Run(async () => await ProcessTtsMessages().ConfigureAwait(false));
    }

    public void Speak(string entityId, string message, string service, string? language, object? options)
    {
        _ttsMessageChannel.Writer.TryWrite(
            new TtsMessage
            {
                EntityId = entityId,
                Message = message,
                Service = service,
                Language = language,
                Options = options
            }
        );
    }

    [SuppressMessage("", "CA1031", Justification = "We need to log unexpected errors")]
    private async Task ProcessTtsMessages()
    {
        await foreach(var ttsMessage in  _ttsMessageChannel.Reader.ReadAllAsync(_cancellationTokenSource.Token).ConfigureAwait(false))
        {
            try
            {
                var homeAssistantConnection = _provider.GetRequiredService<IHomeAssistantConnection>();

                var hassTarget = new HassTarget {EntityIds = new[] {ttsMessage.EntityId}};
                var data = new
                {
                    message = ttsMessage.Message,
                    language = ttsMessage.Language,
                    options = ttsMessage.Options
                };
                await homeAssistantConnection
                    .CallServiceAsync("tts", ttsMessage.Service, data, hassTarget, _cancellationTokenSource.Token)
                    .ConfigureAwait(false);
                // Wait for media player to report state 
                await Task.Delay(InternalTimeForTtsDelay, _cancellationTokenSource.Token).ConfigureAwait(false);
                var state = await homeAssistantConnection
                    .GetEntityStateAsync(ttsMessage.EntityId, _cancellationTokenSource.Token).ConfigureAwait(false);

                if (state?.Attributes is null ||
                    !state.Attributes.TryGetValue("media_duration", out var durationAttribute)) continue;

                var durationElement = (JsonElement) durationAttribute;
                var duration = durationElement.GetDouble();
                // We wait the remaining duration plus 500 ms to make sure the speech is done
                var delayInMilliSeconds = (int) Math.Round(duration * 1000) -
                    InternalTimeForTtsDelay + 500;
                if (delayInMilliSeconds > 0)
                    await Task.Delay(delayInMilliSeconds, _cancellationTokenSource.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // We exit the loop
                return;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unexpected error in processing of text-to-speech messages");
            }
        }
    }

    private const int InternalTimeForTtsDelay = 2500;

    public async ValueTask DisposeAsync()
    {
        _cancellationTokenSource.Cancel();
        await _processTtsTask.ConfigureAwait(false);
        _cancellationTokenSource.Dispose();
    }
    
}
