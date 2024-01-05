using Microsoft.CodeAnalysis.CSharp.Syntax;
using NetDaemon.Client.HomeAssistant.Model;
using NetDaemon.HassModel.CodeGenerator;
using NetDaemon.HassModel.CodeGenerator.Model;
using NetDaemon.HassModel.Tests.TestHelpers;

namespace NetDaemon.HassModel.Tests.CodeGenerator;

public class CodeGeneratorTest
{
    private readonly CodeGenerationSettings _settings = new() { Namespace = "RootNameSpace" };

    [Fact]
    public void RunCodeGenEmpy()
    {
        var code = CodeGenTestHelper.GenerateCompilationUnit(_settings, Array.Empty<HassState>(), Array.Empty<HassServiceDomain>());

        code.DescendantNodes().OfType<FileScopedNamespaceDeclarationSyntax>().First().Name.ToString().Should().Be("RootNameSpace");

        CodeGenTestHelper.AssertCodeCompiles(code.ToString(), string.Empty);
    }

    [Fact]
    public void TestIEntityGeneration()
    {
        var entityStates = new HassState[]
        {
            new() { EntityId = "light.light1" },
            new() { EntityId = "light.light2" },
            new() { EntityId = "switch.switch1" },
            new() { EntityId = "switch.switch2" },
        };

        var generatedCode = CodeGenTestHelper.GenerateCompilationUnit(_settings, entityStates, Array.Empty<HassServiceDomain>());
        var appCode = """
                        using NetDaemon.HassModel.Entities;
                        using NetDaemon.HassModel;
                        using RootNameSpace;

                        public class Root
                        {
                            public void Run(IHaContext ha)
                            {
                                IEntities entities = new Entities(ha);
                                LightEntity light1 = entities.Light.Light1;
                                LightEntity light2 = entities.Light.Light2;
                                SwitchEntity switch1 = entities.Switch.Switch1;
                                SwitchEntity switch2 = entities.Switch.Switch2;

                                // Now check if the entity classes implement the core interfaces
                                ILightEntityCore lightAsCore = entities.Light.Light1;
                                ISwitchEntityCore switch1AsCore = entities.Switch.Switch1;
                            }
                        }
                        """;
        CodeGenTestHelper.AssertCodeCompiles(generatedCode.ToString(), appCode);
    }

    [Fact]
    public void TestEntityDuplictateNormalizedName()
    {
        var entityStates = new HassState[]
        {
            new() { EntityId = "light.light_1_1" },
            new() { EntityId = "light.light_11" },
        };

        var generatedCode = CodeGenTestHelper.GenerateCompilationUnit(_settings, entityStates, Array.Empty<HassServiceDomain>());
        var appCode = """
                        using NetDaemon.HassModel.Entities;
                        using NetDaemon.HassModel;
                        using RootNameSpace;

                        public class Root
                        {
                            public void Run(Entities entities)
                            {
                                LightEntity l1_1 = entities.Light.light_1_1;
                                LightEntity l11 = entities.Light.light_11;
                            }
                        }
                        """;
        CodeGenTestHelper.AssertCodeCompiles(generatedCode.ToString(), appCode);
    }

    [Fact]
    public void TestEntityInvalidCSharpName()
    {
        var entityStates = new HassState[]
        {
            new() { EntityId = "light.1light" },
            new() { EntityId = "light.li@#ght" },
        };

        var generatedCode = CodeGenTestHelper.GenerateCompilationUnit(_settings, entityStates, Array.Empty<HassServiceDomain>());
        var appCode = """
                        using NetDaemon.HassModel.Entities;
                        using NetDaemon.HassModel;
                        using RootNameSpace;

                        public class Root
                        {
                            public void Run(Entities entities)
                            {
                                LightEntity l1 = entities.Light._1light;
                                LightEntity l2 = entities.Light.Light;
                            }
                        }
                        """;
        CodeGenTestHelper.AssertCodeCompiles(generatedCode.ToString(), appCode);
    }

    [Fact]
    public void TestNumericSensorEntityGeneration()
    {
        // Numeric entities should be generated for input_numbers and sensors with a unit_of_measurement attribute
        var entityStates = new HassState[]
        {
            new() {
                EntityId = "number.living_bass",
                AttributesJson = new { unit_of_measurement = "%", }.AsJsonElement()
            },
            new()
            {
                EntityId = "input_number.target_temperature",
                AttributesJson = new { unit_of_measurement = "Kwh", }.AsJsonElement()
            },
            new()
            {
                EntityId = "sensor.daily_power_consumption",
                AttributesJson = new { unit_of_measurement = "Kwh", }.AsJsonElement()
            },
            new()
            {
                EntityId = "sensor.Pir",
            },
            new()
            {
                EntityId = "proximity.home",
            },
        };

        var generatedCode = CodeGenTestHelper.GenerateCompilationUnit(_settings, entityStates, Array.Empty<HassServiceDomain>());
        var appCode = """
                        using NetDaemon.HassModel.Entities;
                        using NetDaemon.HassModel;
                        using RootNameSpace;
                        using System.Collections.Generic;

                        public class Root
                        {
                            public void Run(IHaContext ha)
                            {
                                IEntities entities = new Entities(ha);
                                NumberEntity livingBass  = entities.Number.LivingBass;
                                double? bass = livingBass.State;

                                InputNumberEntity targetTempEntity = entities.InputNumber.TargetTemperature;
                                double? targetTempValue = targetTempEntity.State;

                                NumericSensorEntity dailyPower = entities.Sensor.DailyPowerConsumption;
                                double? cost = dailyPower.State;

                                SensorEntity pirSensor = entities.Sensor.Pir;
                                string? pir = pirSensor.State;

                                ProximityEntity homeProximity = entities.Proximity.Home;
                                double? distance = homeProximity.State;

                                IEnumerable<SensorEntity> allSensors = entities.Sensor.EnumerateAll();
                                IEnumerable<SensorEntity> nonNumericSensors = entities.Sensor.EnumerateAllNonNumeric();
                                IEnumerable<NumericSensorEntity> numericSensors = entities.Sensor.EnumerateAllNumeric();
                             }
                        }
                        """;
        CodeGenTestHelper.AssertCodeCompiles(generatedCode.ToString(), appCode);
    }

