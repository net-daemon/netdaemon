using NetDaemon.Common;
using NetDaemon.Daemon;
using NetDaemon.HassModel.Common;

namespace NetDaemon.DevelopmentApps.apps
{
    [NetDaemonApp]
    public class TtsApp
    {
        public TtsApp(ITextToSpeechService tts, IHaContext ha)
        {
            tts.Speak("media.MyMediaPlayer", "Welcome");
        }
    }
}