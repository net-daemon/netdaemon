using System;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NetDaemon.Common;

namespace NetDaemon.Daemon
{
    internal class TextToSpeechService : ITextToSpeechService
    {
        private readonly Channel<(string, string)> _ttsMessageChannel = Channel.CreateBounded<(string, string)>(200);

        // Used for testing
        internal int InternalDelayTimeForTts = 2500;

        private readonly NetDaemonHost _netDaemonHost;

        public TextToSpeechService(NetDaemonHost netDaemonHost)
        {
            _netDaemonHost = netDaemonHost;
        }

        public void Speak(string entityId, string message)
        {
            _ttsMessageChannel.Writer.TryWrite((entityId, message));
        }

        public Task StopAsync()
        {
            _ttsMessageChannel.Writer.TryComplete();
            return _ttsMessageChannel.Reader.Completion;
        }

        [SuppressMessage("", "CA1031")]
        public async Task ProcessAsync()
        {
            try
            {
                while (!_netDaemonHost.CancelToken.IsCancellationRequested)
                {
                    (string entityId, string message) =
                        await _ttsMessageChannel.Reader.ReadAsync(_netDaemonHost.CancelToken).ConfigureAwait(false);

                    dynamic attributes = new ExpandoObject();
                    attributes.entity_id = entityId;
                    attributes.message = message;
                    await _netDaemonHost.CallServiceAsync("tts", "google_cloud_say", (object)attributes, true)
                        .ConfigureAwait(false);
                    await Task.Delay(InternalDelayTimeForTts, _netDaemonHost.CancelToken)
                        .ConfigureAwait(false); // Wait 2 seconds to wait for status to complete

                    EntityState? currentPlayState = _netDaemonHost.GetState(entityId);

                    if (currentPlayState?.Attribute?.media_duration != null)
                    {
                        int delayInMilliSeconds = (int)Math.Round(currentPlayState?.Attribute?.media_duration * 1000) -
                                                  InternalDelayTimeForTts;

                        if (delayInMilliSeconds > 0)
                        {
                            await Task.Delay(delayInMilliSeconds, _netDaemonHost.CancelToken)
                                .ConfigureAwait(false); // Wait remainder of text message
                        }
                    }
                    // Maybe we should catch within the loop so we can continue processing??
                }
            }
            catch (OperationCanceledException)
            {
                // Do nothing it should be normal
            }
            catch (Exception e)
            {
                _netDaemonHost.Logger.LogError(e, "Error reading TTS channel");
            }
        }
    }
}