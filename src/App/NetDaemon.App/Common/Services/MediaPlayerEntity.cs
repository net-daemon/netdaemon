using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using NetDaemon.Common.Fluent;
using NetDaemon.Common.Reactive;

namespace NetDaemon.Common.Services
{
    public partial class MediaPlayerEntity : RxEntityBase
    {

        public MediaPlayerEntity(INetDaemonRxApp daemon, IEnumerable<string> entityIds): base(daemon, entityIds)
        {
        }

        public void VolumeUp(dynamic? data = null)
        {
            var serviceData = new FluentExpandoObject();
            if (data is ExpandoObject)
            {
                serviceData.CopyFrom(data);
            }
            else if (data is not null)
            {
                var expObject = ((object)data).ToExpandoObject();
                if (expObject is not null)
                    serviceData.CopyFrom(expObject);
            }

            serviceData["entity_id"] = EntityId;
            DaemonRxApp.CallService("media_player", "volume_up", serviceData);
        }

        public void VolumeDown(dynamic? data = null)
        {
            var serviceData = new FluentExpandoObject();
            if (data is ExpandoObject)
            {
                serviceData.CopyFrom(data);
            }
            else if (data is not null)
            {
                var expObject = ((object)data).ToExpandoObject();
                if (expObject is not null)
                    serviceData.CopyFrom(expObject);
            }

            serviceData["entity_id"] = EntityId;
            DaemonRxApp.CallService("media_player", "volume_down", serviceData);
        }

        public void MediaPlayPause(dynamic? data = null)
        {
            var serviceData = new FluentExpandoObject();
            if (data is ExpandoObject)
            {
                serviceData.CopyFrom(data);
            }
            else if (data is not null)
            {
                var expObject = ((object)data).ToExpandoObject();
                if (expObject is not null)
                    serviceData.CopyFrom(expObject);
            }

            serviceData["entity_id"] = EntityId;
            DaemonRxApp.CallService("media_player", "media_play_pause", serviceData);
        }

        public void MediaPlay(dynamic? data = null)
        {
            var serviceData = new FluentExpandoObject();
            if (data is ExpandoObject)
            {
                serviceData.CopyFrom(data);
            }
            else if (data is not null)
            {
                var expObject = ((object)data).ToExpandoObject();
                if (expObject is not null)
                    serviceData.CopyFrom(expObject);
            }

            serviceData["entity_id"] = EntityId;
            DaemonRxApp.CallService("media_player", "media_play", serviceData);
        }

        public void MediaPause(dynamic? data = null)
        {
            var serviceData = new FluentExpandoObject();
            if (data is ExpandoObject)
            {
                serviceData.CopyFrom(data);
            }
            else if (data is not null)
            {
                var expObject = ((object)data).ToExpandoObject();
                if (expObject is not null)
                    serviceData.CopyFrom(expObject);
            }

            serviceData["entity_id"] = EntityId;
            DaemonRxApp.CallService("media_player", "media_pause", serviceData);
        }

        public void MediaStop(dynamic? data = null)
        {
            var serviceData = new FluentExpandoObject();
            if (data is ExpandoObject)
            {
                serviceData.CopyFrom(data);
            }
            else if (data is not null)
            {
                var expObject = ((object)data).ToExpandoObject();
                if (expObject is not null)
                    serviceData.CopyFrom(expObject);
            }

            serviceData["entity_id"] = EntityId;
            DaemonRxApp.CallService("media_player", "media_stop", serviceData);
        }

        public void MediaNextTrack(dynamic? data = null)
        {
            var serviceData = new FluentExpandoObject();
            if (data is ExpandoObject)
            {
                serviceData.CopyFrom(data);
            }
            else if (data is not null)
            {
                var expObject = ((object)data).ToExpandoObject();
                if (expObject is not null)
                    serviceData.CopyFrom(expObject);
            }

            serviceData["entity_id"] = EntityId;
            DaemonRxApp.CallService("media_player", "media_next_track", serviceData);
        }

        public void MediaPreviousTrack(dynamic? data = null)
        {
            var serviceData = new FluentExpandoObject();
            if (data is ExpandoObject)
            {
                serviceData.CopyFrom(data);
            }
            else if (data is not null)
            {
                var expObject = ((object)data).ToExpandoObject();
                if (expObject is not null)
                    serviceData.CopyFrom(expObject);
            }

            serviceData["entity_id"] = EntityId;
            DaemonRxApp.CallService("media_player", "media_previous_track", serviceData);
        }

