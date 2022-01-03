using System.Text.Json.Serialization;
using NetDaemon.HassModel.Common;
using NetDaemon.HassModel.Entities;

namespace NetDaemon.HassModel.Tests.TestHelpers.HassClient
{
    record TestEntity : Entity<TestEntity, EntityState<TestEntityAttributes>, TestEntityAttributes>
    {
        public TestEntity(IHaContext haContext, string entityId) : base(haContext, entityId) { }
    }

    public record TestEntityAttributes
    {
        [JsonPropertyName("name")] public string Name { get; set; } = "";
    }

    public record NumericTestEntity : NumericEntity<NumericTestEntity, NumericEntityState<TestEntityAttributes>, TestEntityAttributes>
    {
        public NumericTestEntity(Entity entity) : base(entity)
        { }

        public NumericTestEntity(IHaContext haContext, string entityId) : base(haContext, entityId)
        { }
    }
}