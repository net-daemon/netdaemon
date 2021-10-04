using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using JoySoftware.HomeAssistant.Model;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NetDaemon.Common;
using NetDaemon.Model3.CodeGenerator;
using NetDaemon.Service.App.CodeGeneration;
using Xunit;

namespace NetDaemon.Model3.Tests.CodeGenerator
{
    public class CodeGeneratorTest
    {
        [Fact]
        public void RunCodeGenEMpy()
        {
            var generated = Generator.CreateCompilationUnitSyntax("RootNameSpace", new EntityState[0], new HassServiceDomain[0]);

            generated.DescendantNodes().OfType<NamespaceDeclarationSyntax>().First().Name.ToString().Should().Be("RootNameSpace");
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

            var code = Generator.CreateCompilationUnitSyntax("RootNameSpace", entityStates, Array.Empty<HassServiceDomain>());

            var entitiesInterface = FindTypeDeclaration<InterfaceDeclarationSyntax>(code, "IEntities");

            var props = entitiesInterface.Members.OfType<PropertyDeclarationSyntax>().Select(p => p.Identifier.Text).ToList();
            props.Should().BeEquivalentTo("Light", "Switch");

            var lightEntitiesClass = code.DescendantNodes().OfType<ClassDeclarationSyntax>().FirstOrDefault(i => i.Identifier.Text == "LightEntities");
            lightEntitiesClass.Members.OfType<PropertyDeclarationSyntax>().Select(p => p.Identifier.Text).Should().BeEquivalentTo("Light1", "Light2");

            FindTypeDeclaration<RecordDeclarationSyntax>(code, "LightEntity").Should().NotBeNull();
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
            
            var code = Generator.CreateCompilationUnitSyntax("RootNameSpace", entityStates, Array.Empty<HassServiceDomain>());

            var lightAttributesRecord = FindTypeDeclaration<RecordDeclarationSyntax>(code, "LightAttributes");
            var props = lightAttributesRecord.Members.OfType<PropertyDeclarationSyntax>();
            props.Should().NotBeNull();
            props.First(p => p.Identifier.ToString() == "Brightness").Type.ToString().Should().Be("int");
            props.First(p => p.Identifier.ToString() == "FriendlyName").Type.ToString().Should().Be("string");
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
                        new() { Service = "turn_off" },
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
                                    Field = "brightnes",
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

            // Assert:
            var lightTurnOnParametersRecord = FindTypeDeclaration<RecordDeclarationSyntax>(code, "LightTurnOnParameters");

            lightTurnOnParametersRecord.Should().NotBeNull();
            var props = lightTurnOnParametersRecord.Members.OfType<PropertyDeclarationSyntax>().ToList();
            
            props.Should().HaveCount(2);
            props.Single(p => p.Identifier.ToString() == "Transition").Type.ToString().Should().Be("long");
            props.Single(p => p.Identifier.ToString() == "Brightness").Type.ToString().Should().Be("double");
        }

        private T FindTypeDeclaration<T>(CompilationUnitSyntax root, string name) where T : TypeDeclarationSyntax
        {
            return root.DescendantNodes().OfType<T>().FirstOrDefault(n => n.Identifier.Text == name);
        }
    }
}