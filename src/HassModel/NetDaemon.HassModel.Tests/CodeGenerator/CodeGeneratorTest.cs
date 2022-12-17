using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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
        var code = GenerateCompilationUnit(_settings, Array.Empty<HassState>(), new HassServiceDomain[0]);

        code.DescendantNodes().OfType<FileScopedNamespaceDeclarationSyntax>().First().Name.ToString().Should().Be("RootNameSpace");
            
        AssertCodeCompiles(code.ToString(), string.Empty);
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

        var generatedCode = GenerateCompilationUnit(_settings, entityStates, Array.Empty<HassServiceDomain>());
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
                            }
                        }
                        """;
        AssertCodeCompiles(generatedCode.ToString(), appCode);
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
        };

        var generatedCode = GenerateCompilationUnit(_settings, entityStates, Array.Empty<HassServiceDomain>());
        var appCode = """
                        using NetDaemon.HassModel.Entities;
                        using NetDaemon.HassModel;
                        using RootNameSpace;

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
                             }
                        }
                        """;
        AssertCodeCompiles(generatedCode.ToString(), appCode);
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
                            Entity = new() { Domain = "number" }
                        },
                        Fields = new HassServiceField[] {
                            new() { Field = "value", Selector = new NumberSelector(), },
                        },
                    }
                }
            }
        };
            
        var generatedCode = GenerateCompilationUnit(_settings, entityStates, hassServiceDomains);
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
        AssertCodeCompiles(generatedCode.ToString(), appCode);
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
            
        var generatedCode = GenerateCompilationUnit(_settings with { UseAttributeBaseClasses = false }, entityStates, Array.Empty<HassServiceDomain>()).ToString();

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
        AssertCodeCompiles(generatedCode, appCode);
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
            
        var generatedCode = GenerateCompilationUnit(_settings with { UseAttributeBaseClasses = true }, entityStates, Array.Empty<HassServiceDomain>()).ToString();
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
                            int? brightness = light1.Attributes?.Brightness;

                            // check it can be Assigned to LightAttributesBase
                            LightAttributesBase? baseAttr = light1.Attributes;
                            int? brightnessBase = baseAttr?.Brightness;
                        }
                    }
                    """;
        AssertCodeCompiles(generatedCode, appCode);
    }
    
    [Fact]
    public void TestServicesGeneration()
    {
        var readOnlyCollection = new HassState[]
        {
            new() { EntityId = "light.light1" },
        };

        var hassServiceDomains = new HassServiceDomain[]
        {
            new()
            {
                Domain = "light",
                Services = new HassService[] {
                    new() {
                        Service = "turn_off",
                        Target = new TargetSelector { Entity = new() { Domain = "light" } }
                    },
                    new() {
                        Service = "turn_on",
                        Fields = new HassServiceField[] {
                            new() { Field = "transition", Selector = new NumberSelector(), },
                            new() { Field = "brightness", Selector = new NumberSelector { Step = 0.2f }, }
                        },
                        Target = new TargetSelector { Entity = new() { Domain = "light" } }
                    }
                }
            }
        };

        // Act:
        var code = GenerateCompilationUnit(_settings, readOnlyCollection, hassServiceDomains);

        var appCode = """
                    using NetDaemon.HassModel;
                    using NetDaemon.HassModel.Entities;
                    using RootNameSpace;

                    public class Root
                    {
                        public void Run(IHaContext ha)
                        {
                            var s = new RootNameSpace.Services(ha);

                            s.Light.TurnOn(new ServiceTarget() );
                            s.Light.TurnOn(new ServiceTarget(), transition: 12, brightness: 324.5f);
                            s.Light.TurnOn(new ServiceTarget(), new (){ Transition = 12L, Brightness = 12.3f });
                            s.Light.TurnOn(new ServiceTarget(), new (){ Brightness = 12.3f });

                            s.Light.TurnOff(new ServiceTarget());

                            var light = new RootNameSpace.LightEntity(ha, "light.testLight");

                            light.TurnOn();
                            light.TurnOn(transition: 12, brightness: 324.5f);
                            light.TurnOn(new (){ Transition = 12L, Brightness = 12.3f });
                            light.TurnOff();
                        }
                    }
                    """;
        AssertCodeCompiles(code.ToString(), appCode);
    }

    private void AssertCodeCompiles(string generated,  string appCode)
    {
        var syntaxtrees = new []
        {
            SyntaxFactory.ParseSyntaxTree(generated, path: "generated.cs"),
            SyntaxFactory.ParseSyntaxTree(appCode, path: "appcode.cs")
        };
            
        var compilation = CSharpCompilation.Create("tempAssembly",
            syntaxtrees,
            AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic).Select(a => MetadataReference.CreateFromFile(a.Location)).ToArray(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable)
        );

        var emitResult = compilation.Emit(Stream.Null);

        var warningsAndErrors = emitResult.Diagnostics
            .Where(d => d.Severity is DiagnosticSeverity.Error or DiagnosticSeverity.Warning).ToList();
        
        if (warningsAndErrors.Any())
        {
            var msg = new StringBuilder("Compile of generated code failed.\r\n");
            foreach (var diagnostic in warningsAndErrors)
            {
                msg.AppendLine(diagnostic.ToString());
            }

            msg.AppendLine();
            msg.AppendLine("generated.cs");
            // output the generated code including line numbers to help debugging 
            msg.AppendLine(string.Join(Environment.NewLine, generated.Split('\n').Select((l, i) => $"{i+1,4}: {l}")));
            
            Assert.Fail(msg.ToString());
        }
    }
    
    private static CompilationUnitSyntax GenerateCompilationUnit(
        CodeGenerationSettings codeGenerationSettings, 
        IReadOnlyCollection<HassState> states, 
        IReadOnlyCollection<HassServiceDomain> services)
    {
        var metaData = EntityMetaDataGenerator.GetEntityDomainMetaData(states);
        metaData = EntityMetaDataMerger.Merge(codeGenerationSettings, new EntitiesMetaData(), metaData);
        var generatedTypes = Generator.GenerateTypes(metaData.Domains, services).ToArray();
        return Generator.BuildCompilationUnit(codeGenerationSettings.Namespace, generatedTypes);
    }
}
