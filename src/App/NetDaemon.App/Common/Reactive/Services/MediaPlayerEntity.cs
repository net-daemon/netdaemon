using System.Collections.Generic;

namespace NetDaemon.Common.Reactive.Services
{
    /// <inheritdoc />
    public partial class MediaPlayerEntity : RxEntityBase
    {
        /// <inheritdoc />
        public MediaPlayerEntity(INetDaemonRxApp daemon, IEnumerable<string> entityIds) : base(daemon, entityIds)
        {
        }

        /// <summary>
        ///     Increase volume
        /// </summary>
        /// <param name="data">Provided data</param>
        public void VolumeUp(dynamic? data = null)
        {
            CallService("media_player", "volume_up", data, true);
        }

        /// <summary>
        ///     Decrease volume
        /// </summary>
        /// <param name="data">Provided data</param>
        public void VolumeDown(dynamic? data = null)
        {
            CallService("media_player", "volume_down", data, true);
        }

        /// <summary>
        ///     play or pause media
        /// </summary>
        /// <param name="data">Provided data</param>
        public void MediaPlayPause(dynamic? data = null)
        {
            CallService("media_player", "media_play_pause", data, true);
        }

        /// <summary>
        ///     Play media
        /// </summary>
        /// <param name="data">Provided data</param>
        public void MediaPlay(dynamic? data = null)
        {
            CallService("media_player", "media_play", data, true);
        }

        /// <summary>
        ///     Pause media
        /// </summary>
        /// <param name="data">Provided data</param>
        public void MediaPause(dynamic? data = null)
        {
            CallService("media_player", "media_pause", data, true);
        }

        /// <summary>
        ///     Stop media
        /// </summary>
        /// <param name="data">Provided data</param>
        public void MediaStop(dynamic? data = null)
        {
            CallService("media_player", "media_stop", data, true);
        }

        /// <summary>
        ///     Play next track
        /// </summary>
        /// <param name="data">Provided data</param>
        public void MediaNextTrack(dynamic? data = null)
        {
            CallService("media_player", "media_next_track", data, true);
        }

        /// <summary>
        ///     Play previous track
        /// </summary>
        /// <param name="data">Provided data</param>
        public void MediaPreviousTrack(dynamic? data = null)
        {
            CallService("media_player", "media_previous_track", data, true);
        }

        /// <summary>
        ///     Clear playlist
        /// </summary>
        /// <param name="data">Provided data</param>
        public void ClearPlaylist(dynamic? data = null)
        {
            CallService("media_player", "clear_playlist", data, true);
        }

        /// <summary>
        ///     Set volume to value
        /// </summary>
        /// <param name="data">Provided data</param>
        public void VolumeSet(dynamic? data = null)
        {
            CallService("media_player", "volume_set", data, true);
        }

        /// <summary>
        ///     Mute volume
        /// </summary>
        /// <param name="data">Provided data</param>
        public void VolumeMute(dynamic? data = null)
        {
            CallService("media_player", "volume_mute", data, true);
        }

        /// <summary>
        ///     Seek media
        /// </summary>
        /// <param name="data">Provided data</param>
        public void MediaSeek(dynamic? data = null)
        {
            CallService("media_player", "media_seek", data, true);
        }

        /// <summary>
        ///     Select media source
        /// </summary>
        /// <param name="data">Provided data</param>
        public void SelectSource(dynamic? data = null)
        {
            CallService("media_player", "select_source", data, true);
        }

        /// <summary>
        ///     Select sound mode
        /// </summary>
        /// <param name="data">Provided data</param>
        public void SelectSoundMode(dynamic? data = null)
        {
            CallService("media_player", "select_sound_mode", data, true);
        }

        /// <summary>
        ///     Play media
        /// </summary>
        /// <param name="data">Provided data</param>
        public void PlayMedia(dynamic? data = null)
        {
            CallService("media_player", "play_media", data, true);
        }

        /// <summary>
        ///     Set shuffle
        /// </summary>
        /// <param name="data">Provided data</param>
        public void ShuffleSet(dynamic? data = null)
        {
            CallService("media_player", "shuffle_set", data, true);
        }

        /// <summary>
        ///     Set repeat
        /// </summary>
        /// <param name="data">Provided data</param>
        public void RepeatSet(dynamic? data = null)
        {
            CallService("media_player", "repeat_set", data, true);
        }
    }
}