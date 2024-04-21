﻿using System.Text.Json.Serialization;
using NetDaemon.HassModel.Entities;

namespace NetDaemon.HassModel.Tests.TestHelpers.HassClient;

sealed record TestEntity : Entity<EntityState<string, TestEntityAttributes>, TestEntityAttributes, string>, IEntity<TestEntity, EntityState<string, TestEntityAttributes>, TestEntityAttributes, string>
{
    public TestEntity(IHaContext haContext, string entityId) : base(haContext, entityId) { }
}

public record TestEntityAttributes
{
    [JsonPropertyName("name")] public string Name { get; set; } = "";
}

public record NumericTestEntity : Entity<EntityState<double, TestEntityAttributes>, TestEntityAttributes, double>, IEntity<NumericTestEntity, EntityState<double, TestEntityAttributes>, TestEntityAttributes, string>
{
    public NumericTestEntity(Entity entity) : base(entity)
    { }

    public NumericTestEntity(IHaContext haContext, string entityId) : base(haContext, entityId)
    { }
}
