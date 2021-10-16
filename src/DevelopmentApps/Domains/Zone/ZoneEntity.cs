using System.Diagnostics.CodeAnalysis;
using NetDaemon.Model3.Common;
using NetDaemon.Model3.Entities;

namespace NetDaemon.DevelopmentApps.Domains.Zone
{
    public record ZoneEntity : Entity<ZoneEntity, EntityState<ZoneAttributes>, ZoneAttributes>
    {
        public ZoneEntity(IHaContext haContext, string entityId) : base(haContext, entityId)
        { }
    }

    [SuppressMessage("", "CA1707")]
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