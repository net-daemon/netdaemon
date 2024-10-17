using System.Diagnostics.CodeAnalysis;
using System.Runtime.Loader;
using Microsoft.Extensions.DependencyInjection;
using NetDaemon.Client;
using NetDaemon.Client.HomeAssistant.Model;
using NetDaemon.HassModel.CodeGenerator;
using NetDaemon.HassModel.CodeGenerator.Model;
using NetDaemon.HassModel.Internal;

namespace NetDaemon.HassModel.Tests.CodeGenerator;

public sealed  class EntityFactoryGeneratorTest : IDisposable
{
    readonly AssemblyLoadContext _assemblyLoadContext = new (name: null, isCollectible: true);

    public EntityFactoryGeneratorTest()
    {
        // We need to force Fluent Assertions to be loaded in order to use it in dynamic code as well
        true.Should().Be(true);
    }

    private readonly CodeGenerationSettings _settings = new();

    [Fact]
    public void GeneratedEntityShouldCreateCorrectGeneratedEntityTypes()
    {
        var states = new HassState[] {
            new() { EntityId = "media_player.my_media" },
            new() { EntityId = "sensor.sun_next_rising" },
            new() { EntityId = "sensor.daily_gas", Attributes = new Dictionary<string, object>{["unit_of_measurement"]="Kwh"}},
        };

        var code = CodeGenTestHelper.GenerateCompilationUnit(_settings, states);

        var haMock = new Mock<IHaContext>();
        haMock.Setup(m => m.GetState(It.IsAny<string>())).Returns((string id) => states.FirstOrDefault(s => s.EntityId == id).Map());

        var assembly = CodeGenTestHelper.LoadDynamicAssembly(code.ToString(), "", _assemblyLoadContext);

        var serviceCollection = new ServiceCollection();
        // Dynamically call AddHomeAssistantGenerated(serviceCollection);
        assembly.GetType("HomeAssistantGenerated.GeneratedExtensions")!.GetMethod("AddHomeAssistantGenerated")!.Invoke(null, [serviceCollection]);

        var sp = serviceCollection.BuildServiceProvider();
        var factory = sp.GetRequiredService<IEntityFactory>();

        factory.GetType().Name.Should().Be("GeneratedEntityFactory", because: "The generated Factory should be registered as IEntityFactory");

        // Now see if the generated factory actually creates the correct types for these entities
        var mediapPlayer = factory.CreateEntity(haMock.Object, "media_player.home");
        mediapPlayer.GetType().Name.Should().Be("MediaPlayerEntity");

        var sunRiseSensor = factory.CreateEntity(haMock.Object, "sensor.sun_next_rising");
        sunRiseSensor.GetType().Name.Should().Be("SensorEntity");

        var dailyGasSensor = factory.CreateEntity(haMock.Object, "sensor.daily_gas");
        dailyGasSensor.GetType().Name.Should().Be("NumericSensorEntity");
    }

    [Fact]
    public void GeneratedEntityFactoryTestWithApp()
    {
        var states = new HassState[] {
            new() { EntityId = "media_player.my_media" },
            new() { EntityId = "light.patio" },
        };

        var serviceMetaData = Array.Empty<HassServiceDomain>();

        // In this Test we are actually going to run the generated code with a simple dynamic compiled TestApp
        // We do the asserts INSIDE the dynamic code
        var appCode = CSharpCode("""
              using NetDaemon.HassModel;
              using NetDaemon.HassModel.Entities;
              using NetDaemon.HassModel.Tests.CodeGenerator;
              using HomeAssistantGenerated;
              using System.Linq;
              using FluentAssertions;

              [NetDaemonTestApp]
              public class TestApp
              {
                  public TestApp(IHaContext ha)
                  {
                      // The entities created by the HaContext should be of the correct generated runtime type
                      ha.Entity("media_player.my_media").Should().BeOfType<MediaPlayerEntity>();;
                      ha.Entity("light.patio").Should().BeOfType<LightEntity>();
                  }
              }
              """);

        var code = CodeGenTestHelper.GenerateCompilationUnit(_settings, states, serviceMetaData);

        var serviceCollection = new ServiceCollection();
        AddMockServices(serviceCollection);

        // Now run the app
        var app = CodeGenTestHelper.RunApp(code.ToString(), appCode, serviceCollection, _assemblyLoadContext);
        app.Should().NotBeNull();
    }

    private static string CSharpCode([StringSyntax("C#")]string input) => input;

    private static void AddMockServices(ServiceCollection serviceCollection)
    {
        serviceCollection.AddLogging();

        var hassConnectionMock = new Mock<IHomeAssistantConnection>();
        serviceCollection.AddSingleton(hassConnectionMock.Object);
        var haRunnerMock = new Mock<IHomeAssistantRunner>();
        haRunnerMock.SetupGet(n => n.CurrentConnection).Returns(hassConnectionMock.Object);

        serviceCollection.AddSingleton(_ => haRunnerMock.Object);
        var apiManagerMock = new Mock<IHomeAssistantApiManager>();

        serviceCollection.AddSingleton(_ => apiManagerMock.Object);
        serviceCollection.AddScopedHaContext();
    }

    public void Dispose()
    {
        _assemblyLoadContext.Unload();
    }
}
