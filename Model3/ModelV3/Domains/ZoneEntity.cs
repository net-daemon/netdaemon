using System;
using System.Reactive.Linq;
using Model3;

namespace NetDaemon.Common.ModelV3.Domains
{
    public record ZoneEntity : Entity<ZoneEntity, ZoneState>
    {
        public ZoneEntity(IHaContext daemon, string entityId) : base(daemon, entityId)
        { }
    }

    public record ZoneState : EntityState<ZoneAttributes>
    {
        public ZoneState(EntityState source) : base(source) { }
    }
    
    public record ZoneAttributes
    {
        public bool hidden { get; init; }
        public double latitude { get; init; }
        public double longitude { get; init; }
        public double radius { get; init; }
        public string? friendly_name { get; init; }
        public string? icon { get; init; }
    }
}