using System;
using JoySoftware.HomeAssistant.Model;
using Model3;

namespace NetDaemon.Common.ModelV3.Domains
{
    public record ZoneState : EntityState
    {
        public ZoneState(EntityState source) : base(source)
        { }
        
        public override ZoneAttributes Attributes => AttributesJson.ToObject<ZoneAttributes>();
    }
    
    public record ZoneAttributes
    {
        public bool hidden { get; init; }

        public double latitude { get; init; }
        public double longitude { get; init; }
        public double radius { get; init; }
        public string friendly_name { get; init; }
        public string icon { get; init; }
    }
    
    public record ZoneStateChange : StateChange
    {
        public override ZoneState New => new(new());
        public override ZoneState Old => new(new());
        public override ZoneEntity Entity => new(null!, null!);
    }

    public class ZoneEntity : Entity
    {
        public ZoneEntity(IHaContext daemon, string entityId) : base(daemon, entityId)
        {
        }

        //public override ZoneState? EntityState { get; }

        public override IObservable<ZoneStateChange> StateAllChanges => null!;
    }
}