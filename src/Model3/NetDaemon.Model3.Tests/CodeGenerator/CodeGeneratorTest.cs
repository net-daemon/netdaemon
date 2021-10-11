using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using JoySoftware.HomeAssistant.Model;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NetDaemon.Common;
using NetDaemon.Model3.CodeGenerator;
using Xunit;

namespace NetDaemon.Model3.Tests.CodeGenerator
{
    public class CodeGeneratorTest
    {
        [Fact]
        public void RunCodeGenEMpy()
        {
            var code = Generator.CreateCompilationUnitSyntax("RootNameSpace", new EntityState[0], new HassServiceDomain[0]);

            code.DescendantNodes().OfType<NamespaceDeclarationSyntax>().First().Name.ToString().Should().Be("RootNameSpace");
            
            AssertCodeCompiles(code.ToString());
        }

        [Fact]
        public void TestIEntityGeneration()
        {
            var entityStates = new EntityState[]
            {
                new() { EntityId = "light.light1" },
                new() { EntityId = "light.light2" },
                new() { EntityId = "switch.switch1" },
                new() { EntityId = "switch.switch2" },
            };

            var generatedCode = Generator.CreateCompilationUnitSyntax("RootNameSpace", entityStates, Array.Empty<HassServiceDomain>());
            var appCode = @"
using NetDaemon.Model3.Entities;
using NetDaemon.Model3.Common;
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
            var entityStates = new EntityState[]
            {
                new()
                {
                    EntityId = "light.light1",
                    Attribute = new Dictionary<string, object>
                    {
                        ["brightness"] = 255,
                        ["friendly_name"] = "attic"
                    }
                },
            };
            
            var generatedCode = Generator.CreateCompilationUnitSyntax("RootNameSpace", entityStates, Array.Empty<HassServiceDomain>());

            var appCode = @"
using NetDaemon.Model3.Entities;
using NetDaemon.Model3.Common;
using RootNameSpace;

public class Root
{
    public void Run(IHaContext ha)
    {
        IEntities entities = new Entities(ha);
        LightEntity light1 = entities.Light.Light1;
        long brightness = light1.Attributes.Brightness;
        string friendlyName = light1.Attributes.FriendlyName;
    }
}";
            AssertCodeCompiles(generatedCode.ToString(), appCode);
        }

        [Fact]
        public void TestServicesGeneration()
        {
            var readOnlyCollection = new EntityState[]
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
                                    Selector = new NumberSelector() { },
                                },
                                new()
                                {
                                    Field = "brightness",
                                    Selector = new NumberSelector() { Step = 0.2f },
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
using NetDaemon.Model3.Common;
using NetDaemon.Model3.Entities;
using RootNameSpace;

public class Root
{
    public void Run(IHaContext ha)
    {
        var s = new RootNameSpace.Services(ha);

        s.Light.TurnOn(new ServiceTarget() );
        s.Light.TurnOn(new ServiceTarget(), transition: 12, brightness: 324.5f);
        s.Light.TurnOn(new ServiceTarget(), new (){ Transition = 12l, Brightness = 12.3f });
        s.Light.TurnOn(new ServiceTarget(), new (){ Brightness = 12.3f });

        s.Light.TurnOff(new ServiceTarget());

        var light = new RootNameSpace.LightEntity(null, ""light.testlight"");

        light.TurnOn();
        light.TurnOn(transition: 12, brightness: 324.5f);
        light.TurnOn(new (){ Transition = 12l, Brightness = 12.3f });
        light.TurnOff();
    }
}";
            AssertCodeCompiles(code.ToString(), appCode);
        }

        private void AssertCodeCompiles(params string[] code)
        {
            var syntaxtrees = code.Select(s => SyntaxFactory.ParseSyntaxTree(s)).ToArray();
            
            var compilation = CSharpCompilation.Create("tempAssembly",
                syntaxtrees,
                AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic).Select(a => MetadataReference.CreateFromFile(a.Location)).ToArray(),
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            );

            var emitResult = compilation.Emit(Stream.Null);

            emitResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Should().BeEmpty();
            
        }
    }
}