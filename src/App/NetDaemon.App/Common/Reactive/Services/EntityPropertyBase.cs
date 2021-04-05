using System;

namespace NetDaemon.Common.Reactive.Services
{
    public record EntityPropertyBase : IEntityProperties
    {
        protected readonly IEntityProperties _innerProperties;

        public EntityPropertyBase(IEntityProperties innerProperties)
        {
            _innerProperties = innerProperties;
        }

        public string? Area => _innerProperties.Area;

        public dynamic? Attribute => _innerProperties.Attribute;

        public string EntityId => _innerProperties.EntityId;

        public DateTime LastChanged => _innerProperties.LastChanged;

        public DateTime LastUpdated => _innerProperties.LastUpdated;

        public object? State => _innerProperties.State;

        public Context? Context => _innerProperties.Context;
    }
}