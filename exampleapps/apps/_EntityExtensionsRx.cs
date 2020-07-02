using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using JoySoftware.HomeAssistant.NetDaemon.Common;
using JoySoftware.HomeAssistant.NetDaemon.Common.Reactive;

namespace Netdaemon.Generated.Reactive
{
    public class GeneratedAppBase : NetDaemonRxApp
    {
        public SensorEntities Sensor => new SensorEntities(this);
        public SwitchEntities Switch => new SwitchEntities(this);
        public DeviceTrackerEntities DeviceTracker => new DeviceTrackerEntities(this);
        public LightEntities Light => new LightEntities(this);
        public ProximityEntities Proximity => new ProximityEntities(this);
        public GroupEntities Group => new GroupEntities(this);
        public InputNumberEntities InputNumber => new InputNumberEntities(this);
        public ZoneEntities Zone => new ZoneEntities(this);
        public MediaPlayerEntities MediaPlayer => new MediaPlayerEntities(this);
        public ScriptEntity Script => new ScriptEntity(this, new string[]{""});
        public BinarySensorEntities BinarySensor => new BinarySensorEntities(this);
        public AutomationEntities Automation => new AutomationEntities(this);
        public NetdaemonEntities Netdaemon => new NetdaemonEntities(this);
        public SceneEntities Scene => new SceneEntities(this);
        public PersonEntities Person => new PersonEntities(this);
        public WeatherEntities Weather => new WeatherEntities(this);
        public SunEntities Sun => new SunEntities(this);
        public InputSelectEntities InputSelect => new InputSelectEntities(this);
        public RemoteEntities Remote => new RemoteEntities(this);
        public CameraEntities Camera => new CameraEntities(this);
        public CalendarEntities Calendar => new CalendarEntities(this);
        public InputBooleanEntities InputBoolean => new InputBooleanEntities(this);
        public CoverEntities Cover => new CoverEntities(this);
    }

    public partial class SensorEntity : RxEntity
    {
        public string EntityId => EntityIds.First();
        public string? Area => DaemonRxApp.State(EntityId)?.Area;
        public dynamic? Attribute => DaemonRxApp.State(EntityId)?.Attribute;
        public DateTime LastChanged => DaemonRxApp.State(EntityId).LastChanged;
        public DateTime LastUpdated => DaemonRxApp.State(EntityId).LastUpdated;
        public dynamic? State => DaemonRxApp.State(EntityId)?.State;
        public SensorEntity(INetDaemonReactive daemon, IEnumerable<string> entityIds): base(daemon, entityIds)
        {
        }
    }

    public partial class SwitchEntity : RxEntity
    {
        public string EntityId => EntityIds.First();
        public string? Area => DaemonRxApp.State(EntityId)?.Area;
        public dynamic? Attribute => DaemonRxApp.State(EntityId)?.Attribute;
        public DateTime LastChanged => DaemonRxApp.State(EntityId).LastChanged;
        public DateTime LastUpdated => DaemonRxApp.State(EntityId).LastUpdated;
        public dynamic? State => DaemonRxApp.State(EntityId)?.State;
        public SwitchEntity(INetDaemonReactive daemon, IEnumerable<string> entityIds): base(daemon, entityIds)
        {
        }
    }

    public partial class DeviceTrackerEntity : RxEntity
    {
        public string EntityId => EntityIds.First();
        public string? Area => DaemonRxApp.State(EntityId)?.Area;
        public dynamic? Attribute => DaemonRxApp.State(EntityId)?.Attribute;
        public DateTime LastChanged => DaemonRxApp.State(EntityId).LastChanged;
        public DateTime LastUpdated => DaemonRxApp.State(EntityId).LastUpdated;
        public dynamic? State => DaemonRxApp.State(EntityId)?.State;
        public DeviceTrackerEntity(INetDaemonReactive daemon, IEnumerable<string> entityIds): base(daemon, entityIds)
        {
        }

        public void See(dynamic? data = null)
        {
            var serviceData = new FluentExpandoObject();
            if (data is ExpandoObject)
            {
                serviceData.CopyFrom(data);
            }
            else if (data is object)
            {
                var expObject = ((object)data).ToExpandoObject();
                serviceData.CopyFrom(expObject);
            }

            DaemonRxApp.CallService("device_tracker", "see", serviceData);
        }
    }

    public partial class LightEntity : RxEntity
    {
        public string EntityId => EntityIds.First();
        public string? Area => DaemonRxApp.State(EntityId)?.Area;
        public dynamic? Attribute => DaemonRxApp.State(EntityId)?.Attribute;
        public DateTime LastChanged => DaemonRxApp.State(EntityId).LastChanged;
        public DateTime LastUpdated => DaemonRxApp.State(EntityId).LastUpdated;
        public dynamic? State => DaemonRxApp.State(EntityId)?.State;
        public LightEntity(INetDaemonReactive daemon, IEnumerable<string> entityIds): base(daemon, entityIds)
        {
        }
    }

    public partial class ProximityEntity : RxEntity
    {
        public string EntityId => EntityIds.First();
        public string? Area => DaemonRxApp.State(EntityId)?.Area;
        public dynamic? Attribute => DaemonRxApp.State(EntityId)?.Attribute;
        public DateTime LastChanged => DaemonRxApp.State(EntityId).LastChanged;
        public DateTime LastUpdated => DaemonRxApp.State(EntityId).LastUpdated;
        public dynamic? State => DaemonRxApp.State(EntityId)?.State;
        public ProximityEntity(INetDaemonReactive daemon, IEnumerable<string> entityIds): base(daemon, entityIds)
        {
        }
    }

    public partial class GroupEntity : RxEntity
    {
        public string EntityId => EntityIds.First();
        public string? Area => DaemonRxApp.State(EntityId)?.Area;
        public dynamic? Attribute => DaemonRxApp.State(EntityId)?.Attribute;
        public DateTime LastChanged => DaemonRxApp.State(EntityId).LastChanged;
        public DateTime LastUpdated => DaemonRxApp.State(EntityId).LastUpdated;
        public dynamic? State => DaemonRxApp.State(EntityId)?.State;
        public GroupEntity(INetDaemonReactive daemon, IEnumerable<string> entityIds): base(daemon, entityIds)
        {
        }

        public void Reload(dynamic? data = null)
        {
            var serviceData = new FluentExpandoObject();
            if (data is ExpandoObject)
            {
                serviceData.CopyFrom(data);
            }
            else if (data is object)
            {
                var expObject = ((object)data).ToExpandoObject();
                serviceData.CopyFrom(expObject);
            }

            DaemonRxApp.CallService("group", "reload", serviceData);
        }

        public void Set(dynamic? data = null)
        {
            var serviceData = new FluentExpandoObject();
            if (data is ExpandoObject)
            {
                serviceData.CopyFrom(data);
            }
            else if (data is object)
            {
                var expObject = ((object)data).ToExpandoObject();
                serviceData.CopyFrom(expObject);
            }

            DaemonRxApp.CallService("group", "set", serviceData);
        }

        public void Remove(dynamic? data = null)
        {
            var serviceData = new FluentExpandoObject();
            if (data is ExpandoObject)
            {
                serviceData.CopyFrom(data);
            }
            else if (data is object)
            {
                var expObject = ((object)data).ToExpandoObject();
                serviceData.CopyFrom(expObject);
            }

            DaemonRxApp.CallService("group", "remove", serviceData);
        }
    }

    public partial class InputNumberEntity : RxEntity
    {
        public string EntityId => EntityIds.First();
        public string? Area => DaemonRxApp.State(EntityId)?.Area;
        public dynamic? Attribute => DaemonRxApp.State(EntityId)?.Attribute;
        public DateTime LastChanged => DaemonRxApp.State(EntityId).LastChanged;
        public DateTime LastUpdated => DaemonRxApp.State(EntityId).LastUpdated;
        public dynamic? State => DaemonRxApp.State(EntityId)?.State;
        public InputNumberEntity(INetDaemonReactive daemon, IEnumerable<string> entityIds): base(daemon, entityIds)
        {
        }

        public void Reload(dynamic? data = null)
        {
            var serviceData = new FluentExpandoObject();
            if (data is ExpandoObject)
            {
                serviceData.CopyFrom(data);
            }
            else if (data is object)
            {
                var expObject = ((object)data).ToExpandoObject();
                serviceData.CopyFrom(expObject);
            }

            DaemonRxApp.CallService("input_number", "reload", serviceData);
        }

        public void SetValue(dynamic? data = null)
        {
            var serviceData = new FluentExpandoObject();
            if (data is ExpandoObject)
            {
                serviceData.CopyFrom(data);
            }
            else if (data is object)
            {
                var expObject = ((object)data).ToExpandoObject();
                serviceData.CopyFrom(expObject);
            }

            serviceData["entity_id"] = EntityId;
            DaemonRxApp.CallService("input_number", "set_value", serviceData);
        }

        public void Increment(dynamic? data = null)
        {
            var serviceData = new FluentExpandoObject();
            if (data is ExpandoObject)
            {
                serviceData.CopyFrom(data);
            }
            else if (data is object)
            {
                var expObject = ((object)data).ToExpandoObject();
                serviceData.CopyFrom(expObject);
            }

            serviceData["entity_id"] = EntityId;
            DaemonRxApp.CallService("input_number", "increment", serviceData);
        }

        public void Decrement(dynamic? data = null)
        {
            var serviceData = new FluentExpandoObject();
            if (data is ExpandoObject)
            {
                serviceData.CopyFrom(data);
            }
            else if (data is object)
            {
                var expObject = ((object)data).ToExpandoObject();
                serviceData.CopyFrom(expObject);
            }

            serviceData["entity_id"] = EntityId;
            DaemonRxApp.CallService("input_number", "decrement", serviceData);
        }
    }

    public partial class ZoneEntity : RxEntity
    {
        public string EntityId => EntityIds.First();
        public string? Area => DaemonRxApp.State(EntityId)?.Area;
        public dynamic? Attribute => DaemonRxApp.State(EntityId)?.Attribute;
        public DateTime LastChanged => DaemonRxApp.State(EntityId).LastChanged;
        public DateTime LastUpdated => DaemonRxApp.State(EntityId).LastUpdated;
        public dynamic? State => DaemonRxApp.State(EntityId)?.State;
        public ZoneEntity(INetDaemonReactive daemon, IEnumerable<string> entityIds): base(daemon, entityIds)
        {
        }

        public void Reload(dynamic? data = null)
        {
            var serviceData = new FluentExpandoObject();
            if (data is ExpandoObject)
            {
                serviceData.CopyFrom(data);
            }
            else if (data is object)
            {
                var expObject = ((object)data).ToExpandoObject();
                serviceData.CopyFrom(expObject);
            }

            DaemonRxApp.CallService("zone", "reload", serviceData);
        }
    }

    public partial class MediaPlayerEntity : RxEntity
    {
        public string EntityId => EntityIds.First();
        public string? Area => DaemonRxApp.State(EntityId)?.Area;
        public dynamic? Attribute => DaemonRxApp.State(EntityId)?.Attribute;
        public DateTime LastChanged => DaemonRxApp.State(EntityId).LastChanged;
        public DateTime LastUpdated => DaemonRxApp.State(EntityId).LastUpdated;
        public dynamic? State => DaemonRxApp.State(EntityId)?.State;
        public MediaPlayerEntity(INetDaemonReactive daemon, IEnumerable<string> entityIds): base(daemon, entityIds)
        {
        }

