using System.Collections.Generic;
using System.Text.Json;
using NetDaemon.Client.HomeAssistant.Model;
using NetDaemon.HassModel.CodeGenerator;
using NetDaemon.HassModel.CodeGenerator.Model;

namespace NetDaemon.HassModel.Tests.CodeGenerator;

public class ServicesGeneratorTest
{
    private readonly CodeGenerationSettings _settings = new() { Namespace = "RootNameSpace" };

    [Fact]
    public void TestServicesGeneration()
    {
        var readOnlyCollection = new HassState[] {
            new() { EntityId = "light.light1" },
        };

        var hassServiceDomains = new HassServiceDomain[] {
            new() {
                Domain = "light",
                Services = new HassService[] {
                    new() {
                        Service = "turn_off",
                        Target = new TargetSelector
                        {
                            Entity = new[] { new EntitySelector { Domain = new[] { "light" } } }
                        }

                    },
                    new() {
                        Service = "turn_on",
                        Fields = new HassServiceField[] {
                            new() { Field = "transition", Selector = new NumberSelector(), },
                            new() { Field = "brightness", Selector = new NumberSelector { Step = 0.2d }, }
                        },
                        Target = new TargetSelector
                        {
                            Entity = new[] { new EntitySelector { Domain = new[] { "light" } } }
                        }
                    }
                }
            }
        };

        // Act:
        var code = CodeGenTestHelper.GenerateCompilationUnit(_settings, readOnlyCollection, hassServiceDomains);

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

                            LightEntity light = new RootNameSpace.LightEntity(ha, "light.testLight");
                            light.TurnOn();
                            light.TurnOn(transition: 12, brightness: 324.5f);
                            light.TurnOn(new (){ Transition = 12L, Brightness = 12.3f });
                            light.TurnOff();

                            ILightEntityCore lightCore = light;
                            lightCore.TurnOn();
                            lightCore.TurnOn(transition: 12, brightness: 324.5f);
                            lightCore.TurnOn(new (){ Transition = 12L, Brightness = 12.3f });
                            lightCore.TurnOff();
                        }
                    }
                    """;

        CodeGenTestHelper.AssertCodeCompiles(code.ToString(), appCode);
    }

    [Fact]
    public void TestServiceWithoutAnyTargetEntity_ExtensionMethodSkipped()
    {
        var readOnlyCollection = new HassState[] {
            new() { EntityId = "orbiter.cassini" },
        };

        var hassServiceDomains = new HassServiceDomain[]
        {
            new() {
                Domain = "smart_things",
                Services = new HassService[] {
                    new() {
                        Service = "dig",
                        Target = new TargetSelector
                        {
                            Entity = new[] { new EntitySelector { Domain = new[] { "humidifiers" } } }
                        },
                    },
                    new() {
                        Service = "orbit",
                        Target = new TargetSelector
                        {
                            Entity = new[] { new EntitySelector { Domain = new[] { "orbiter" } } }
                        },
                    }
                },
            }
        };

        // Act:
        var code = CodeGenTestHelper.GenerateCompilationUnit(_settings, readOnlyCollection, hassServiceDomains);

        code.ToString().Should().NotContain("rover", because:"There is no entity for domain rover");

        var appCode = """
                    using NetDaemon.HassModel;
                    using NetDaemon.HassModel.Entities;
                    using RootNameSpace;

                    public class Root
                    {
                        public void Run(Entities entities, Services services)
                        {
                            // Test the Orbit extension Method exists
                            SmartThingsEntityExtensionMethods.Orbit(entities.Orbiter.Cassini);
                            entities.Orbiter.Cassini.Orbit();

                            // Test the Methods on the service classes do exist
                            services.SmartThings.Dig(new ServiceTarget());
                            services.SmartThings.Orbit(new ServiceTarget());
                        }
                    }
                    """;

        CodeGenTestHelper.AssertCodeCompiles(code.ToString(), appCode);
    }

    [Fact]
    public void TestServiceWithoutAnyMethods_ClassSkipped()
    {
        var readOnlyCollection = new HassState[] {
            new() { EntityId = "light.light1" },
        };

        var hassServiceDomains = new HassServiceDomain[] {
            new() {
                Domain = "dumbthings",
                Services = new HassService[] {
                    new() {
                        Service = "push_button",
                        Target = new TargetSelector
                        {
                            Entity = new[] { new EntitySelector { Domain = new[] { "uselessbox" } } }
                        },
                    },
                },
            }
        };

        // Act:
        var code = CodeGenTestHelper.GenerateCompilationUnit(_settings, readOnlyCollection, hassServiceDomains);
        code.ToString().Should().NotContain("DumbthingsEntityExtensionMethods",
            because:"There is no entity for any of the services in dumbthings");

        var appCode = """
                    using NetDaemon.HassModel;
                    using NetDaemon.HassModel.Entities;
                    using RootNameSpace;

                    public class Root
                    {
                        public void Run(Entities entities, Services services)
                        {
                            // But the Method on the Dumbthings service class should still exist
                            services.Dumbthings.PushButton(new ServiceTarget());
                        }
                    }
                    """;

        CodeGenTestHelper.AssertCodeCompiles(code.ToString(), appCode);
    }

    [Fact]
    public void TestServiceWithKeyWordFieldName_ParamEscaped()
    {
        var readOnlyCollection = new HassState[] {
            new() { EntityId = "light.light1" },
        };

        var hassServiceDomains = new HassServiceDomain[] {
            new() {
                Domain = "light",
                Services = new HassService[] {
                    new() {
                        Service = "set_value",
                        Target = new TargetSelector {
                            Entity = new[] { new EntitySelector { Domain = new[] { "light" } } }
                        },
                        Fields = new HassServiceField[] {
                            new() { Field = "class", Selector = new NumberSelector(), },
                        },
                    }
                }
            }
        };

        // Act:
        var code = CodeGenTestHelper.GenerateCompilationUnit(_settings, readOnlyCollection, hassServiceDomains);

        var appCode = """
                    using NetDaemon.HassModel;
                    using NetDaemon.HassModel.Entities;
                    using RootNameSpace;

                    public class Root
                    {
                        public void Run(Entities entities, Services services)
                        {
                            entities.Light.Light1.SetValue(@class:2);
                            services.Light.SetValue(new ServiceTarget(), @class:2);
                        }
                    }
                    """;

        CodeGenTestHelper.AssertCodeCompiles(code.ToString(), appCode);
    }

    [Fact]
    public void MultpileEntitySelector_ShouldGenerateArray()
    {
        var states = new HassState[] {
            new() { EntityId = "media_player.group1" },
        };

        var serviceMetaData = """
                    {
                      "media_player": {
                        "join": {
                          "name": "Join",
                          "description": "Group players together. Only works on platforms with support for player groups.",
                          "fields": {
                            "group_members": {
                              "name": "Group members",
                              "description": "The players which will be synced with the target player.",
                              "required": true,
                              "example": "- media_player.multiroom_player2\n- media_player.multiroom_player3\n",
                              "selector": {
                                "entity": {
                                  "multiple": true,
                                  "domain": "media_player"
                                }
                              }
                            }
                          },
                          "target": {
                            "entity": {
                              "domain": "media_player"
                            }
                          }
                        }
                     }
                    }
                    """;

        var appCode = """
                    using NetDaemon.HassModel;
                    using NetDaemon.HassModel.Entities;
                    using RootNameSpace;

                    public class Root
                    {
                        public void Run(Entities entities, Services services)
                        {
                            entities.MediaPlayer.Group1.Join(groupMembers: new string [] {"media_player.multiroom_player1", "media_player.multiroom_player2"});
                            services.MediaPlayer.Join(new ServiceTarget(), groupMembers: new string [] {"media_player.multiroom_player1", "media_player.multiroom_player2"});
                        }
                    }
                    """;

        var hassServiceDomains = Parse(serviceMetaData);

// Act:
        var code = CodeGenTestHelper.GenerateCompilationUnit(_settings, states, hassServiceDomains);

        CodeGenTestHelper.AssertCodeCompiles(code.ToString(), appCode);
    }

    private static IReadOnlyCollection<HassServiceDomain> Parse(string sample)
    {
        var element = JsonDocument.Parse(sample).RootElement;
        var result = ServiceMetaDataParser.Parse(element, out var errors);
        errors.Should().BeEmpty();
        return result;
    }
}
