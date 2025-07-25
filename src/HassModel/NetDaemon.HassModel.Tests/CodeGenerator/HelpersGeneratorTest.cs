using NetDaemon.Client.HomeAssistant.Model;
using NetDaemon.HassModel.CodeGenerator;
using NetDaemon.HassModel.CodeGenerator.CodeGeneration;

namespace NetDaemon.HassModel.Tests.CodeGenerator;

public class HelpersGeneratorTest
{
    [Fact]
    public void AddHomeAssistantGenerated_ShouldNotRegisterDuplicateEntityClasses()
    {
        // Arrange: Create test data that would lead to duplicate SensorEntities registrations
        var states = new HassState[]
        {
            // Non-numeric sensor (no unit_of_measurement)
            new() { EntityId = "sensor.simple_text", Attributes = new Dictionary<string, object>() },
            
            // Numeric sensor (has unit_of_measurement)  
            new() { EntityId = "sensor.temperature", Attributes = new Dictionary<string, object> { ["unit_of_measurement"] = "°C" } }
        };
        
        // Act: Generate metadata which creates both numeric and non-numeric sensor domains
        var metaData = EntityMetaDataGenerator.GetEntityDomainMetaData(states);
        
        // Both should have the same EntitiesForDomainClassName
        var sensorDomains = metaData.Domains.Where(d => d.Domain == "sensor").ToList();
        sensorDomains.Should().HaveCount(2, "there should be both numeric and non-numeric sensor domains");
        sensorDomains.Should().AllSatisfy(d => d.EntitiesForDomainClassName.Should().Be("SensorEntities"));
        
        // Generate the extension method code
        var generatedMembers = HelpersGenerator.Generate(metaData.Domains, []).ToList();
        var generatedCode = generatedMembers.First().ToString();
        
        // Assert: The generated code should not contain duplicate SensorEntities registrations
        var sensorEntitiesMatches = System.Text.RegularExpressions.Regex.Matches(
            generatedCode, 
            @"serviceCollection\.AddTransient<SensorEntities>\(\);");
            
        sensorEntitiesMatches.Should().HaveCount(1, "SensorEntities should only be registered once, not duplicated");
    }
    
    [Fact]
    public void AddHomeAssistantGenerated_ShouldRegisterAllUniqueEntityClasses()
    {
        // Arrange: Create test data with different domains
        var states = new HassState[]
        {
            new() { EntityId = "sensor.temperature", Attributes = new Dictionary<string, object> { ["unit_of_measurement"] = "°C" } },
            new() { EntityId = "light.living_room", Attributes = new Dictionary<string, object>() },
            new() { EntityId = "switch.kitchen", Attributes = new Dictionary<string, object>() }
        };
        
        // Act: Generate metadata
        var metaData = EntityMetaDataGenerator.GetEntityDomainMetaData(states);
        
        // Generate the extension method code
        var generatedMembers = HelpersGenerator.Generate(metaData.Domains, []).ToList();
        var generatedCode = generatedMembers.First().ToString();
        
        // Assert: Each domain should be registered exactly once
        generatedCode.Should().Contain("serviceCollection.AddTransient<SensorEntities>();");
        generatedCode.Should().Contain("serviceCollection.AddTransient<LightEntities>();");
        generatedCode.Should().Contain("serviceCollection.AddTransient<SwitchEntities>();");
        
        // Verify no duplicates
        var sensorMatches = System.Text.RegularExpressions.Regex.Matches(generatedCode, @"serviceCollection\.AddTransient<SensorEntities>\(\);");
        var lightMatches = System.Text.RegularExpressions.Regex.Matches(generatedCode, @"serviceCollection\.AddTransient<LightEntities>\(\);");
        var switchMatches = System.Text.RegularExpressions.Regex.Matches(generatedCode, @"serviceCollection\.AddTransient<SwitchEntities>\(\);");
        
        sensorMatches.Should().HaveCount(1);
        lightMatches.Should().HaveCount(1); 
        switchMatches.Should().HaveCount(1);
    }
}