        public void VolumeUp(dynamic? data = null)
        {
            var serviceData = new FluentExpandoObject();
            if (data is ExpandoObject)
            {
                serviceData.CopyFrom(data);
            }
            else if (data is object)
            {
                var expObject = ((object)data).ToExpandoObject();
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
            else if (data is object)
            {
                var expObject = ((object)data).ToExpandoObject();
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
            else if (data is object)
            {
                var expObject = ((object)data).ToExpandoObject();
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
            else if (data is object)
            {
                var expObject = ((object)data).ToExpandoObject();
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
            else if (data is object)
            {
                var expObject = ((object)data).ToExpandoObject();
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
            else if (data is object)
            {
                var expObject = ((object)data).ToExpandoObject();
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
            else if (data is object)
            {
                var expObject = ((object)data).ToExpandoObject();
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
            else if (data is object)
            {
                var expObject = ((object)data).ToExpandoObject();
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
            else if (data is object)
            {
                var expObject = ((object)data).ToExpandoObject();
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
            else if (data is object)
            {
                var expObject = ((object)data).ToExpandoObject();
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
            else if (data is object)
            {
                var expObject = ((object)data).ToExpandoObject();
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
            else if (data is object)
            {
                var expObject = ((object)data).ToExpandoObject();
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
            else if (data is object)
            {
                var expObject = ((object)data).ToExpandoObject();
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
            else if (data is object)
            {
                var expObject = ((object)data).ToExpandoObject();
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
            else if (data is object)
            {
                var expObject = ((object)data).ToExpandoObject();
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
            else if (data is object)
            {
                var expObject = ((object)data).ToExpandoObject();
                serviceData.CopyFrom(expObject);
            }

            serviceData["entity_id"] = EntityId;
            DaemonRxApp.CallService("media_player", "shuffle_set", serviceData);
        }
    }

    public partial class ScriptEntity : RxEntity
    {
        public string EntityId => EntityIds.First();
        public string? Area => DaemonRxApp.State(EntityId)?.Area;
        public dynamic? Attribute => DaemonRxApp.State(EntityId)?.Attribute;
        public DateTime LastChanged => DaemonRxApp.State(EntityId).LastChanged;
        public DateTime LastUpdated => DaemonRxApp.State(EntityId).LastUpdated;
        public dynamic? State => DaemonRxApp.State(EntityId)?.State;
        public ScriptEntity(INetDaemonReactive daemon, IEnumerable<string> entityIds): base(daemon, entityIds)
        {
        }

        public void MorningScene(dynamic? data = null)
        {
            var serviceData = new FluentExpandoObject();
            if (data is ExpandoObject)
            {
                serviceData.CopyFrom(data);
            }
            else if (data is object)
            {
                var expObject = ((object)data).ToExpandoObject();
                serviceData.CopyFrom(expObject);
            }

            DaemonRxApp.CallService("script", "morning_scene", serviceData);
        }

        public void DayScene(dynamic? data = null)
        {
            var serviceData = new FluentExpandoObject();
            if (data is ExpandoObject)
            {
                serviceData.CopyFrom(data);
            }
            else if (data is object)
            {
                var expObject = ((object)data).ToExpandoObject();
                serviceData.CopyFrom(expObject);
            }

            DaemonRxApp.CallService("script", "day_scene", serviceData);
        }

        public void EveningScene(dynamic? data = null)
        {
            var serviceData = new FluentExpandoObject();
            if (data is ExpandoObject)
            {
                serviceData.CopyFrom(data);
            }
            else if (data is object)
            {
                var expObject = ((object)data).ToExpandoObject();
                serviceData.CopyFrom(expObject);
            }

            DaemonRxApp.CallService("script", "evening_scene", serviceData);
        }

        public void ColorScene(dynamic? data = null)
        {
            var serviceData = new FluentExpandoObject();
            if (data is ExpandoObject)
            {
                serviceData.CopyFrom(data);
            }
            else if (data is object)
            {
                var expObject = ((object)data).ToExpandoObject();
                serviceData.CopyFrom(expObject);
            }

            DaemonRxApp.CallService("script", "color_scene", serviceData);
        }

        public void CleaningScene(dynamic? data = null)
        {
            var serviceData = new FluentExpandoObject();
            if (data is ExpandoObject)
            {
                serviceData.CopyFrom(data);
            }
            else if (data is object)
            {
                var expObject = ((object)data).ToExpandoObject();
                serviceData.CopyFrom(expObject);
            }

            DaemonRxApp.CallService("script", "cleaning_scene", serviceData);
        }

        public void NightScene(dynamic? data = null)
        {
            var serviceData = new FluentExpandoObject();
            if (data is ExpandoObject)
            {
                serviceData.CopyFrom(data);
            }
            else if (data is object)
            {
                var expObject = ((object)data).ToExpandoObject();
                serviceData.CopyFrom(expObject);
            }

            DaemonRxApp.CallService("script", "night_scene", serviceData);
        }

        public void TvScene(dynamic? data = null)
        {
            var serviceData = new FluentExpandoObject();
            if (data is ExpandoObject)
            {
                serviceData.CopyFrom(data);
            }
            else if (data is object)
            {
                var expObject = ((object)data).ToExpandoObject();
                serviceData.CopyFrom(expObject);
            }

            DaemonRxApp.CallService("script", "tv_scene", serviceData);
        }

        public void FilmScene(dynamic? data = null)
        {
            var serviceData = new FluentExpandoObject();
            if (data is ExpandoObject)
            {
                serviceData.CopyFrom(data);
            }
            else if (data is object)
            {
                var expObject = ((object)data).ToExpandoObject();
                serviceData.CopyFrom(expObject);
            }

            DaemonRxApp.CallService("script", "film_scene", serviceData);
        }

        public void TvOffScene(dynamic? data = null)
        {
            var serviceData = new FluentExpandoObject();
            if (data is ExpandoObject)
            {
                serviceData.CopyFrom(data);
            }
            else if (data is object)
            {
                var expObject = ((object)data).ToExpandoObject();
                serviceData.CopyFrom(expObject);
            }

            DaemonRxApp.CallService("script", "tv_off_scene", serviceData);
        }

        public void S1586350051032(dynamic? data = null)
        {
            var serviceData = new FluentExpandoObject();
            if (data is ExpandoObject)
            {
                serviceData.CopyFrom(data);
            }
            else if (data is object)
            {
                var expObject = ((object)data).ToExpandoObject();
                serviceData.CopyFrom(expObject);
            }

            DaemonRxApp.CallService("script", "1586350051032", serviceData);
        }

        public void Notify(dynamic? data = null)
        {
            var serviceData = new FluentExpandoObject();
            if (data is ExpandoObject)
            {
                serviceData.CopyFrom(data);
            }
            else if (data is object)
            {
                var expObject = ((object)data).ToExpandoObject();
                serviceData.CopyFrom(expObject);
            }

            DaemonRxApp.CallService("script", "notify", serviceData);
        }

        public void NotifyGreet(dynamic? data = null)
        {
            var serviceData = new FluentExpandoObject();
            if (data is ExpandoObject)
            {
                serviceData.CopyFrom(data);
            }
            else if (data is object)
            {
                var expObject = ((object)data).ToExpandoObject();
                serviceData.CopyFrom(expObject);
            }

            DaemonRxApp.CallService("script", "notify_greet", serviceData);
        }

        public void Setnightmode(dynamic? data = null)
        {
            var serviceData = new FluentExpandoObject();
            if (data is ExpandoObject)
            {
                serviceData.CopyFrom(data);
            }
            else if (data is object)
            {
                var expObject = ((object)data).ToExpandoObject();
                serviceData.CopyFrom(expObject);
            }

            DaemonRxApp.CallService("script", "setnightmode", serviceData);
        }

        public void Reload(dynamic? data = null)
        {
            var serviceData = new FluentExpandoObject();
            if (data is ExpandoObject)
            {
                serviceData.CopyFrom(data);
            }
            else if (data is object)
            {
                var expObject = ((object)data).ToExpandoObject();
                serviceData.CopyFrom(expObject);
            }

            DaemonRxApp.CallService("script", "reload", serviceData);
        }
    }

    public partial class BinarySensorEntity : RxEntity
    {
        public string EntityId => EntityIds.First();
        public string? Area => DaemonRxApp.State(EntityId)?.Area;
        public dynamic? Attribute => DaemonRxApp.State(EntityId)?.Attribute;
        public DateTime LastChanged => DaemonRxApp.State(EntityId).LastChanged;
        public DateTime LastUpdated => DaemonRxApp.State(EntityId).LastUpdated;
        public dynamic? State => DaemonRxApp.State(EntityId)?.State;
        public BinarySensorEntity(INetDaemonReactive daemon, IEnumerable<string> entityIds): base(daemon, entityIds)
        {
        }
    }

    public partial class AutomationEntity : RxEntity
    {
        public string EntityId => EntityIds.First();
        public string? Area => DaemonRxApp.State(EntityId)?.Area;
        public dynamic? Attribute => DaemonRxApp.State(EntityId)?.Attribute;
        public DateTime LastChanged => DaemonRxApp.State(EntityId).LastChanged;
        public DateTime LastUpdated => DaemonRxApp.State(EntityId).LastUpdated;
        public dynamic? State => DaemonRxApp.State(EntityId)?.State;
        public AutomationEntity(INetDaemonReactive daemon, IEnumerable<string> entityIds): base(daemon, entityIds)
        {
        }

        public void Trigger(dynamic? data = null)
        {
            var serviceData = new FluentExpandoObject();
            if (data is ExpandoObject)
            {
                serviceData.CopyFrom(data);
            }
            else if (data is object)
            {
                var expObject = ((object)data).ToExpandoObject();
                serviceData.CopyFrom(expObject);
            }

            serviceData["entity_id"] = EntityId;
            DaemonRxApp.CallService("automation", "trigger", serviceData);
        }

        public void Reload(dynamic? data = null)
        {
            var serviceData = new FluentExpandoObject();
            if (data is ExpandoObject)
            {
                serviceData.CopyFrom(data);
            }
            else if (data is object)
            {
                var expObject = ((object)data).ToExpandoObject();
                serviceData.CopyFrom(expObject);
            }

            DaemonRxApp.CallService("automation", "reload", serviceData);
        }
    }

    public partial class NetdaemonEntity : RxEntity
    {
        public string EntityId => EntityIds.First();
        public string? Area => DaemonRxApp.State(EntityId)?.Area;
        public dynamic? Attribute => DaemonRxApp.State(EntityId)?.Attribute;
        public DateTime LastChanged => DaemonRxApp.State(EntityId).LastChanged;
        public DateTime LastUpdated => DaemonRxApp.State(EntityId).LastUpdated;
        public dynamic? State => DaemonRxApp.State(EntityId)?.State;
        public NetdaemonEntity(INetDaemonReactive daemon, IEnumerable<string> entityIds): base(daemon, entityIds)
        {
        }

        public void RegisterService(dynamic? data = null)
        {
            var serviceData = new FluentExpandoObject();
            if (data is ExpandoObject)
            {
                serviceData.CopyFrom(data);
            }
            else if (data is object)
            {
                var expObject = ((object)data).ToExpandoObject();
                serviceData.CopyFrom(expObject);
            }

            DaemonRxApp.CallService("netdaemon", "register_service", serviceData);
        }

        public void ReloadApps(dynamic? data = null)
        {
            var serviceData = new FluentExpandoObject();
            if (data is ExpandoObject)
            {
                serviceData.CopyFrom(data);
            }
            else if (data is object)
            {
                var expObject = ((object)data).ToExpandoObject();
                serviceData.CopyFrom(expObject);
            }

            DaemonRxApp.CallService("netdaemon", "reload_apps", serviceData);
        }
    }

    public partial class SceneEntity : RxEntity
    {
        public string EntityId => EntityIds.First();
        public string? Area => DaemonRxApp.State(EntityId)?.Area;
        public dynamic? Attribute => DaemonRxApp.State(EntityId)?.Attribute;
        public DateTime LastChanged => DaemonRxApp.State(EntityId).LastChanged;
        public DateTime LastUpdated => DaemonRxApp.State(EntityId).LastUpdated;
        public dynamic? State => DaemonRxApp.State(EntityId)?.State;
        public SceneEntity(INetDaemonReactive daemon, IEnumerable<string> entityIds): base(daemon, entityIds)
        {
        }

        public void Reload(dynamic? data = null)
        {
            var serviceData = new FluentExpandoObject();
            if (data is ExpandoObject)
            {
                serviceData.CopyFrom(data);
            }
            else if (data is object)
            {
                var expObject = ((object)data).ToExpandoObject();
                serviceData.CopyFrom(expObject);
            }

            DaemonRxApp.CallService("scene", "reload", serviceData);
        }

        public void Apply(dynamic? data = null)
        {
            var serviceData = new FluentExpandoObject();
            if (data is ExpandoObject)
            {
                serviceData.CopyFrom(data);
            }
            else if (data is object)
            {
                var expObject = ((object)data).ToExpandoObject();
                serviceData.CopyFrom(expObject);
            }

            DaemonRxApp.CallService("scene", "apply", serviceData);
        }

        public void Create(dynamic? data = null)
        {
            var serviceData = new FluentExpandoObject();
            if (data is ExpandoObject)
            {
                serviceData.CopyFrom(data);
            }
            else if (data is object)
            {
                var expObject = ((object)data).ToExpandoObject();
                serviceData.CopyFrom(expObject);
            }

            DaemonRxApp.CallService("scene", "create", serviceData);
        }
    }

    public partial class PersonEntity : RxEntity
    {
        public string EntityId => EntityIds.First();
        public string? Area => DaemonRxApp.State(EntityId)?.Area;
        public dynamic? Attribute => DaemonRxApp.State(EntityId)?.Attribute;
        public DateTime LastChanged => DaemonRxApp.State(EntityId).LastChanged;
        public DateTime LastUpdated => DaemonRxApp.State(EntityId).LastUpdated;
        public dynamic? State => DaemonRxApp.State(EntityId)?.State;
        public PersonEntity(INetDaemonReactive daemon, IEnumerable<string> entityIds): base(daemon, entityIds)
        {
        }

        public void Reload(dynamic? data = null)
        {
            var serviceData = new FluentExpandoObject();
            if (data is ExpandoObject)
            {
                serviceData.CopyFrom(data);
            }
            else if (data is object)
            {
                var expObject = ((object)data).ToExpandoObject();
                serviceData.CopyFrom(expObject);
            }

            DaemonRxApp.CallService("person", "reload", serviceData);
        }
    }

    public partial class WeatherEntity : RxEntity
    {
        public string EntityId => EntityIds.First();
        public string? Area => DaemonRxApp.State(EntityId)?.Area;
        public dynamic? Attribute => DaemonRxApp.State(EntityId)?.Attribute;
        public DateTime LastChanged => DaemonRxApp.State(EntityId).LastChanged;
        public DateTime LastUpdated => DaemonRxApp.State(EntityId).LastUpdated;
        public dynamic? State => DaemonRxApp.State(EntityId)?.State;
        public WeatherEntity(INetDaemonReactive daemon, IEnumerable<string> entityIds): base(daemon, entityIds)
        {
        }
    }

    public partial class SunEntity : RxEntity
    {
        public string EntityId => EntityIds.First();
        public string? Area => DaemonRxApp.State(EntityId)?.Area;
        public dynamic? Attribute => DaemonRxApp.State(EntityId)?.Attribute;
        public DateTime LastChanged => DaemonRxApp.State(EntityId).LastChanged;
        public DateTime LastUpdated => DaemonRxApp.State(EntityId).LastUpdated;
        public dynamic? State => DaemonRxApp.State(EntityId)?.State;
        public SunEntity(INetDaemonReactive daemon, IEnumerable<string> entityIds): base(daemon, entityIds)
        {
        }
    }

    public partial class InputSelectEntity : RxEntity
    {
        public string EntityId => EntityIds.First();
        public string? Area => DaemonRxApp.State(EntityId)?.Area;
        public dynamic? Attribute => DaemonRxApp.State(EntityId)?.Attribute;
        public DateTime LastChanged => DaemonRxApp.State(EntityId).LastChanged;
        public DateTime LastUpdated => DaemonRxApp.State(EntityId).LastUpdated;
        public dynamic? State => DaemonRxApp.State(EntityId)?.State;
        public InputSelectEntity(INetDaemonReactive daemon, IEnumerable<string> entityIds): base(daemon, entityIds)
        {
        }

        public void Reload(dynamic? data = null)
        {
            var serviceData = new FluentExpandoObject();
            if (data is ExpandoObject)
            {
                serviceData.CopyFrom(data);
            }
            else if (data is object)
            {
                var expObject = ((object)data).ToExpandoObject();
                serviceData.CopyFrom(expObject);
            }

            DaemonRxApp.CallService("input_select", "reload", serviceData);
        }

        public void SelectOption(dynamic? data = null)
        {
            var serviceData = new FluentExpandoObject();
            if (data is ExpandoObject)
            {
                serviceData.CopyFrom(data);
            }
            else if (data is object)
            {
                var expObject = ((object)data).ToExpandoObject();
                serviceData.CopyFrom(expObject);
            }

            serviceData["entity_id"] = EntityId;
            DaemonRxApp.CallService("input_select", "select_option", serviceData);
        }

        public void SelectNext(dynamic? data = null)
        {
            var serviceData = new FluentExpandoObject();
            if (data is ExpandoObject)
            {
                serviceData.CopyFrom(data);
            }
            else if (data is object)
            {
                var expObject = ((object)data).ToExpandoObject();
                serviceData.CopyFrom(expObject);
            }

            serviceData["entity_id"] = EntityId;
            DaemonRxApp.CallService("input_select", "select_next", serviceData);
        }

        public void SelectPrevious(dynamic? data = null)
        {
            var serviceData = new FluentExpandoObject();
            if (data is ExpandoObject)
            {
                serviceData.CopyFrom(data);
            }
            else if (data is object)
            {
                var expObject = ((object)data).ToExpandoObject();
                serviceData.CopyFrom(expObject);
            }

            serviceData["entity_id"] = EntityId;
            DaemonRxApp.CallService("input_select", "select_previous", serviceData);
        }

        public void SetOptions(dynamic? data = null)
        {
            var serviceData = new FluentExpandoObject();
            if (data is ExpandoObject)
            {
                serviceData.CopyFrom(data);
            }
            else if (data is object)
            {
                var expObject = ((object)data).ToExpandoObject();
                serviceData.CopyFrom(expObject);
            }

            serviceData["entity_id"] = EntityId;
            DaemonRxApp.CallService("input_select", "set_options", serviceData);
        }
    }

    public partial class RemoteEntity : RxEntity
    {
        public string EntityId => EntityIds.First();
        public string? Area => DaemonRxApp.State(EntityId)?.Area;
        public dynamic? Attribute => DaemonRxApp.State(EntityId)?.Attribute;
        public DateTime LastChanged => DaemonRxApp.State(EntityId).LastChanged;
        public DateTime LastUpdated => DaemonRxApp.State(EntityId).LastUpdated;
        public dynamic? State => DaemonRxApp.State(EntityId)?.State;
        public RemoteEntity(INetDaemonReactive daemon, IEnumerable<string> entityIds): base(daemon, entityIds)
        {
        }

        public void SendCommand(dynamic? data = null)
        {
            var serviceData = new FluentExpandoObject();
            if (data is ExpandoObject)
            {
                serviceData.CopyFrom(data);
            }
            else if (data is object)
            {
                var expObject = ((object)data).ToExpandoObject();
                serviceData.CopyFrom(expObject);
            }

            serviceData["entity_id"] = EntityId;
            DaemonRxApp.CallService("remote", "send_command", serviceData);
        }

        public void LearnCommand(dynamic? data = null)
        {
            var serviceData = new FluentExpandoObject();
            if (data is ExpandoObject)
            {
                serviceData.CopyFrom(data);
            }
            else if (data is object)
            {
                var expObject = ((object)data).ToExpandoObject();
                serviceData.CopyFrom(expObject);
            }

            serviceData["entity_id"] = EntityId;
            DaemonRxApp.CallService("remote", "learn_command", serviceData);
        }
    }

    public partial class CameraEntity : RxEntity
    {
        public string EntityId => EntityIds.First();
        public string? Area => DaemonRxApp.State(EntityId)?.Area;
        public dynamic? Attribute => DaemonRxApp.State(EntityId)?.Attribute;
        public DateTime LastChanged => DaemonRxApp.State(EntityId).LastChanged;
        public DateTime LastUpdated => DaemonRxApp.State(EntityId).LastUpdated;
        public dynamic? State => DaemonRxApp.State(EntityId)?.State;
        public CameraEntity(INetDaemonReactive daemon, IEnumerable<string> entityIds): base(daemon, entityIds)
        {
        }

        public void EnableMotionDetection(dynamic? data = null)
        {
            var serviceData = new FluentExpandoObject();
            if (data is ExpandoObject)
            {
                serviceData.CopyFrom(data);
            }
            else if (data is object)
            {
                var expObject = ((object)data).ToExpandoObject();
                serviceData.CopyFrom(expObject);
            }

            serviceData["entity_id"] = EntityId;
            DaemonRxApp.CallService("camera", "enable_motion_detection", serviceData);
        }

        public void DisableMotionDetection(dynamic? data = null)
        {
            var serviceData = new FluentExpandoObject();
            if (data is ExpandoObject)
            {
                serviceData.CopyFrom(data);
            }
            else if (data is object)
            {
                var expObject = ((object)data).ToExpandoObject();
                serviceData.CopyFrom(expObject);
            }

            serviceData["entity_id"] = EntityId;
            DaemonRxApp.CallService("camera", "disable_motion_detection", serviceData);
        }

        public void Snapshot(dynamic? data = null)
        {
            var serviceData = new FluentExpandoObject();
            if (data is ExpandoObject)
            {
                serviceData.CopyFrom(data);
            }
            else if (data is object)
            {
                var expObject = ((object)data).ToExpandoObject();
                serviceData.CopyFrom(expObject);
            }

            serviceData["entity_id"] = EntityId;
            DaemonRxApp.CallService("camera", "snapshot", serviceData);
        }

        public void PlayStream(dynamic? data = null)
        {
            var serviceData = new FluentExpandoObject();
            if (data is ExpandoObject)
            {
                serviceData.CopyFrom(data);
            }
            else if (data is object)
            {
                var expObject = ((object)data).ToExpandoObject();
                serviceData.CopyFrom(expObject);
            }

            serviceData["entity_id"] = EntityId;
            DaemonRxApp.CallService("camera", "play_stream", serviceData);
        }

        public void Record(dynamic? data = null)
        {
            var serviceData = new FluentExpandoObject();
            if (data is ExpandoObject)
            {
                serviceData.CopyFrom(data);
            }
            else if (data is object)
            {
                var expObject = ((object)data).ToExpandoObject();
                serviceData.CopyFrom(expObject);
            }

            serviceData["entity_id"] = EntityId;
            DaemonRxApp.CallService("camera", "record", serviceData);
        }
    }

    public partial class CalendarEntity : RxEntity
    {
        public string EntityId => EntityIds.First();
        public string? Area => DaemonRxApp.State(EntityId)?.Area;
        public dynamic? Attribute => DaemonRxApp.State(EntityId)?.Attribute;
        public DateTime LastChanged => DaemonRxApp.State(EntityId).LastChanged;
        public DateTime LastUpdated => DaemonRxApp.State(EntityId).LastUpdated;
        public dynamic? State => DaemonRxApp.State(EntityId)?.State;
        public CalendarEntity(INetDaemonReactive daemon, IEnumerable<string> entityIds): base(daemon, entityIds)
        {
        }
    }

    public partial class InputBooleanEntity : RxEntity
    {
        public string EntityId => EntityIds.First();
        public string? Area => DaemonRxApp.State(EntityId)?.Area;
        public dynamic? Attribute => DaemonRxApp.State(EntityId)?.Attribute;
        public DateTime LastChanged => DaemonRxApp.State(EntityId).LastChanged;
        public DateTime LastUpdated => DaemonRxApp.State(EntityId).LastUpdated;
        public dynamic? State => DaemonRxApp.State(EntityId)?.State;
        public InputBooleanEntity(INetDaemonReactive daemon, IEnumerable<string> entityIds): base(daemon, entityIds)
        {
        }

        public void Reload(dynamic? data = null)
        {
            var serviceData = new FluentExpandoObject();
            if (data is ExpandoObject)
            {
                serviceData.CopyFrom(data);
            }
            else if (data is object)
            {
                var expObject = ((object)data).ToExpandoObject();
                serviceData.CopyFrom(expObject);
            }

            DaemonRxApp.CallService("input_boolean", "reload", serviceData);
        }
    }

    public partial class CoverEntity : RxEntity
    {
        public string EntityId => EntityIds.First();
        public string? Area => DaemonRxApp.State(EntityId)?.Area;
        public dynamic? Attribute => DaemonRxApp.State(EntityId)?.Attribute;
        public DateTime LastChanged => DaemonRxApp.State(EntityId).LastChanged;
        public DateTime LastUpdated => DaemonRxApp.State(EntityId).LastUpdated;
        public dynamic? State => DaemonRxApp.State(EntityId)?.State;
        public CoverEntity(INetDaemonReactive daemon, IEnumerable<string> entityIds): base(daemon, entityIds)
        {
        }

        public void OpenCover(dynamic? data = null)
        {
            var serviceData = new FluentExpandoObject();
            if (data is ExpandoObject)
            {
                serviceData.CopyFrom(data);
            }
            else if (data is object)
            {
                var expObject = ((object)data).ToExpandoObject();
                serviceData.CopyFrom(expObject);
            }

            serviceData["entity_id"] = EntityId;
            DaemonRxApp.CallService("cover", "open_cover", serviceData);
        }

        public void CloseCover(dynamic? data = null)
        {
            var serviceData = new FluentExpandoObject();
            if (data is ExpandoObject)
            {
                serviceData.CopyFrom(data);
            }
            else if (data is object)
            {
                var expObject = ((object)data).ToExpandoObject();
                serviceData.CopyFrom(expObject);
            }

            serviceData["entity_id"] = EntityId;
            DaemonRxApp.CallService("cover", "close_cover", serviceData);
        }

        public void SetCoverPosition(dynamic? data = null)
        {
            var serviceData = new FluentExpandoObject();
            if (data is ExpandoObject)
            {
                serviceData.CopyFrom(data);
            }
            else if (data is object)
            {
                var expObject = ((object)data).ToExpandoObject();
                serviceData.CopyFrom(expObject);
            }

            serviceData["entity_id"] = EntityId;
            DaemonRxApp.CallService("cover", "set_cover_position", serviceData);
        }

        public void StopCover(dynamic? data = null)
        {
            var serviceData = new FluentExpandoObject();
            if (data is ExpandoObject)
            {
                serviceData.CopyFrom(data);
            }
            else if (data is object)
            {
                var expObject = ((object)data).ToExpandoObject();
                serviceData.CopyFrom(expObject);
            }

            serviceData["entity_id"] = EntityId;
            DaemonRxApp.CallService("cover", "stop_cover", serviceData);
        }

        public void OpenCoverTilt(dynamic? data = null)
        {
            var serviceData = new FluentExpandoObject();
            if (data is ExpandoObject)
            {
                serviceData.CopyFrom(data);
            }
            else if (data is object)
            {
                var expObject = ((object)data).ToExpandoObject();
                serviceData.CopyFrom(expObject);
            }

            serviceData["entity_id"] = EntityId;
            DaemonRxApp.CallService("cover", "open_cover_tilt", serviceData);
        }

        public void CloseCoverTilt(dynamic? data = null)
        {
            var serviceData = new FluentExpandoObject();
            if (data is ExpandoObject)
            {
                serviceData.CopyFrom(data);
            }
            else if (data is object)
            {
                var expObject = ((object)data).ToExpandoObject();
                serviceData.CopyFrom(expObject);
            }

            serviceData["entity_id"] = EntityId;
            DaemonRxApp.CallService("cover", "close_cover_tilt", serviceData);
        }

        public void StopCoverTilt(dynamic? data = null)
        {
            var serviceData = new FluentExpandoObject();
            if (data is ExpandoObject)
            {
                serviceData.CopyFrom(data);
            }
            else if (data is object)
            {
                var expObject = ((object)data).ToExpandoObject();
                serviceData.CopyFrom(expObject);
            }

            serviceData["entity_id"] = EntityId;
            DaemonRxApp.CallService("cover", "stop_cover_tilt", serviceData);
        }

        public void SetCoverTiltPosition(dynamic? data = null)
        {
            var serviceData = new FluentExpandoObject();
            if (data is ExpandoObject)
            {
                serviceData.CopyFrom(data);
            }
            else if (data is object)
            {
                var expObject = ((object)data).ToExpandoObject();
                serviceData.CopyFrom(expObject);
            }

            serviceData["entity_id"] = EntityId;
            DaemonRxApp.CallService("cover", "set_cover_tilt_position", serviceData);
        }

        public void ToggleCoverTilt(dynamic? data = null)
        {
            var serviceData = new FluentExpandoObject();
            if (data is ExpandoObject)
            {
                serviceData.CopyFrom(data);
            }
            else if (data is object)
            {
                var expObject = ((object)data).ToExpandoObject();
                serviceData.CopyFrom(expObject);
            }

            serviceData["entity_id"] = EntityId;
            DaemonRxApp.CallService("cover", "toggle_cover_tilt", serviceData);
        }
    }

    public partial class SensorEntities
    {
        private readonly NetDaemonRxApp _app;
        public SensorEntities(NetDaemonRxApp app)
        {
            _app = app;
        }

        public SensorEntity MelkersRumTempBatteryLevel => new SensorEntity(_app, new string[]{"sensor.melkers_rum_temp_battery_level"});
        public SensorEntity KokFrysTempBatteryLevel => new SensorEntity(_app, new string[]{"sensor.kok_frys_temp_battery_level"});
        public SensorEntity VardagsrumPirBatteryLevel => new SensorEntity(_app, new string[]{"sensor.vardagsrum_pir_battery_level"});
        public SensorEntity VardagsrumHum => new SensorEntity(_app, new string[]{"sensor.vardagsrum_hum"});
        public SensorEntity UteTempBatteryLevel => new SensorEntity(_app, new string[]{"sensor.ute_temp_battery_level"});
        public SensorEntity SovrumPirBatteryLevel => new SensorEntity(_app, new string[]{"sensor.sovrum_pir_battery_level"});
        public SensorEntity TrappPirBatteryLevel => new SensorEntity(_app, new string[]{"sensor.trapp_pir_battery_level"});
        public SensorEntity Consumption41 => new SensorEntity(_app, new string[]{"sensor.consumption_41"});
        public SensorEntity SmG975fWifiConnection => new SensorEntity(_app, new string[]{"sensor.sm_g975f_wifi_connection"});
        public SensorEntity TempOutside => new SensorEntity(_app, new string[]{"sensor.temp_outside"});
        public SensorEntity Load5m => new SensorEntity(_app, new string[]{"sensor.load_5m"});
        public SensorEntity KokFrysTemp => new SensorEntity(_app, new string[]{"sensor.kok_frys_temp"});
        public SensorEntity MelkersRumTemp2 => new SensorEntity(_app, new string[]{"sensor.melkers_rum_temp_2"});
        public SensorEntity TrappPir => new SensorEntity(_app, new string[]{"sensor.trapp_pir"});
        public SensorEntity SovrumTemp => new SensorEntity(_app, new string[]{"sensor.sovrum_temp"});
        public SensorEntity YrWindSpeed => new SensorEntity(_app, new string[]{"sensor.yr_wind_speed"});
        public SensorEntity DiodTemp => new SensorEntity(_app, new string[]{"sensor.diod_temp"});
        public SensorEntity VardagsrumTemp => new SensorEntity(_app, new string[]{"sensor.vardagsrum_temp"});
        public SensorEntity DiskUsePercentHome => new SensorEntity(_app, new string[]{"sensor.disk_use_percent_home"});
        public SensorEntity TvrumPir => new SensorEntity(_app, new string[]{"sensor.tvrum_pir"});
        public SensorEntity CarDepartureTime => new SensorEntity(_app, new string[]{"sensor.car_departure_time"});
        public SensorEntity SallysRumTemp => new SensorEntity(_app, new string[]{"sensor.sallys_rum_temp"});
        public SensorEntity NetworkOutWlp2s0 => new SensorEntity(_app, new string[]{"sensor.network_out_wlp2s0"});
        public SensorEntity FrysuppeTemperature => new SensorEntity(_app, new string[]{"sensor.frysuppe_temperature"});
        public SensorEntity HobbyrumTemp => new SensorEntity(_app, new string[]{"sensor.hobbyrum_temp"});
        public SensorEntity UtetempTemperature => new SensorEntity(_app, new string[]{"sensor.utetemp_temperature"});
        public SensorEntity TradfriOpenCloseSwitchBatteryLevel => new SensorEntity(_app, new string[]{"sensor.tradfri_open_close_switch_battery_level"});
        public SensorEntity SmG975fBatteryLevel => new SensorEntity(_app, new string[]{"sensor.sm_g975f_battery_level"});
        public SensorEntity YtQg8r => new SensorEntity(_app, new string[]{"sensor.yt_qg8r"});
        public SensorEntity SovrumHum => new SensorEntity(_app, new string[]{"sensor.sovrum_hum"});
        public SensorEntity HallDorrBatteryLevel => new SensorEntity(_app, new string[]{"sensor.hall_dorr_battery_level"});
        public SensorEntity Ssid => new SensorEntity(_app, new string[]{"sensor.ssid"});
        public SensorEntity UteTemp => new SensorEntity(_app, new string[]{"sensor.ute_temp"});
        public SensorEntity GeocodedLocation => new SensorEntity(_app, new string[]{"sensor.geocoded_location"});
        public SensorEntity LastBoot => new SensorEntity(_app, new string[]{"sensor.last_boot"});
        public SensorEntity KokFrysTemp2 => new SensorEntity(_app, new string[]{"sensor.kok_frys_temp_2"});
        public SensorEntity Power42 => new SensorEntity(_app, new string[]{"sensor.power_42"});
        public SensorEntity MyfitnesspalTomas => new SensorEntity(_app, new string[]{"sensor.myfitnesspal_tomas"});
        public SensorEntity UtetempHumidity => new SensorEntity(_app, new string[]{"sensor.utetemp_humidity"});
        public SensorEntity MemoryUsePercent => new SensorEntity(_app, new string[]{"sensor.memory_use_percent"});
        public SensorEntity Bssid => new SensorEntity(_app, new string[]{"sensor.bssid"});
        public SensorEntity FyrturBlockOutRollerBlindBatteryLevel2 => new SensorEntity(_app, new string[]{"sensor.fyrtur_block_out_roller_blind_battery_level_2"});
        public SensorEntity Power40 => new SensorEntity(_app, new string[]{"sensor.power_40"});
        public SensorEntity SallysRumPirBatteryLevel => new SensorEntity(_app, new string[]{"sensor.sallys_rum_pir_battery_level"});
        public SensorEntity FrysnereTemperature => new SensorEntity(_app, new string[]{"sensor.frysnere_temperature"});
        public SensorEntity E085007078700008949Temperature => new SensorEntity(_app, new string[]{"sensor.085007078700008949_temperature"});
        public SensorEntity VardagsrumPir => new SensorEntity(_app, new string[]{"sensor.vardagsrum_pir"});
        public SensorEntity SmG975fGeocodedLocation => new SensorEntity(_app, new string[]{"sensor.sm_g975f_geocoded_location"});
        public SensorEntity BatteryState => new SensorEntity(_app, new string[]{"sensor.battery_state"});
        public SensorEntity Consumption39 => new SensorEntity(_app, new string[]{"sensor.consumption_39"});
        public SensorEntity Load1m => new SensorEntity(_app, new string[]{"sensor.load_1m"});
        public SensorEntity YrSymbol => new SensorEntity(_app, new string[]{"sensor.yr_symbol"});
        public SensorEntity TvrumPirBatteryLevel => new SensorEntity(_app, new string[]{"sensor.tvrum_pir_battery_level"});
        public SensorEntity YtPewdiepie => new SensorEntity(_app, new string[]{"sensor.yt_pewdiepie"});
        public SensorEntity HumOutside => new SensorEntity(_app, new string[]{"sensor.hum_outside"});
        public SensorEntity FyrturBlockOutRollerBlindBatteryLevel => new SensorEntity(_app, new string[]{"sensor.fyrtur_block_out_roller_blind_battery_level"});
        public SensorEntity VardagsrumTempBatteryLevel => new SensorEntity(_app, new string[]{"sensor.vardagsrum_temp_battery_level"});
        public SensorEntity TradfriOpenCloseSwitch2BatteryLevel => new SensorEntity(_app, new string[]{"sensor.tradfri_open_close_switch_2_battery_level"});
        public SensorEntity ProcessorUse => new SensorEntity(_app, new string[]{"sensor.processor_use"});
        public SensorEntity MelkersRumTemp => new SensorEntity(_app, new string[]{"sensor.melkers_rum_temp"});
        public SensorEntity SallysRumTemp2 => new SensorEntity(_app, new string[]{"sensor.sallys_rum_temp_2"});
        public SensorEntity TvrumCubeBatteryLevel => new SensorEntity(_app, new string[]{"sensor.tvrum_cube_battery_level"});
        public SensorEntity YtHelto => new SensorEntity(_app, new string[]{"sensor.yt_helto"});
        public SensorEntity SnapshotBackup => new SensorEntity(_app, new string[]{"sensor.snapshot_backup"});
        public SensorEntity BatteryLevel => new SensorEntity(_app, new string[]{"sensor.battery_level"});
        public SensorEntity SallysRumTempBatteryLevel => new SensorEntity(_app, new string[]{"sensor.sallys_rum_temp_battery_level"});
        public SensorEntity PlexPlex => new SensorEntity(_app, new string[]{"sensor.plex_plex"});
        public SensorEntity YrTemperature => new SensorEntity(_app, new string[]{"sensor.yr_temperature"});
        public SensorEntity YrCloudiness => new SensorEntity(_app, new string[]{"sensor.yr_cloudiness"});
        public SensorEntity HobbyrumTempBatteryLevel => new SensorEntity(_app, new string[]{"sensor.hobbyrum_temp_battery_level"});
        public SensorEntity HobbyrumTemp2 => new SensorEntity(_app, new string[]{"sensor.hobbyrum_temp_2"});
        public SensorEntity KrisinformationVasternorrland => new SensorEntity(_app, new string[]{"sensor.krisinformation_vasternorrland"});
        public SensorEntity TomasRumPirBatteryLevel => new SensorEntity(_app, new string[]{"sensor.tomas_rum_pir_battery_level"});
        public SensorEntity Hacs => new SensorEntity(_app, new string[]{"sensor.hacs"});
        public SensorEntity SmG975fBatteryState => new SensorEntity(_app, new string[]{"sensor.sm_g975f_battery_state"});
        public SensorEntity Load15m => new SensorEntity(_app, new string[]{"sensor.load_15m"});
        public SensorEntity SovrumTempBatteryLevel => new SensorEntity(_app, new string[]{"sensor.sovrum_temp_battery_level"});
        public SensorEntity YtMe4le => new SensorEntity(_app, new string[]{"sensor.yt_me4le"});
        public SensorEntity ConnectionType => new SensorEntity(_app, new string[]{"sensor.connection_type"});
        public SensorEntity MelkersRumPirBatteryLevel => new SensorEntity(_app, new string[]{"sensor.melkers_rum_pir_battery_level"});
        public SensorEntity Activity => new SensorEntity(_app, new string[]{"sensor.activity"});
        public SensorEntity UteHum => new SensorEntity(_app, new string[]{"sensor.ute_hum"});
        public SensorEntity LastUpdateTrigger => new SensorEntity(_app, new string[]{"sensor.last_update_trigger"});
        public SensorEntity Power3 => new SensorEntity(_app, new string[]{"sensor.power_3"});
        public SensorEntity KokPirBatteryLevel => new SensorEntity(_app, new string[]{"sensor.kok_pir_battery_level"});
        public SensorEntity VardagsrumPir2 => new SensorEntity(_app, new string[]{"sensor.vardagsrum_pir_2"});
        public SensorEntity HouseMode => new SensorEntity(_app, new string[]{"sensor.house_mode"});
        public SensorEntity Consumption2 => new SensorEntity(_app, new string[]{"sensor.consumption_2"});
        public SensorEntity KokPir => new SensorEntity(_app, new string[]{"sensor.kok_pir"});
    }

    public partial class SwitchEntities
    {
        private readonly NetDaemonRxApp _app;
        public SwitchEntities(NetDaemonRxApp app)
        {
            _app = app;
        }

        public SwitchEntity NetdaemonLightManager => new SwitchEntity(_app, new string[]{"switch.netdaemon_light_manager"});
        public SwitchEntity Tv => new SwitchEntity(_app, new string[]{"switch.tv"});
        public SwitchEntity Testswitch => new SwitchEntity(_app, new string[]{"switch.testswitch"});
        public SwitchEntity Sonoff1Relay => new SwitchEntity(_app, new string[]{"switch.sonoff1_relay"});
        public SwitchEntity JulbelysningTomasRum => new SwitchEntity(_app, new string[]{"switch.julbelysning_tomas_rum"});
        public SwitchEntity Switch15 => new SwitchEntity(_app, new string[]{"switch.switch15"});
        public SwitchEntity NetdaemonCalendarTrash => new SwitchEntity(_app, new string[]{"switch.netdaemon_calendar_trash"});
        public SwitchEntity MelkersRumFonster => new SwitchEntity(_app, new string[]{"switch.melkers_rum_fonster"});
        public SwitchEntity Motorvarmare => new SwitchEntity(_app, new string[]{"switch.motorvarmare"});
        public SwitchEntity Switch3 => new SwitchEntity(_app, new string[]{"switch.switch3"});
        public SwitchEntity Switch10 => new SwitchEntity(_app, new string[]{"switch.switch10"});
        public SwitchEntity Switch66 => new SwitchEntity(_app, new string[]{"switch.switch66"});
        public SwitchEntity ShellyRelay => new SwitchEntity(_app, new string[]{"switch.shelly_relay"});
        public SwitchEntity NetdaemonMotion => new SwitchEntity(_app, new string[]{"switch.netdaemon_motion"});
        public SwitchEntity NetdaemonFrys => new SwitchEntity(_app, new string[]{"switch.netdaemon_frys"});
        public SwitchEntity Remote1B2 => new SwitchEntity(_app, new string[]{"switch.remote_1_b2"});
        public SwitchEntity Switch11 => new SwitchEntity(_app, new string[]{"switch.switch11"});
        public SwitchEntity NetdaemonMotorvarmare => new SwitchEntity(_app, new string[]{"switch.netdaemon_motorvarmare"});
        public SwitchEntity Switch1Lb => new SwitchEntity(_app, new string[]{"switch.switch_1_lb"});
        public SwitchEntity NetdaemonHacsNotifyOnUpdate => new SwitchEntity(_app, new string[]{"switch.netdaemon_hacs_notify_on_update"});
        public SwitchEntity Switch8MelkersTv => new SwitchEntity(_app, new string[]{"switch.switch8_melkers_tv"});
        public SwitchEntity JulbelysningVardagsrumH => new SwitchEntity(_app, new string[]{"switch.julbelysning_vardagsrum_h"});
        public SwitchEntity NetdaemonTv => new SwitchEntity(_app, new string[]{"switch.netdaemon_tv"});
        public SwitchEntity Switch5MelkersFan => new SwitchEntity(_app, new string[]{"switch.switch5_melkers_fan"});
        public SwitchEntity NetdaemonHouseStateManager => new SwitchEntity(_app, new string[]{"switch.netdaemon_house_state_manager"});
        public SwitchEntity Switch7 => new SwitchEntity(_app, new string[]{"switch.switch7"});
        public SwitchEntity Remote1B3 => new SwitchEntity(_app, new string[]{"switch.remote_1_b3"});
        public SwitchEntity JulbelysningVardagsrumM => new SwitchEntity(_app, new string[]{"switch.julbelysning_vardagsrum_m"});
        public SwitchEntity SallysRumFonster => new SwitchEntity(_app, new string[]{"switch.sallys_rum_fonster"});
        public SwitchEntity TvrumVagg => new SwitchEntity(_app, new string[]{"switch.tvrum_vagg"});
        public SwitchEntity NetdaemonRoomSpecific => new SwitchEntity(_app, new string[]{"switch.netdaemon_room_specific"});
        public SwitchEntity KokKaffebryggare => new SwitchEntity(_app, new string[]{"switch.kok_kaffebryggare"});
        public SwitchEntity ADiod => new SwitchEntity(_app, new string[]{"switch.a_diod"});
        public SwitchEntity JulbelysningSovrum => new SwitchEntity(_app, new string[]{"switch.julbelysning_sovrum"});
        public SwitchEntity Switch4TomasFan => new SwitchEntity(_app, new string[]{"switch.switch4_tomas_fan"});
        public SwitchEntity NetdaemonWelcomeHome => new SwitchEntity(_app, new string[]{"switch.netdaemon_welcome_home"});
        public SwitchEntity Switch1 => new SwitchEntity(_app, new string[]{"switch.switch1"});
        public SwitchEntity JulbelysningKokV => new SwitchEntity(_app, new string[]{"switch.julbelysning_kok_v"});
        public SwitchEntity Switch13 => new SwitchEntity(_app, new string[]{"switch.switch13"});
        public SwitchEntity Film => new SwitchEntity(_app, new string[]{"switch.film"});
        public SwitchEntity Switch1Rb => new SwitchEntity(_app, new string[]{"switch.switch_1_rb"});
        public SwitchEntity Switch14 => new SwitchEntity(_app, new string[]{"switch.switch14"});
        public SwitchEntity Switch2 => new SwitchEntity(_app, new string[]{"switch.switch2"});
        public SwitchEntity Switch9outdoor => new SwitchEntity(_app, new string[]{"switch.switch9outdoor"});
        public SwitchEntity Switch12 => new SwitchEntity(_app, new string[]{"switch.switch12"});
        public SwitchEntity ComputerTomas => new SwitchEntity(_app, new string[]{"switch.computer_tomas"});
        public SwitchEntity Remote1B1 => new SwitchEntity(_app, new string[]{"switch.remote_1_b1"});
        public SwitchEntity NetdaemonMagicCube => new SwitchEntity(_app, new string[]{"switch.netdaemon_magic_cube"});
        public SwitchEntity JulbelysningKokH => new SwitchEntity(_app, new string[]{"switch.julbelysning_kok_h"});
        public SwitchEntity Switch16 => new SwitchEntity(_app, new string[]{"switch.switch16"});
        public SwitchEntity JulbelysningVardagsrumV => new SwitchEntity(_app, new string[]{"switch.julbelysning_vardagsrum_v"});
    }

    public partial class DeviceTrackerEntities
    {
        private readonly NetDaemonRxApp _app;
        public DeviceTrackerEntities(NetDaemonRxApp app)
        {
            _app = app;
        }

        public DeviceTrackerEntity GoogleHomeMini4 => new DeviceTrackerEntity(_app, new string[]{"device_tracker.google_home_mini_4"});
        public DeviceTrackerEntity Samsunggalaxys7 => new DeviceTrackerEntity(_app, new string[]{"device_tracker.samsunggalaxys7"});
        public DeviceTrackerEntity MelkerHuaweiWifi => new DeviceTrackerEntity(_app, new string[]{"device_tracker.melker_huawei_wifi"});
        public DeviceTrackerEntity SmG975f => new DeviceTrackerEntity(_app, new string[]{"device_tracker.sm_g975f"});
        public DeviceTrackerEntity ElinsIpad2 => new DeviceTrackerEntity(_app, new string[]{"device_tracker.elins_ipad_2"});
        public DeviceTrackerEntity Elinsipad => new DeviceTrackerEntity(_app, new string[]{"device_tracker.elinsipad"});
        public DeviceTrackerEntity EspKamera12 => new DeviceTrackerEntity(_app, new string[]{"device_tracker.esp_kamera_1_2"});
        public DeviceTrackerEntity GoogleMaps113728439587103002947 => new DeviceTrackerEntity(_app, new string[]{"device_tracker.google_maps_113728439587103002947"});
        public DeviceTrackerEntity Piserver => new DeviceTrackerEntity(_app, new string[]{"device_tracker.piserver"});
        public DeviceTrackerEntity NintendoWiiU => new DeviceTrackerEntity(_app, new string[]{"device_tracker.nintendo_wii_u"});
        public DeviceTrackerEntity Unifi862c82A4F469Default => new DeviceTrackerEntity(_app, new string[]{"device_tracker.unifi_86_2c_82_a4_f4_69_default"});
        public DeviceTrackerEntity ElinGalaxyWifi => new DeviceTrackerEntity(_app, new string[]{"device_tracker.elin_galaxy_wifi"});
        public DeviceTrackerEntity Esp12Test => new DeviceTrackerEntity(_app, new string[]{"device_tracker.esp_12_test"});
        public DeviceTrackerEntity EspKamera1 => new DeviceTrackerEntity(_app, new string[]{"device_tracker.esp_kamera_1"});
        public DeviceTrackerEntity Piserver5 => new DeviceTrackerEntity(_app, new string[]{"device_tracker.piserver_5"});
        public DeviceTrackerEntity Tomasipad => new DeviceTrackerEntity(_app, new string[]{"device_tracker.tomasipad"});
        public DeviceTrackerEntity Piserver2 => new DeviceTrackerEntity(_app, new string[]{"device_tracker.piserver_2"});
        public DeviceTrackerEntity GoogleHomeMini2 => new DeviceTrackerEntity(_app, new string[]{"device_tracker.google_home_mini_2"});
        public DeviceTrackerEntity TomasGalaxyWifi => new DeviceTrackerEntity(_app, new string[]{"device_tracker.tomas_galaxy_wifi"});
        public DeviceTrackerEntity TomasIpad2 => new DeviceTrackerEntity(_app, new string[]{"device_tracker.tomas_ipad_2"});
        public DeviceTrackerEntity GalaxywatchBc7b => new DeviceTrackerEntity(_app, new string[]{"device_tracker.galaxywatch_bc7b"});
        public DeviceTrackerEntity Sallygps => new DeviceTrackerEntity(_app, new string[]{"device_tracker.sallygps"});
        public DeviceTrackerEntity Piserver3 => new DeviceTrackerEntity(_app, new string[]{"device_tracker.piserver_3"});
        public DeviceTrackerEntity Uppe => new DeviceTrackerEntity(_app, new string[]{"device_tracker.uppe"});
        public DeviceTrackerEntity Nere => new DeviceTrackerEntity(_app, new string[]{"device_tracker.nere"});
        public DeviceTrackerEntity EspA82880 => new DeviceTrackerEntity(_app, new string[]{"device_tracker.esp_a82880"});
        public DeviceTrackerEntity ElgatoKeyLightAirA847 => new DeviceTrackerEntity(_app, new string[]{"device_tracker.elgato_key_light_air_a847"});
        public DeviceTrackerEntity GalaxyS8 => new DeviceTrackerEntity(_app, new string[]{"device_tracker.galaxy_s8"});
        public DeviceTrackerEntity HuaweiMate20Pro3c5327 => new DeviceTrackerEntity(_app, new string[]{"device_tracker.huawei_mate_20_pro_3c5327"});
        public DeviceTrackerEntity ElinPresence => new DeviceTrackerEntity(_app, new string[]{"device_tracker.elin_presence"});
        public DeviceTrackerEntity GoogleMaps115932713534918928318 => new DeviceTrackerEntity(_app, new string[]{"device_tracker.google_maps_115932713534918928318"});
        public DeviceTrackerEntity ElinGalaxyWifiOld => new DeviceTrackerEntity(_app, new string[]{"device_tracker.elin_galaxy_wifi_old"});
        public DeviceTrackerEntity MelkerPresence => new DeviceTrackerEntity(_app, new string[]{"device_tracker.melker_presence"});
        public DeviceTrackerEntity Raspberrypi2 => new DeviceTrackerEntity(_app, new string[]{"device_tracker.raspberrypi_2"});
        public DeviceTrackerEntity Unifi04D3B0632d29Default => new DeviceTrackerEntity(_app, new string[]{"device_tracker.unifi_04_d3_b0_63_2d_29_default"});
        public DeviceTrackerEntity GoogleMapsTomash277hassioGmailCom => new DeviceTrackerEntity(_app, new string[]{"device_tracker.google_maps_tomash277hassio_gmail_com"});
        public DeviceTrackerEntity EfraimsIphone => new DeviceTrackerEntity(_app, new string[]{"device_tracker.efraims_iphone"});
        public DeviceTrackerEntity Ipad => new DeviceTrackerEntity(_app, new string[]{"device_tracker.ipad"});
        public DeviceTrackerEntity Melkergps => new DeviceTrackerEntity(_app, new string[]{"device_tracker.melkergps"});
        public DeviceTrackerEntity Xboxsystemos => new DeviceTrackerEntity(_app, new string[]{"device_tracker.xboxsystemos"});
        public DeviceTrackerEntity Unifi0024E451550aDefault => new DeviceTrackerEntity(_app, new string[]{"device_tracker.unifi_00_24_e4_51_55_0a_default"});
        public DeviceTrackerEntity GoogleMaps110786808112177763666 => new DeviceTrackerEntity(_app, new string[]{"device_tracker.google_maps_110786808112177763666"});
        public DeviceTrackerEntity E5cg81709hj => new DeviceTrackerEntity(_app, new string[]{"device_tracker.5cg81709hj"});
        public DeviceTrackerEntity TomasIpad3 => new DeviceTrackerEntity(_app, new string[]{"device_tracker.tomas_ipad_3"});
        public DeviceTrackerEntity XboxSystemos => new DeviceTrackerEntity(_app, new string[]{"device_tracker.xbox_systemos"});
        public DeviceTrackerEntity GoogleHomeMini => new DeviceTrackerEntity(_app, new string[]{"device_tracker.google_home_mini"});
        public DeviceTrackerEntity Tomaspc => new DeviceTrackerEntity(_app, new string[]{"device_tracker.tomaspc"});
        public DeviceTrackerEntity Harmony => new DeviceTrackerEntity(_app, new string[]{"device_tracker.harmony"});
        public DeviceTrackerEntity Googlehome3 => new DeviceTrackerEntity(_app, new string[]{"device_tracker.googlehome_3"});
        public DeviceTrackerEntity E5cg8292f67 => new DeviceTrackerEntity(_app, new string[]{"device_tracker.5cg8292f67"});
        public DeviceTrackerEntity UnifiAcFdCe031c4aDefault => new DeviceTrackerEntity(_app, new string[]{"device_tracker.unifi_ac_fd_ce_03_1c_4a_default"});
        public DeviceTrackerEntity Shelly1 => new DeviceTrackerEntity(_app, new string[]{"device_tracker.shelly_1"});
        public DeviceTrackerEntity Piserver7 => new DeviceTrackerEntity(_app, new string[]{"device_tracker.piserver_7"});
        public DeviceTrackerEntity GoogleHomeMini5 => new DeviceTrackerEntity(_app, new string[]{"device_tracker.google_home_mini_5"});
        public DeviceTrackerEntity ElinsIpad => new DeviceTrackerEntity(_app, new string[]{"device_tracker.elins_ipad"});
        public DeviceTrackerEntity SallyPresence => new DeviceTrackerEntity(_app, new string[]{"device_tracker.sally_presence"});
        public DeviceTrackerEntity Chromecast => new DeviceTrackerEntity(_app, new string[]{"device_tracker.chromecast"});
        public DeviceTrackerEntity Piserver4 => new DeviceTrackerEntity(_app, new string[]{"device_tracker.piserver_4"});
        public DeviceTrackerEntity Tomasgps => new DeviceTrackerEntity(_app, new string[]{"device_tracker.tomasgps"});
        public DeviceTrackerEntity Piserver6 => new DeviceTrackerEntity(_app, new string[]{"device_tracker.piserver_6"});
        public DeviceTrackerEntity GoogleHomeMini3 => new DeviceTrackerEntity(_app, new string[]{"device_tracker.google_home_mini_3"});
        public DeviceTrackerEntity Chromecast5 => new DeviceTrackerEntity(_app, new string[]{"device_tracker.chromecast_5"});
        public DeviceTrackerEntity Raspberrypi3 => new DeviceTrackerEntity(_app, new string[]{"device_tracker.raspberrypi_3"});
        public DeviceTrackerEntity E1921681104F83f512eB4E6 => new DeviceTrackerEntity(_app, new string[]{"device_tracker.192_168_1_104_f8_3f_51_2e_b4_e6"});
        public DeviceTrackerEntity UnifiC417Fe4d8f42Default => new DeviceTrackerEntity(_app, new string[]{"device_tracker.unifi_c4_17_fe_4d_8f_42_default"});
        public DeviceTrackerEntity GoogleHome => new DeviceTrackerEntity(_app, new string[]{"device_tracker.google_home"});
        public DeviceTrackerEntity Raspberrypi => new DeviceTrackerEntity(_app, new string[]{"device_tracker.raspberrypi"});
        public DeviceTrackerEntity Chromecast3 => new DeviceTrackerEntity(_app, new string[]{"device_tracker.chromecast_3"});
        public DeviceTrackerEntity E5cg709284w => new DeviceTrackerEntity(_app, new string[]{"device_tracker.5cg709284w"});
        public DeviceTrackerEntity Piserver8 => new DeviceTrackerEntity(_app, new string[]{"device_tracker.piserver_8"});
        public DeviceTrackerEntity Chromecast4 => new DeviceTrackerEntity(_app, new string[]{"device_tracker.chromecast_4"});
        public DeviceTrackerEntity HuaweiMate10LiteD2b0a => new DeviceTrackerEntity(_app, new string[]{"device_tracker.huawei_mate_10_lite_d2b0a"});
        public DeviceTrackerEntity GalaxyS10 => new DeviceTrackerEntity(_app, new string[]{"device_tracker.galaxy_s10"});
        public DeviceTrackerEntity Sonoff1 => new DeviceTrackerEntity(_app, new string[]{"device_tracker.sonoff1"});
        public DeviceTrackerEntity GoogleMaps118123190245690142371 => new DeviceTrackerEntity(_app, new string[]{"device_tracker.google_maps_118123190245690142371"});
        public DeviceTrackerEntity Dafang => new DeviceTrackerEntity(_app, new string[]{"device_tracker.dafang"});
        public DeviceTrackerEntity ElgatoKeyLightAirAcae => new DeviceTrackerEntity(_app, new string[]{"device_tracker.elgato_key_light_air_acae"});
        public DeviceTrackerEntity Tomass8 => new DeviceTrackerEntity(_app, new string[]{"device_tracker.tomass8"});
        public DeviceTrackerEntity SonoffTest => new DeviceTrackerEntity(_app, new string[]{"device_tracker.sonoff_test"});
        public DeviceTrackerEntity TomasGamlaPad => new DeviceTrackerEntity(_app, new string[]{"device_tracker.tomas_gamla_pad"});
        public DeviceTrackerEntity EspD6983d => new DeviceTrackerEntity(_app, new string[]{"device_tracker.esp_d6983d"});
        public DeviceTrackerEntity TomasIpad4 => new DeviceTrackerEntity(_app, new string[]{"device_tracker.tomas_ipad_4"});
        public DeviceTrackerEntity TomasIpad => new DeviceTrackerEntity(_app, new string[]{"device_tracker.tomas_ipad"});
        public DeviceTrackerEntity TomasPresence => new DeviceTrackerEntity(_app, new string[]{"device_tracker.tomas_presence"});
        public DeviceTrackerEntity Octopi => new DeviceTrackerEntity(_app, new string[]{"device_tracker.octopi"});
        public DeviceTrackerEntity YeelinkLightColor1Miio867704 => new DeviceTrackerEntity(_app, new string[]{"device_tracker.yeelink_light_color1_miio867704"});
        public DeviceTrackerEntity E0024E451550a => new DeviceTrackerEntity(_app, new string[]{"device_tracker.00_24_e4_51_55_0a"});
        public DeviceTrackerEntity SallyHuaweiWifi => new DeviceTrackerEntity(_app, new string[]{"device_tracker.sally_huawei_wifi"});
        public DeviceTrackerEntity Googlehome2 => new DeviceTrackerEntity(_app, new string[]{"device_tracker.googlehome_2"});
        public DeviceTrackerEntity Chromecast2 => new DeviceTrackerEntity(_app, new string[]{"device_tracker.chromecast_2"});
        public DeviceTrackerEntity NintendoWiiU2 => new DeviceTrackerEntity(_app, new string[]{"device_tracker.nintendo_wii_u_2"});
        public DeviceTrackerEntity SamsungGalaxyS7 => new DeviceTrackerEntity(_app, new string[]{"device_tracker.samsung_galaxy_s7"});
    }

    public partial class LightEntities
    {
        private readonly NetDaemonRxApp _app;
        public LightEntities(NetDaemonRxApp app)
        {
            _app = app;
        }

        public LightEntity TvrumFonsterHoger => new LightEntity(_app, new string[]{"light.tvrum_fonster_hoger"});
        public LightEntity JulbelysningVardagsrumH => new LightEntity(_app, new string[]{"light.julbelysning_vardagsrum_h"});
        public LightEntity FarstukvistLed => new LightEntity(_app, new string[]{"light.farstukvist_led"});
        public LightEntity HallDorr => new LightEntity(_app, new string[]{"light.hall_dorr"});
        public LightEntity Sovrum => new LightEntity(_app, new string[]{"light.sovrum"});
        public LightEntity SovrumByra => new LightEntity(_app, new string[]{"light.sovrum_byra"});
        public LightEntity VardagsrumFonsterHoger => new LightEntity(_app, new string[]{"light.vardagsrum_fonster_hoger"});
        public LightEntity ConfigurationTool27 => new LightEntity(_app, new string[]{"light.configuration_tool_27"});
        public LightEntity JulbelysningKokV => new LightEntity(_app, new string[]{"light.julbelysning_kok_v"});
        public LightEntity JulbelysningVardagsrumM => new LightEntity(_app, new string[]{"light.julbelysning_vardagsrum_m"});
        public LightEntity Tvrummet => new LightEntity(_app, new string[]{"light.tvrummet"});
        public LightEntity JulbelysningKokH => new LightEntity(_app, new string[]{"light.julbelysning_kok_h"});
        public LightEntity HallByra => new LightEntity(_app, new string[]{"light.hall_byra"});
        public LightEntity Kok => new LightEntity(_app, new string[]{"light.kok"});
        public LightEntity TvrumBakgrundTv => new LightEntity(_app, new string[]{"light.tvrum_bakgrund_tv"});
        public LightEntity KokFonster => new LightEntity(_app, new string[]{"light.kok_fonster"});
        public LightEntity VardagsrumFonsterVanster => new LightEntity(_app, new string[]{"light.vardagsrum_fonster_vanster"});
        public LightEntity MelkersRumFonster => new LightEntity(_app, new string[]{"light.melkers_rum_fonster"});
        public LightEntity TomasRum => new LightEntity(_app, new string[]{"light.tomas_rum"});
        public LightEntity KokLillaFonster => new LightEntity(_app, new string[]{"light.kok_lilla_fonster"});
        public LightEntity TvrumVagg => new LightEntity(_app, new string[]{"light.tvrum_vagg"});
        public LightEntity VardagsrumFonsterMitten => new LightEntity(_app, new string[]{"light.vardagsrum_fonster_mitten"});
        public LightEntity Vardagsrum => new LightEntity(_app, new string[]{"light.vardagsrum"});
        public LightEntity TvrumFonsterVanster => new LightEntity(_app, new string[]{"light.tvrum_fonster_vanster"});
        public LightEntity TomasRumFonster => new LightEntity(_app, new string[]{"light.tomas_rum_fonster"});
        public LightEntity Blinds => new LightEntity(_app, new string[]{"light.blinds"});
        public LightEntity SallysRumFonster => new LightEntity(_app, new string[]{"light.sallys_rum_fonster"});
        public LightEntity JulbelysningVardagsrumV => new LightEntity(_app, new string[]{"light.julbelysning_vardagsrum_v"});
        public LightEntity Ambient => new LightEntity(_app, new string[]{"light.ambient"});
        public LightEntity JulbelysningTomasRum => new LightEntity(_app, new string[]{"light.julbelysning_tomas_rum"});
        public LightEntity JulbelysningSovrum => new LightEntity(_app, new string[]{"light.julbelysning_sovrum"});
        public LightEntity SovrumFonster => new LightEntity(_app, new string[]{"light.sovrum_fonster"});
        public LightEntity Farstukvisten => new LightEntity(_app, new string[]{"light.farstukvisten"});
        public LightEntity MelkersRum => new LightEntity(_app, new string[]{"light.melkers_rum"});
        public LightEntity SallysRum => new LightEntity(_app, new string[]{"light.sallys_rum"});
        public LightEntity Group61506 => new LightEntity(_app, new string[]{"light.group_61506"});
    }

    public partial class ProximityEntities
    {
        private readonly NetDaemonRxApp _app;
        public ProximityEntities(NetDaemonRxApp app)
        {
            _app = app;
        }

        public ProximityEntity ProxHomeMelker => new ProximityEntity(_app, new string[]{"proximity.prox_home_melker"});
        public ProximityEntity ProxHomeSally => new ProximityEntity(_app, new string[]{"proximity.prox_home_sally"});
        public ProximityEntity ProxHomeTomas => new ProximityEntity(_app, new string[]{"proximity.prox_home_tomas"});
        public ProximityEntity ProxHomeElin => new ProximityEntity(_app, new string[]{"proximity.prox_home_elin"});
    }

    public partial class GroupEntities
    {
        private readonly NetDaemonRxApp _app;
        public GroupEntities(NetDaemonRxApp app)
        {
            _app = app;
        }

        public GroupEntity Climate => new GroupEntity(_app, new string[]{"group.climate"});
        public GroupEntity SystemMetrix => new GroupEntity(_app, new string[]{"group.system_metrix"});
        public GroupEntity Dummy => new GroupEntity(_app, new string[]{"group.dummy"});
        public GroupEntity MelkersDevices => new GroupEntity(_app, new string[]{"group.melkers_devices"});
        public GroupEntity ElinsDevices => new GroupEntity(_app, new string[]{"group.elins_devices"});
        public GroupEntity TomasDevices => new GroupEntity(_app, new string[]{"group.tomas_devices"});
        public GroupEntity PeopleStatus => new GroupEntity(_app, new string[]{"group.people_status"});
        public GroupEntity LowBatteryDevices => new GroupEntity(_app, new string[]{"group.low_battery_devices"});
        public GroupEntity Remotes => new GroupEntity(_app, new string[]{"group.remotes"});
        public GroupEntity Googlehomes => new GroupEntity(_app, new string[]{"group.googlehomes"});
        public GroupEntity Presence => new GroupEntity(_app, new string[]{"group.presence"});
        public GroupEntity Chromecasts => new GroupEntity(_app, new string[]{"group.chromecasts"});
        public GroupEntity Kodis => new GroupEntity(_app, new string[]{"group.kodis"});
        public GroupEntity SallysDevices => new GroupEntity(_app, new string[]{"group.sallys_devices"});
        public GroupEntity KameraUppe => new GroupEntity(_app, new string[]{"group.kamera_uppe"});
        public GroupEntity Cameras => new GroupEntity(_app, new string[]{"group.cameras"});
    }

    public partial class InputNumberEntities
    {
        private readonly NetDaemonRxApp _app;
        public InputNumberEntities(NetDaemonRxApp app)
        {
            _app = app;
        }

        public InputNumberEntity CarHeaterDepTimeHour => new InputNumberEntity(_app, new string[]{"input_number.car_heater_dep_time_hour"});
        public InputNumberEntity CarHeaterDepTimeMinutes => new InputNumberEntity(_app, new string[]{"input_number.car_heater_dep_time_minutes"});
    }

    public partial class ZoneEntities
    {
        private readonly NetDaemonRxApp _app;
        public ZoneEntities(NetDaemonRxApp app)
        {
            _app = app;
        }

        public ZoneEntity Vardinge => new ZoneEntity(_app, new string[]{"zone.vardinge"});
        public ZoneEntity Sjukhuset => new ZoneEntity(_app, new string[]{"zone.sjukhuset"});
        public ZoneEntity Garn => new ZoneEntity(_app, new string[]{"zone.garn"});
        public ZoneEntity Home => new ZoneEntity(_app, new string[]{"zone.home"});
        public ZoneEntity Svarmor => new ZoneEntity(_app, new string[]{"zone.svarmor"});
        public ZoneEntity Spv => new ZoneEntity(_app, new string[]{"zone.spv"});
    }

    public partial class MediaPlayerEntities
    {
        private readonly NetDaemonRxApp _app;
        public MediaPlayerEntities(NetDaemonRxApp app)
        {
            _app = app;
        }

        public MediaPlayerEntity PlexPlexCastChromecast => new MediaPlayerEntity(_app, new string[]{"media_player.plex_plex_cast_chromecast"});
        public MediaPlayerEntity Huset => new MediaPlayerEntity(_app, new string[]{"media_player.huset"});
        public MediaPlayerEntity KodiTvNere => new MediaPlayerEntity(_app, new string[]{"media_player.kodi_tv_nere"});
        public MediaPlayerEntity TvNere => new MediaPlayerEntity(_app, new string[]{"media_player.tv_nere"});
        public MediaPlayerEntity PlexPlexWebChrome => new MediaPlayerEntity(_app, new string[]{"media_player.plex_plex_web_chrome"});
        public MediaPlayerEntity PlexChromecast => new MediaPlayerEntity(_app, new string[]{"media_player.plex_chromecast"});
        public MediaPlayerEntity PlexChrome2 => new MediaPlayerEntity(_app, new string[]{"media_player.plex_chrome_2"});
        public MediaPlayerEntity PlexChrome => new MediaPlayerEntity(_app, new string[]{"media_player.plex_chrome"});
        public MediaPlayerEntity PlexKodiAddOnLibreelec => new MediaPlayerEntity(_app, new string[]{"media_player.plex_kodi_add_on_libreelec"});
        public MediaPlayerEntity SallysHogtalare => new MediaPlayerEntity(_app, new string[]{"media_player.sallys_hogtalare"});
        public MediaPlayerEntity MelkersRum => new MediaPlayerEntity(_app, new string[]{"media_player.melkers_rum"});
        public MediaPlayerEntity Vardagsrum => new MediaPlayerEntity(_app, new string[]{"media_player.vardagsrum"});
        public MediaPlayerEntity TvRummetGh => new MediaPlayerEntity(_app, new string[]{"media_player.tv_rummet_gh"});
        public MediaPlayerEntity TvUppe2 => new MediaPlayerEntity(_app, new string[]{"media_player.tv_uppe2"});
        public MediaPlayerEntity Kok => new MediaPlayerEntity(_app, new string[]{"media_player.kok"});
        public MediaPlayerEntity Sovrum => new MediaPlayerEntity(_app, new string[]{"media_player.sovrum"});
        public MediaPlayerEntity PlexTomasIpad => new MediaPlayerEntity(_app, new string[]{"media_player.plex_tomas_ipad"});
        public MediaPlayerEntity MelkersTv => new MediaPlayerEntity(_app, new string[]{"media_player.melkers_tv"});
        public MediaPlayerEntity PlexChrome3 => new MediaPlayerEntity(_app, new string[]{"media_player.plex_chrome_3"});
        public MediaPlayerEntity TvUppe => new MediaPlayerEntity(_app, new string[]{"media_player.tv_uppe"});
    }

    public partial class ScriptEntities
    {
        private readonly NetDaemonRxApp _app;
        public ScriptEntities(NetDaemonRxApp app)
        {
            _app = app;
        }

        public ScriptEntity TvScene => new ScriptEntity(_app, new string[]{"script.tv_scene"});
        public ScriptEntity NightScene => new ScriptEntity(_app, new string[]{"script.night_scene"});
        public ScriptEntity Setnightmode => new ScriptEntity(_app, new string[]{"script.setnightmode"});
        public ScriptEntity CleaningScene => new ScriptEntity(_app, new string[]{"script.cleaning_scene"});
        public ScriptEntity NotifyGreet => new ScriptEntity(_app, new string[]{"script.notify_greet"});
        public ScriptEntity EveningScene => new ScriptEntity(_app, new string[]{"script.evening_scene"});
        public ScriptEntity Notify => new ScriptEntity(_app, new string[]{"script.notify"});
        public ScriptEntity MorningScene => new ScriptEntity(_app, new string[]{"script.morning_scene"});
        public ScriptEntity TvOffScene => new ScriptEntity(_app, new string[]{"script.tv_off_scene"});
        public ScriptEntity FilmScene => new ScriptEntity(_app, new string[]{"script.film_scene"});
        public ScriptEntity ColorScene => new ScriptEntity(_app, new string[]{"script.color_scene"});
        public ScriptEntity E1586350051032 => new ScriptEntity(_app, new string[]{"script.1586350051032"});
        public ScriptEntity DayScene => new ScriptEntity(_app, new string[]{"script.day_scene"});
    }

    public partial class BinarySensorEntities
    {
        private readonly NetDaemonRxApp _app;
        public BinarySensorEntities(NetDaemonRxApp app)
        {
            _app = app;
        }

        public BinarySensorEntity SallysRumPir => new BinarySensorEntity(_app, new string[]{"binary_sensor.sallys_rum_pir"});
        public BinarySensorEntity MelkersRumPir => new BinarySensorEntity(_app, new string[]{"binary_sensor.melkers_rum_pir"});
        public BinarySensorEntity TvrumPir => new BinarySensorEntity(_app, new string[]{"binary_sensor.tvrum_pir"});
        public BinarySensorEntity Updater => new BinarySensorEntity(_app, new string[]{"binary_sensor.updater"});
        public BinarySensorEntity Sonoff1Button => new BinarySensorEntity(_app, new string[]{"binary_sensor.sonoff1_button"});
        public BinarySensorEntity KokPir => new BinarySensorEntity(_app, new string[]{"binary_sensor.kok_pir"});
        public BinarySensorEntity SweRecyclingVattjom => new BinarySensorEntity(_app, new string[]{"binary_sensor.swe_recycling_vattjom"});
        public BinarySensorEntity TrappPir => new BinarySensorEntity(_app, new string[]{"binary_sensor.trapp_pir"});
        public BinarySensorEntity TomasRumPir => new BinarySensorEntity(_app, new string[]{"binary_sensor.tomas_rum_pir"});
        public BinarySensorEntity SovrumPir => new BinarySensorEntity(_app, new string[]{"binary_sensor.sovrum_pir"});
        public BinarySensorEntity SwInput => new BinarySensorEntity(_app, new string[]{"binary_sensor.sw_input"});
        public BinarySensorEntity SweRecyclingMatfors => new BinarySensorEntity(_app, new string[]{"binary_sensor.swe_recycling_matfors"});
        public BinarySensorEntity SnapshotsStale => new BinarySensorEntity(_app, new string[]{"binary_sensor.snapshots_stale"});
        public BinarySensorEntity VardagsrumPir => new BinarySensorEntity(_app, new string[]{"binary_sensor.vardagsrum_pir"});
        public BinarySensorEntity Kamera3MotionDetected => new BinarySensorEntity(_app, new string[]{"binary_sensor.kamera_3_motion_detected"});
        public BinarySensorEntity HallDorr => new BinarySensorEntity(_app, new string[]{"binary_sensor.hall_dorr"});
    }

    public partial class AutomationEntities
    {
        private readonly NetDaemonRxApp _app;
        public AutomationEntities(NetDaemonRxApp app)
        {
            _app = app;
        }

        public AutomationEntity SetThemeAtStartup => new AutomationEntity(_app, new string[]{"automation.set_theme_at_startup"});
    }

    public partial class NetdaemonEntities
    {
        private readonly NetDaemonRxApp _app;
        public NetdaemonEntities(NetDaemonRxApp app)
        {
            _app = app;
        }

        public NetdaemonEntity Status => new NetdaemonEntity(_app, new string[]{"netdaemon.status"});
    }

    public partial class SceneEntities
    {
        private readonly NetDaemonRxApp _app;
        public SceneEntities(NetDaemonRxApp app)
        {
            _app = app;
        }

        public SceneEntity Morgon => new SceneEntity(_app, new string[]{"scene.morgon"});
        public SceneEntity Natt => new SceneEntity(_app, new string[]{"scene.natt"});
        public SceneEntity Kvall => new SceneEntity(_app, new string[]{"scene.kvall"});
        public SceneEntity Dag => new SceneEntity(_app, new string[]{"scene.dag"});
        public SceneEntity Stadning => new SceneEntity(_app, new string[]{"scene.stadning"});
    }

    public partial class PersonEntities
    {
        private readonly NetDaemonRxApp _app;
        public PersonEntities(NetDaemonRxApp app)
        {
            _app = app;
        }

        public PersonEntity Melker => new PersonEntity(_app, new string[]{"person.melker"});
        public PersonEntity Sally => new PersonEntity(_app, new string[]{"person.sally"});
        public PersonEntity Tomas => new PersonEntity(_app, new string[]{"person.tomas"});
        public PersonEntity Elin => new PersonEntity(_app, new string[]{"person.elin"});
    }

    public partial class WeatherEntities
    {
        private readonly NetDaemonRxApp _app;
        public WeatherEntities(NetDaemonRxApp app)
        {
            _app = app;
        }

        public WeatherEntity SmhiHemma => new WeatherEntity(_app, new string[]{"weather.smhi_hemma"});
    }

    public partial class SunEntities
    {
        private readonly NetDaemonRxApp _app;
        public SunEntities(NetDaemonRxApp app)
        {
            _app = app;
        }

        public SunEntity Sun => new SunEntity(_app, new string[]{"sun.sun"});
    }

    public partial class InputSelectEntities
    {
        private readonly NetDaemonRxApp _app;
        public InputSelectEntities(NetDaemonRxApp app)
        {
            _app = app;
        }

        public InputSelectEntity HouseModeSelectTest => new InputSelectEntity(_app, new string[]{"input_select.house_mode_select_test"});
        public InputSelectEntity HouseModeSelect => new InputSelectEntity(_app, new string[]{"input_select.house_mode_select"});
    }

    public partial class RemoteEntities
    {
        private readonly NetDaemonRxApp _app;
        public RemoteEntities(NetDaemonRxApp app)
        {
            _app = app;
        }

        public RemoteEntity Tvrummet => new RemoteEntity(_app, new string[]{"remote.tvrummet"});
    }

    public partial class CameraEntities
    {
        private readonly NetDaemonRxApp _app;
        public CameraEntities(NetDaemonRxApp app)
        {
            _app = app;
        }

        public CameraEntity Kamera3 => new CameraEntity(_app, new string[]{"camera.kamera_3"});
        public CameraEntity KameraStream => new CameraEntity(_app, new string[]{"camera.kamera_stream"});
        public CameraEntity MyCamera => new CameraEntity(_app, new string[]{"camera.my_camera"});
    }

    public partial class CalendarEntities
    {
        private readonly NetDaemonRxApp _app;
        public CalendarEntities(NetDaemonRxApp app)
        {
            _app = app;
        }

        public CalendarEntity Tomash277GmailCom => new CalendarEntity(_app, new string[]{"calendar.tomash277_gmail_com"});
        public CalendarEntity TaUtSopor => new CalendarEntity(_app, new string[]{"calendar.ta_ut_sopor"});
        public CalendarEntity FamiljenHellstrom => new CalendarEntity(_app, new string[]{"calendar.familjen_hellstrom"});
        public CalendarEntity SundsvallsFotoklubb => new CalendarEntity(_app, new string[]{"calendar.sundsvalls_fotoklubb"});
    }

    public partial class InputBooleanEntities
    {
        private readonly NetDaemonRxApp _app;
        public InputBooleanEntities(NetDaemonRxApp app)
        {
            _app = app;
        }

        public InputBooleanEntity GoodNightHouse => new InputBooleanEntity(_app, new string[]{"input_boolean.good_night_house"});
        public InputBooleanEntity ScheduleOnWeekends => new InputBooleanEntity(_app, new string[]{"input_boolean.schedule_on_weekends"});
    }

    public partial class CoverEntities
    {
        private readonly NetDaemonRxApp _app;
        public CoverEntities(NetDaemonRxApp app)
        {
            _app = app;
        }

        public CoverEntity Tvrum => new CoverEntity(_app, new string[]{"cover.tvrum"});
        public CoverEntity TvrumRullgardinVanster => new CoverEntity(_app, new string[]{"cover.tvrum_rullgardin_vanster"});
        public CoverEntity TvrumRullgardinHoger => new CoverEntity(_app, new string[]{"cover.tvrum_rullgardin_hoger"});
    }
}