    [Fact]
    public void TestNumberExtensionMethodGeneration()
    {
        var entityStates = new HassState[] {
            new() {
                EntityId = "number.living_bass",
                AttributesJson = new { unit_of_measurement = "%", }.AsJsonElement()
            },
            new() {
                EntityId = "unknown.number",
                AttributesJson = new { unit_of_measurement = "pcs", }.AsJsonElement()
            },
            new() {
                EntityId = "unknown.string",
            },
        };

        var hassServiceDomains = new HassServiceDomain[] {
            new() {
                Domain = "number",
                Services = new HassService[] {
                    new() {
                        Service = "set_value",
                        Target = new TargetSelector {
                            Entity = new[] { new EntitySelector { Domain = new[] { "number" } } }
                        },
                        Fields = new HassServiceField[] {
                            new() { Field = "value", Selector = new NumberSelector(), },
                        },
                    }
                }
            }
        };

        var generatedCode = CodeGenTestHelper.GenerateCompilationUnit(_settings, entityStates, hassServiceDomains);
        var appCode = """
                        using NetDaemon.HassModel.Entities;
                        using NetDaemon.HassModel;
                        using RootNameSpace;

                        public class Root
                        {
                            public void Run(IHaContext ha)
                            {
                                IEntities entities = new Entities(ha);

                            entities.Number.LivingBass.SetValue(12);
                             }
                        }
                        """;
        CodeGenTestHelper.AssertCodeCompiles(generatedCode.ToString(), appCode);
    }

    [Fact]
    public void TestAttributeClassGeneration_UseAttributeBaseClassesFalse()
    {
        var entityStates = new HassState[]
        {
            new()
            {
                EntityId = "light.light1",
                AttributesJson = new
                {
                    brightness = 255L,
                    friendly_name = "attic",
                    FriendlyName = "attic",
                    start_date = new DateTime(2010, 12, 23, 23, 12, 00),
                    not_used = (string?)null,
                    trueValue = true,
                    falseValue = false,
                    dict = new {},
                    arr = new []{"red", "blue"}
                }.AsJsonElement()
            },
        };

        var generatedCode = CodeGenTestHelper.GenerateCompilationUnit(_settings with { UseAttributeBaseClasses = false }, entityStates, Array.Empty<HassServiceDomain>()).ToString();

        var appCode = """
                    using NetDaemon.HassModel.Entities;
                    using NetDaemon.HassModel;
                    using RootNameSpace;

                    public class Root
                    {
                        public void Run(IHaContext ha)
                        {
                            IEntities entities = new Entities(ha);
                            LightEntity light1 = entities.Light.Light1;
                            double? brightnessNullable = light1.Attributes?.Brightness;
                            double brightness = light1.Attributes?.Brightness ?? 0L;
                            string? friendlyName0 = light1.Attributes?.FriendlyName_0;
                            string? friendlyName1 = light1.Attributes?.FriendlyName_1;
                            string? startDate = light1.Attributes?.StartDate;
                            string? arrElement1 = light1.Attributes?.Arr?[0];
                        }
                    }
                    """;
        CodeGenTestHelper.AssertCodeCompiles(generatedCode, appCode);
    }


    [Fact]
    public void TestAttributeClassGenerationSkipBaseProperties()
    {
        var entityStates = new HassState[]
        {
            new()
            {
                EntityId = "light.light1",
                AttributesJson = new
                {
                    brightness = 30,
                    start_date = new DateTime(2010, 12, 23, 23, 12, 00),
                    not_used = (string?)null,
                    trueValue = true,
                    falseValue = false,
                    dict = new {},
                    arr = new []{"red", "blue"}
                }.AsJsonElement()
            },
        };

        var generatedCode = CodeGenTestHelper.GenerateCompilationUnit(_settings with { UseAttributeBaseClasses = true }, entityStates, Array.Empty<HassServiceDomain>()).ToString();
        generatedCode.Should().NotContain("Brightness", because: "It is in the base class");

        var appCode = """
                    using NetDaemon.HassModel.Entities;
                    using NetDaemon.HassModel.Entities.Core;
                    using NetDaemon.HassModel;
                    using RootNameSpace;

                    public class Root
                    {
                        public void Run(IHaContext ha)
                        {
                            IEntities entities = new Entities(ha);
                            LightEntity light1 = entities.Light.Light1;
                            double? brightness = light1.Attributes?.Brightness;

                            // check it can be Assigned to LightAttributesBase
                            LightAttributesBase? baseAttr = light1.Attributes;
                            double? brightnessBase = baseAttr?.Brightness;
                        }
                    }
                    """;
        CodeGenTestHelper.AssertCodeCompiles(generatedCode, appCode);
    }

}
