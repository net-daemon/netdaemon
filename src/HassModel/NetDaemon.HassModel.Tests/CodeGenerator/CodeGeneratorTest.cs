using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using JoySoftware.HomeAssistant.Model;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NetDaemon.HassModel.CodeGenerator;
using NetDaemon.HassModel.Tests.TestHelpers;
using Xunit;

namespace NetDaemon.HassModel.Tests.CodeGenerator
{
    public class CodeGeneratorTest
    {
        [Fact]
        public void RunCodeGenEMpy()
        {
            var code = Generator.CreateCompilationUnitSyntax("RootNameSpace", Array.Empty<HassState>(), new HassServiceDomain[0]);

            code.DescendantNodes().OfType<NamespaceDeclarationSyntax>().First().Name.ToString().Should().Be("RootNameSpace");
            
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

            var generatedCode = Generator.CreateCompilationUnitSyntax("RootNameSpace", entityStates, Array.Empty<HassServiceDomain>());
            var appCode = @"
using NetDaemon.HassModel.Entities;
using NetDaemon.HassModel.Common;
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
}";
            AssertCodeCompiles(generatedCode.ToString(), appCode);
        }

        [Fact]
        public void TestAttributeClassGeneration()
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
                        not_used = (string)null,
                        trueValue = true,
                        falseValue = false,
                        dict = new {},
                        arr = new []{"red", "blue"}
                    }.AsJsonElement()
                },
            };
            
            var generatedCode = Generator.GenerateCode("RootNameSpace", entityStates, Array.Empty<HassServiceDomain>());

            var appCode = @"
using NetDaemon.HassModel.Entities;
using NetDaemon.HassModel.Common;
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
    }
}";
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
                    Services = new HassService[]
                    {
                        new()
                        {
                            Service = "turn_off",
                            Target = new TargetSelector()
                            {
                                Entity = new() { Domain = "light" }
                            }
                        },
                        new()
                        {
                            Service = "turn_on",
                            Fields = new HassServiceField[]
                            {
                                new()
                                {
                                    Field = "transition",
                                    Selector = new NumberSelector(),
                                },
                                new()
                                {
                                    Field = "brightness",
                                    Selector = new NumberSelector { Step = 0.2f },
                                }
                            },
                            Target = new TargetSelector()
                            {
                                Entity = new() { Domain = "light" }
                            }
                        }
                    }
                }
            };

            // Act:
            var code = Generator.CreateCompilationUnitSyntax("RootNameSpace", readOnlyCollection, hassServiceDomains);
            // uncomment for debugging
            // File.WriteAllText(@"c:\temp\generated.cs", code.ToString());

            // Assert:

            var appCode = @"
using NetDaemon.HassModel.Common;
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

        var light = new RootNameSpace.LightEntity(ha, ""light.testLight"");

        light.TurnOn();
        light.TurnOn(transition: 12, brightness: 324.5f);
        light.TurnOn(new (){ Transition = 12L, Brightness = 12.3f });
        light.TurnOff();
    }
}";
            AssertCodeCompiles(code.ToString(), appCode);
        }

        private void AssertCodeCompiles(string generated, string appCode)
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

            emitResult.Diagnostics
                .Where(d => d.Severity is DiagnosticSeverity.Error or DiagnosticSeverity.Warning)
                .Should().BeEmpty();
            
        }
    }
}