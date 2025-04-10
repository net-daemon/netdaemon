using System.Diagnostics.CodeAnalysis;
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
                            Entity = [new EntitySelector { Domain = ["light"] }]
                        }

                    },
                    new() {
                        Service = "turn_on",
                        Fields = new HassServiceField[] {
                            new() { Field = "transition", Selector = new NumberSelector(), },
                            new() { Field = "brightness", Selector = new NumberSelector { Step = 0.2d }, },
                            new() { Field = "int_field", Selector = new NumberSelector { Step = 10 }, },
                            new() { Field = "rgb_color", Selector = new Selector() { Type = "color_rgb" }, },
                        },
                        Target = new TargetSelector
                        {
                            Entity = [new EntitySelector { Domain = ["light"] }]
                        }
                    }
                }
            }
        };

        // Act:
        var code = CodeGenTestHelper.GenerateCompilationUnit(_settings, readOnlyCollection, hassServiceDomains);

        var appCode = WrapMethodBody(
            """
                services.Light.TurnOn(new ServiceTarget() );
                services.Light.TurnOn(new ServiceTarget(), transition: 12.5d, brightness: 324.5d, rgbColor: [128, 12, 23]);
                services.Light.TurnOn(new ServiceTarget(), new () { Transition = 12.5d, Brightness = 12.3d, IntField = 2, RgbColor = [128, 12, 23] });
                services.Light.TurnOn(new ServiceTarget(), new () { Brightness = 12.3f });

                services.Light.TurnOff(new ServiceTarget());

                LightEntity light = entities.Light.Light1;
                light.TurnOn();
                light.TurnOn(transition: 12, brightness: 324.5f);
                light.TurnOn(new (){ Transition = 12L, Brightness = 12.3f });
                light.TurnOff();

                ILightEntityCore lightCore = light;
                lightCore.TurnOn();
                lightCore.TurnOn(transition: 12, brightness: 324.5f, rgbColor: [128, 12, 23]);
                lightCore.TurnOn(new (){ Transition = 12L, Brightness = 12.3f, RgbColor = [128, 12, 23]});
                lightCore.TurnOff();
            """);

        CodeGenTestHelper.AssertCodeCompiles(code.ToString(), appCode);
    }

    [Fact]
    public void TestServicesWithReturnValueGeneration()
    {
        var readOnlyCollection = new HassState[] {
            new() { EntityId = "weather.mytown" },
        };

        var hassServiceDomains = new HassServiceDomain[] {
            new() {
                Domain = "weather",
                Services = [
                    new() {
                        Service = "get_forecast",
                        Target = new TargetSelector
                        {
                            Entity = [new EntitySelector { Domain = ["weather"] }]
                        },
                        Fields = [
                            new() { Field = "FakeAttribute", Selector = new NumberSelector(), },
                            new() { Field = "AnotherFakeAttribute", Selector = new NumberSelector { Step = 0.2d }, }
                        ],
                        Response = new Response
                        {
                            Optional = true,
                        }

                    }
                ]
            }
        };

        // Act:
        var code = CodeGenTestHelper.GenerateCompilationUnit(_settings, readOnlyCollection, hassServiceDomains);

        var appCode = """
                    using System.Threading.Tasks;
                    using NetDaemon.HassModel;
                    using NetDaemon.HassModel.Entities;
                    using RootNameSpace;

                    public class Root
                    {
                        public async Task Run(IHaContext ha)
                        {
                            var s = new RootNameSpace.Services(ha);

                            var retVal = await s.Weather.GetForecastAsync(new ServiceTarget() );
                            retVal = await s.Weather.GetForecastAsync(new ServiceTarget(), new (){ FakeAttribute = 12L, AnotherFakeAttribute = 12.3f });

                            WeatherEntity weather = new RootNameSpace.WeatherEntity(ha, "weather.mytown");
                            retVal  = await weather.GetForecastAsync();
                            retVal  = await weather.GetForecastAsync(new (){ FakeAttribute = 12L, AnotherFakeAttribute = 12.3f } );
                            retVal  = await weather.CallServiceWithResponseAsync( "get_forecast", new { FakeAttribute = 12L, AnotherFakeAttribute = 12.3f });

                            IWeatherEntityCore weatherCore = weather;
                            retVal  = await weatherCore.GetForecastAsync();
                            retVal  = await weatherCore.GetForecastAsync(new (){ FakeAttribute = 12L, AnotherFakeAttribute = 12.3f } );
                            retVal  = await weatherCore.CallServiceWithResponseAsync( "get_forecast", new { FakeAttribute = 12L, AnotherFakeAttribute = 12.3f });

                        }
                    }
                    """;

        CodeGenTestHelper.AssertCodeCompiles(code.ToString(), appCode);
    }

    [Fact]
    public void TestServicesGenerationWithAndWithoutProvidingDataForServicesWithoutTargetOrFields()
    {
        var readOnlyCollection = new HassState[] {
            new() { EntityId = "script.script1" },
        };

        var hassServiceDomains = new HassServiceDomain[] {
            new() {
                Domain = "script",
                Services = [
                    new() {
                        Service = "turn_off",
                        Target = new TargetSelector
                        {
                            Entity = [new EntitySelector { Domain = ["script"] }]
                        }

                    },
                    new() {
                        Service = "reload",
                    },
                ]
            }
        };

        // Act:
        var code = CodeGenTestHelper.GenerateCompilationUnit(_settings, readOnlyCollection, hassServiceDomains);

        var appCode = WrapMethodBody(
            """
                services.Script.TurnOff(new ServiceTarget());
                services.Script.TurnOff(new ServiceTarget(), new { });

                services.Script.Reload();
                services.Script.Reload(new { });

                ScriptEntity script = entities.Script.Script1;

                script.TurnOff(new { });
                script.TurnOff();

                IScriptEntityCore scriptCore = script;
                scriptCore.TurnOff();
                scriptCore.TurnOff(new { });
            """);

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
                            Entity = [new EntitySelector { Domain = ["humidifiers"] }]
                        },
                    },
                    new() {
                        Service = "orbit",
                        Target = new TargetSelector
                        {
                            Entity = [new EntitySelector { Domain = ["orbiter"] }]
                        },
                    }
                },
            }
        };

        // Act:
        var code = CodeGenTestHelper.GenerateCompilationUnit(_settings, readOnlyCollection, hassServiceDomains);

        code.ToString().Should().NotContain("rover", because:"There is no entity for domain rover");

        var appCode = WrapMethodBody(
            """
                // Test the Orbit extension Method exists
                SmartThingsEntityExtensionMethods.Orbit(entities.Orbiter.Cassini);
                entities.Orbiter.Cassini.Orbit();

                // Test the Methods on the service classes do exist
                services.SmartThings.Dig(new ServiceTarget());
                services.SmartThings.Orbit(new ServiceTarget());
            """);

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
                            Entity = [new EntitySelector { Domain = ["uselessbox"] }]
                        },
                    },
                },
            }
        };

        // Act:
        var code = CodeGenTestHelper.GenerateCompilationUnit(_settings, readOnlyCollection, hassServiceDomains);
        code.ToString().Should().NotContain("DumbthingsEntityExtensionMethods",
            because:"There is no entity for any of the services in dumbthings");

        var appCode = WrapMethodBody(
            """
                    // But the Method on the Dumbthings service class should still exist
                    services.Dumbthings.PushButton(new ServiceTarget());
            """);

        CodeGenTestHelper.AssertCodeCompiles(code.ToString(), appCode);
    }

    [Fact]
    public void TestServiceWithReturnValueWithoutAnyMethods()
    {
        var readOnlyCollection = new HassState[] {
            new() { EntityId = "weather.hometown" },
        };

        var hassServiceDomains = new HassServiceDomain[] {
            new() {
                Domain = "weather",
                Services = [
                    new() {
                        Service = "get_forecast",
                        Target = new TargetSelector
                        {
                            Entity = [new EntitySelector { Domain = ["weather"] }]
                        },
                        Response = new Response
                        {
                            Optional = true,
                        }
                    },
                ],
            }
        };

        // Act:
        var code = CodeGenTestHelper.GenerateCompilationUnit(_settings, readOnlyCollection, hassServiceDomains);

        var appCode = """
                    using System.Threading.Tasks;
                    using NetDaemon.HassModel;
                    using NetDaemon.HassModel.Entities;
                    using RootNameSpace;

                    public class Root
                    {
                        public async Task Run(Entities entities, Services services)
                        {
                            // But the Method on the Dumbthings service class should still exist
                            var x = await services.Weather.GetForecastAsync(new ServiceTarget());
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
                            Entity = [new EntitySelector { Domain = ["light"] }]
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

        var appCode = WrapMethodBody(
            """
                entities.Light.Light1.SetValue(@class:2);
                services.Light.SetValue(new ServiceTarget(), @class:2);
            """);

        CodeGenTestHelper.AssertCodeCompiles(code.ToString(), appCode);
    }

    [Fact]
    public void MultpileEntitySelector_ShouldGenerateArray()
    {
        var states = new HassState[] { new() { EntityId = "media_player.group1" } };

        var hassServiceDomains = Parse("""
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
                                       """);

        var appCode = WrapMethodBody(
            """
                entities.MediaPlayer.Group1.Join(groupMembers: new string [] {"media_player.multiroom_player1", "media_player.multiroom_player2"});
                services.MediaPlayer.Join(new ServiceTarget(), groupMembers: new string [] {"media_player.multiroom_player1", "media_player.multiroom_player2"});
            """);


// Act:
        var code = CodeGenTestHelper.GenerateCompilationUnit(_settings, states, hassServiceDomains);
        CodeGenTestHelper.AssertCodeCompiles(code.ToString(), appCode);
    }


        [Fact]
    public void MultipleTextSelector_ShouldGenerateArray()
    {
        var states = new HassState[] { new() { EntityId = "input_select.home_mode" } };

        var hassServiceDomains = Parse("""
                                       {
                                         "input_select": {
                                           "set_options": {
                                               "name": "Set options",
                                               "description": "Sets the options.",
                                               "fields": {
                                                   "options": {
                                                       "required": true,
                                                       "example": "[\u0022Item A\u0022, \u0022Item B\u0022, \u0022Item C\u0022]",
                                                       "selector": {
                                                           "text": {
                                                               "multiple": true
                                                           }
                                                       },
                                                       "name": "Options",
                                                       "description": "List of options."
                                                   }
                                                 },
                                                 "target": {
                                                   "entity": {
                                                     "domain": "input_select"
                                                   }
                                                 }
                                               }
                                            }
                                          }
                                       """);

        var appCode = WrapMethodBody(
            """
            entities.InputSelect.HomeMode.SetOptions(options: new string [] {"home", "away"});
            entities.InputSelect.HomeMode.SetOptions(options: ["home", "away"]);

            services.InputSelect.SetOptions(new ServiceTarget(), options: new string [] {"home", "away"});
            services.InputSelect.SetOptions(new ServiceTarget(), options: ["home", "away"]);
            """);

// Act:
        var code = CodeGenTestHelper.GenerateCompilationUnit(_settings, states, hassServiceDomains);
        CodeGenTestHelper.AssertCodeCompiles(code.ToString(), appCode);
    }


    [Fact]
    public void FieldNamedTarget_DoesNotClash()
    {
        var states = new HassState[] { new() { EntityId = "input_select.home_mode" } };

        var hassServiceDomains = Parse("""
                                       {
                                         "input_select": {
                                           "set_options": {
                                               "name": "Set options",
                                               "description": "Sets the options.",
                                               "fields": {
                                                   "target": {
                                                       "required": false,
                                                       "example": "Some Input Data",
                                                       "selector": {
                                                           "text": {
                                                           }
                                                       },
                                                       "name": "Target",
                                                       "description": "Field that is named target to verify it does not clash"
                                                   }
                                                 },
                                                 "target": {
                                                   "entity": {
                                                     "domain": "input_select"
                                                   }
                                                 }
                                               }
                                            }
                                          }
                                       """);

        var appCode = WrapMethodBody(
            """
            entities.InputSelect.HomeMode.SetOptions(target: "Some target");
            // the 'this' parameter should be called '_target' now to avoid a clash with the parameter for the field called target
            InputSelectEntityExtensionMethods.SetOptions(_target: entities.InputSelect.HomeMode, target: "Some target");

            services.InputSelect.SetOptions(new ServiceTarget(), target: "Some target");
            """);

// Act:
        var code = CodeGenTestHelper.GenerateCompilationUnit(_settings, states, hassServiceDomains);
        CodeGenTestHelper.AssertCodeCompiles(code.ToString(), appCode);
    }


    [Fact]
    public void ValidateLightsArguments()
    {
        var states = new HassState[] { new() { EntityId = "light.the_light" } };
        var hassServiceDomains = Parse(File.ReadAllText("CodeGenerator/ServiceMetaDataSamples/light.json"));

        var appCode = WrapMethodBody(
            """
                // make sure transtion is a double and rgbColor is IReadOnlyCollection<int>
                entities.Light.TheLight.TurnOn(transition: 0.5, rgbColor:[200, 12, 10]);
            """);

        var code = CodeGenTestHelper.GenerateCompilationUnit(_settings, states, hassServiceDomains);
        CodeGenTestHelper.AssertCodeCompiles(code.ToString(), appCode);
    }


    [Fact]
    public void CalendarArguments()
    {
        var states = new HassState[] { new() { EntityId = "calendar.holiday" } };
        var hassServiceDomains = Parse(File.ReadAllText("CodeGenerator/ServiceMetaDataSamples/calendar.json"));

        var appCode = WrapMethodBody(
            """
                entities.Calendar.Holiday.CreateEvent(summary: "Away", startDateTime: new DateTime(2024, 12, 3, 4, 2, 1), endDate: new DateOnly(2014, 12, 9));
            """);

        var code = CodeGenTestHelper.GenerateCompilationUnit(_settings, states, hassServiceDomains);
        CodeGenTestHelper.AssertCodeCompiles(code.ToString(), appCode);
    }

    private static string WrapMethodBody([StringSyntax("C#")]string methodBody)
    {
        var appCode = $$"""
                        using System.Threading.Tasks;
                        using NetDaemon.HassModel;
                        using NetDaemon.HassModel.Entities;
                        using RootNameSpace;
                        using System;

                        public class Root
                        {
                            public void Run(Entities entities, Services services)
                            {
                                {{methodBody}}
                            }
                        }
                        """;
        return appCode;
    }


    private static IReadOnlyCollection<HassServiceDomain> Parse([StringSyntax("json")] string sample)
    {
        var element = JsonDocument.Parse(sample).RootElement;
        var result = ServiceMetaDataParser.Parse(element, out var errors);
        errors.Should().BeEmpty();
        return result;
    }
}
