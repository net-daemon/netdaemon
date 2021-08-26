using System;
using System.Reactive.Linq;
using Model3;

namespace NetDaemon.Common.ModelV3.Domains
{
    public record ZoneEntity : Entity<ZoneEntity, EntityState<string, ZoneAttributes>, string, ZoneAttributes>
    {
        public ZoneEntity(IHaContext haContext, string entityId) : base(haContext, entityId)
        { }
    }

    public record ZoneAttributes
    {
        // TODO: complete these props and correct casing using [JsonPropertyName] (this is really an example)
        // maybe we can implement automatic mapping of snake_case to PascalCase

        public bool hidden { get; init; }
        public double latitude { get; init; }
        public double longitude { get; init; }
        public double radius { get; init; }
        public string? friendly_name { get; init; }
        public string? icon { get; init; }
    }
}