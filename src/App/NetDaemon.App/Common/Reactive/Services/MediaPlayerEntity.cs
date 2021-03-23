using System.Collections.Generic;

namespace NetDaemon.Common.Reactive.Services
{
    /// <inheritdoc />
    public class MediaPlayerEntity : RxEntityBase
    {
        /// <inheritdoc />
        public MediaPlayerEntity(INetDaemonRxApp daemon, IEnumerable<string> entityIds) : base(daemon, entityIds)
        {
        }

        public void VolumeUp(dynamic? data = null)
        {
            CallService("media_player", "volume_up", data, true);
        }

        public void VolumeDown(dynamic? data = null)
        {
            CallService("media_player", "volume_down", data, true);
        }

        public void MediaPlayPause(dynamic? data = null)
        {
            CallService("media_player", "media_play_pause", data, true);
        }

        public void MediaPlay(dynamic? data = null)
        {
            CallService("media_player", "media_play", data, true);
        }

        public void MediaPause(dynamic? data = null)
        {
            CallService("media_player", "media_pause", data, true);
        }

        public void MediaStop(dynamic? data = null)
        {
            CallService("media_player", "media_stop", data, true);
        }

        public void MediaNextTrack(dynamic? data = null)
        {
            CallService("media_player", "media_next_track", data, true);
        }

        public void MediaPreviousTrack(dynamic? data = null)
        {
            CallService("media_player", "media_previous_track", data, true);
        }

        public void ClearPlaylist(dynamic? data = null)
        {
            CallService("media_player", "clear_playlist", data, true);
        }

        public void VolumeSet(dynamic? data = null)
        {
            CallService("media_player", "volume_set", data, true);
        }

        public void VolumeMute(dynamic? data = null)
        {
            CallService("media_player", "volume_mute", data, true);
        }

        public void MediaSeek(dynamic? data = null)
        {
            CallService("media_player", "media_seek", data, true);
        }

        public void SelectSource(dynamic? data = null)
        {
            CallService("media_player", "select_source", data, true);
        }

        public void SelectSoundMode(dynamic? data = null)
        {
            CallService("media_player", "select_sound_mode", data, true);
        }

        public void PlayMedia(dynamic? data = null)
        {
            CallService("media_player", "play_media", data, true);
        }

        public void ShuffleSet(dynamic? data = null)
        {
            CallService("media_player", "shuffle_set", data, true);
        }

        public void RepeatSet(dynamic? data = null)
        {
            CallService("media_player", "repeat_set", data, true);
        }
    }
}