        public void ClearPlaylist(dynamic? data = null)
        {
            var serviceData = new FluentExpandoObject();
            if (data is ExpandoObject)
            {
                serviceData.CopyFrom(data);
            }
            else if (data is not null)
            {
                var expObject = ((object)data).ToExpandoObject();
                if (expObject is not null)
                    serviceData.CopyFrom(expObject);
            }

            serviceData["entity_id"] = EntityId;
            DaemonRxApp.CallService("media_player", "clear_playlist", serviceData);
        }

        public void VolumeSet(dynamic? data = null)
        {
            var serviceData = new FluentExpandoObject();
            if (data is ExpandoObject)
            {
                serviceData.CopyFrom(data);
            }
            else if (data is not null)
            {
                var expObject = ((object)data).ToExpandoObject();
                if (expObject is not null)
                    serviceData.CopyFrom(expObject);
            }

            serviceData["entity_id"] = EntityId;
            DaemonRxApp.CallService("media_player", "volume_set", serviceData);
        }

        public void VolumeMute(dynamic? data = null)
        {
            var serviceData = new FluentExpandoObject();
            if (data is ExpandoObject)
            {
                serviceData.CopyFrom(data);
            }
            else if (data is not null)
            {
                var expObject = ((object)data).ToExpandoObject();
                if (expObject is not null)
                    serviceData.CopyFrom(expObject);
            }

            serviceData["entity_id"] = EntityId;
            DaemonRxApp.CallService("media_player", "volume_mute", serviceData);
        }

        public void MediaSeek(dynamic? data = null)
        {
            var serviceData = new FluentExpandoObject();
            if (data is ExpandoObject)
            {
                serviceData.CopyFrom(data);
            }
            else if (data is not null)
            {
                var expObject = ((object)data).ToExpandoObject();
                if (expObject is not null)
                    serviceData.CopyFrom(expObject);
            }

            serviceData["entity_id"] = EntityId;
            DaemonRxApp.CallService("media_player", "media_seek", serviceData);
        }

        public void SelectSource(dynamic? data = null)
        {
            var serviceData = new FluentExpandoObject();
            if (data is ExpandoObject)
            {
                serviceData.CopyFrom(data);
            }
            else if (data is not null)
            {
                var expObject = ((object)data).ToExpandoObject();
                if (expObject is not null)
                    serviceData.CopyFrom(expObject);
            }

            serviceData["entity_id"] = EntityId;
            DaemonRxApp.CallService("media_player", "select_source", serviceData);
        }

        public void SelectSoundMode(dynamic? data = null)
        {
            var serviceData = new FluentExpandoObject();
            if (data is ExpandoObject)
            {
                serviceData.CopyFrom(data);
            }
            else if (data is not null)
            {
                var expObject = ((object)data).ToExpandoObject();
                if (expObject is not null)
                    serviceData.CopyFrom(expObject);
            }

            serviceData["entity_id"] = EntityId;
            DaemonRxApp.CallService("media_player", "select_sound_mode", serviceData);
        }

        public void PlayMedia(dynamic? data = null)
        {
            var serviceData = new FluentExpandoObject();
            if (data is ExpandoObject)
            {
                serviceData.CopyFrom(data);
            }
            else if (data is not null)
            {
                var expObject = ((object)data).ToExpandoObject();
                if (expObject is not null)
                    serviceData.CopyFrom(expObject);
            }

            serviceData["entity_id"] = EntityId;
            DaemonRxApp.CallService("media_player", "play_media", serviceData);
        }

        public void ShuffleSet(dynamic? data = null)
        {
            var serviceData = new FluentExpandoObject();
            if (data is ExpandoObject)
            {
                serviceData.CopyFrom(data);
            }
            else if (data is not null)
            {
                var expObject = ((object)data).ToExpandoObject();
                if (expObject is not null)
                    serviceData.CopyFrom(expObject);
            }

            serviceData["entity_id"] = EntityId;
            DaemonRxApp.CallService("media_player", "shuffle_set", serviceData);
        }

        public void RepeatSet(dynamic? data = null)
        {
            var serviceData = new FluentExpandoObject();
            if (data is ExpandoObject)
            {
                serviceData.CopyFrom(data);
            }
            else if (data is not null)
            {
                var expObject = ((object)data).ToExpandoObject();
                if (expObject is not null)
                    serviceData.CopyFrom(expObject);
            }

            serviceData["entity_id"] = EntityId;
            DaemonRxApp.CallService("media_player", "repeat_set", serviceData);
        }
    }
}