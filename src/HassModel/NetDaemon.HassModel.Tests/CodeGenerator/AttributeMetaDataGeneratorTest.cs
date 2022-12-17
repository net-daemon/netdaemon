using System.Collections.Generic;
using System.Linq;
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
            new() { AttributesJson = new { values = new []{new []{ 1,2,3 }, new []{ 2,3,4 } }, }.AsJsonElement() },           
        };

        var metadata = AttributeMetaDataGenerator.GetMetaDataFromEntityStates(entityStates).ToArray();

        metadata.Should().BeEquivalentTo(new[] { new EntityAttributeMetaData("values", "Values", typeof(IReadOnlyList<IReadOnlyList<double>>)) });


        var copy = SerializeAndDeserialize(metadata.First());
        copy.Should().Be(metadata.First());
    }


    private EntityAttributeMetaData? SerializeAndDeserialize(EntityAttributeMetaData input)
    {
        var serializeOptions = new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            Converters = { new ClrTypeJsonConverter() }
        };

        var json = JsonSerializer.Serialize(input, serializeOptions);
        return JsonSerializer.Deserialize<EntityAttributeMetaData>(json, serializeOptions);
    }

}