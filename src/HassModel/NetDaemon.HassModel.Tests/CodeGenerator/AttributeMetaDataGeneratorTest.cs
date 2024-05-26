using System.Text.Json;
using NetDaemon.Client.HomeAssistant.Model;
using NetDaemon.HassModel.CodeGenerator;
using NetDaemon.HassModel.Tests.TestHelpers;

namespace NetDaemon.HassModel.Tests.CodeGenerator;

public class AttributeMetaDataGeneratorTest
{
    [Fact]
    public void SameAttributeDifferentTypes_obect()
    {
        var entityStates = new HassState[] {
            new() { AttributesJson = new { size = "Large", }.AsJsonElement() },
            new() { AttributesJson = new { size = 2, }.AsJsonElement() },
        };

        var metadata = AttributeMetaDataGenerator.GetMetaDataFromEntityStates(entityStates);

        metadata.Should().BeEquivalentTo(new[] { new EntityAttributeMetaData("size", "Size", typeof(object)) });
    }

    [Fact]
    public void SameAttributeDifferentArrayTypes_obect()
    {
        var entityStates = new HassState[] {
            new() { AttributesJson = new { values = new []{ [1,2,3], new []{ 2,3,4 } }, }.AsJsonElement() },
        };

        var metadata = AttributeMetaDataGenerator.GetMetaDataFromEntityStates(entityStates).ToArray();

        metadata.Should().BeEquivalentTo(new[] { new EntityAttributeMetaData("values", "Values", typeof(IReadOnlyList<IReadOnlyList<double>>)) });


        var copy = SerializeAndDeserialize(metadata.First());
        copy.Should().Be(metadata.First());
    }

    [Fact]
    public void AllNullShouldBeNull()
    {
        var entityStates = new HassState[] {
            new() { AttributesJson = new { size = (object?)null, }.AsJsonElement() },
            new() { AttributesJson = new { size = (object?)null, }.AsJsonElement() },
        };

        var metadata = AttributeMetaDataGenerator.GetMetaDataFromEntityStates(entityStates);

        metadata.Should().BeEquivalentTo(new[] { new EntityAttributeMetaData("size", "Size", null) });
    }

    [Fact]
    public void IncludeSaveAndMergeState()
    {
        // first we have a few lights that are ON and therefore have a brightness propert as a double
        var entityStates = new HassState[] {
            new() {EntityId = "light.attic", AttributesJson = new { brightness = 2.1d, }.AsJsonElement() },
            new() {EntityId = "light.livingroom", AttributesJson = new { brightness = 2.2d, }.AsJsonElement() },
        };

        var metadata = AttributeMetaDataGenerator.GetMetaDataFromEntityStates(entityStates).ToList();
        // lets pretend it gets saved and reloaded from disk
        metadata = SerializeAndDeserialize(metadata);

        // now get new metadata while the lights are off (brightness = null)
        var newEntityStates = new HassState[] {
            new() {EntityId = "light.attic", AttributesJson = new { brightness = (object?)null, }.AsJsonElement() },
            new() {EntityId = "light.livingroom", AttributesJson = new { brightness = (object?)null, }.AsJsonElement() },
        };

        var newMetadata = AttributeMetaDataGenerator.GetMetaDataFromEntityStates(newEntityStates).ToList();

        // merge the previous and current metadata
        var merged = EntityMetaDataMerger.Merge(new CodeGenerationSettings(),
            new EntitiesMetaData { Domains = new []{new EntityDomainMetadata("light", false, Array.Empty<EntityMetaData>(), metadata!)}},
            new EntitiesMetaData { Domains = new []{new EntityDomainMetadata("light", false, Array.Empty<EntityMetaData>(), newMetadata)}}
            );

        // We should still have Brightness as a double
        merged.Domains.First().Attributes.Should().BeEquivalentTo(new[] { new EntityAttributeMetaData("brightness", "Brightness", typeof(double)) });
    }


    private static T? SerializeAndDeserialize<T>(T input)
    {
        var serializeOptions = new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            Converters = { new ClrTypeJsonConverter() }
        };

        var json = JsonSerializer.Serialize(input, serializeOptions);
        return JsonSerializer.Deserialize<T>(json, serializeOptions);
    }

}
