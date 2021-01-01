using NetDaemon.Common;
using NetDaemon.Common.Fluent;

namespace Netdaemon.Generated.Extensions
{
    public static partial class EntityExtension
    {
        public static LightEntities LightEntities(this NetDaemonApp app) => new(app);
        public static MediaPlayerEntities MediaPlayerEntities(this NetDaemonApp app) => new(app);
    }

    public partial class LightEntities
    {
        private readonly NetDaemonApp _app;
        public LightEntities(NetDaemonApp app)
        {
            _app = app;
        }

        public IEntity KoketFonster => _app.Entity("light.koket_fonster");
    }

    public partial class MediaPlayerEntities
    {
        private readonly NetDaemonApp _app;
        public MediaPlayerEntities(NetDaemonApp app)
        {
            _app = app;
        }

        public IMediaPlayer MyPlayer => _app.MediaPlayer("media_player.my_player");
    }
}