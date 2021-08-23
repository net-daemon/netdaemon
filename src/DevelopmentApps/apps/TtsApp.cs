using NetDaemon.Common;
using NetDaemon.Common.ModelV3;
using NetDaemon.Daemon;

namespace NetDaemon.DevelopmentApps.apps
{
    [NetDaemonApp]
    [Focus]
    public class TtsApp
    {
        public TtsApp(ITextToSpeechService tts, IHaContext ha)
        {
            tts.Speak("media.MyMediaPlayer", "Welcome");
        }
    }
}