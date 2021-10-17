using System;
using System.Collections.Generic;
using System.Text.Json;
using NetDaemon.HassModel.Common;

namespace NetDaemon.HassModel.Entities
{
    /// <summary>
    /// Detailed state information
    /// </summary>
    public record EntityState
    {
        /// <summary>Unique id of the entity</summary>
        public string EntityId { get; init; } = "";
    
        /// <summary>The state </summary>
        public string? State { get; init; }

        internal JsonElement? AttributesJson { get; init; }
        
        /// <summary>
        /// The attributes
        /// </summary>
        public virtual object? Attributes => AttributesJson?.ToObject<Dictionary<string, object>>() ?? new Dictionary<string, object>();
    
        /// <summary>Last changed, when state changed from and to different values</summary>
        public DateTime? LastChanged { get; init; }
    
        /// <summary>Last updated, when entity state or attributes changed </summary>
        public DateTime? LastUpdated { get; init; }
    
        /// <summary>Context</summary>
        public Context? Context { get; init; }
    }
    
    /// <summary>
    /// Generic EntityState with specific types of State and Attributes
    /// </summary>
    /// <typeparam name="TAttributes">The type of the Attributes Property</typeparam>
    public record EntityState<TAttributes> : EntityState 
        where TAttributes : class
    {
        private readonly Lazy<TAttributes?> _attributesLazy;

        /// <summary>
        /// Copy constructor from base class
        /// </summary>
        /// <param name="source"></param>
        public EntityState(EntityState source) : base(source)
        {
            _attributesLazy = new (() => AttributesJson?.ToObject<TAttributes>() ?? default);            
        }

        /// <inheritdoc/>
        public override TAttributes? Attributes => _attributesLazy.Value;
    }
}