using System.Text.Json.Serialization;
using NetDaemon.HassModel.Common;
using NetDaemon.HassModel.Entities;

namespace NetDaemon.HassModel.Tests.TestHelpers
{
    record TestEntity : Entity<TestEntity, EntityState<TestEntityAttributes>, TestEntityAttributes>
    {
        public TestEntity(IHaContext haContext, string entityId) : base(haContext, entityId) { }
    }
 
    record TestEntityAttributes
    {
        [JsonPropertyName("name")] public string Name { get; set; } = "";
    }
}