using System.Text.Json;
using NetDaemon.HassModel.CodeGenerator;
using NetDaemon.HassModel.CodeGenerator.Model;

namespace NetDaemon.HassModel.Tests.CodeGenerator;

public class ServiceMetaDataParserTest
{
    [Fact]
    public void TestSomeBasicServicesCanBeParsed()
    {
      var sample = """
          {
            "homeassistant": {
              "save_persistent_states": {
                "name": "Save Persistent States",
                "description": "Save the persistent states (for entities derived from RestoreEntity) immediately. Maintain the normal periodic saving interval.",
                "fields": {}
              },
              "turn_off": {
                "name": "Generic turn off",
                "description": "Generic service to turn devices off under any domain.",
                "fields": {},
                "target": {
                  "entity": {}
                }
              },
              "turn_on": {
                "name": "Generic turn on",
                "description": "Generic service to turn devices on under any domain.",
                "fields": {},
                "target": {
                  "entity": {}
                }
              }
            }
          }
          """;
      var res = Parse(sample);
      res.Should().HaveCount(1);
      res.First().Domain.Should().Be("homeassistant");
      res.First().Services.ElementAt(1).Target!.Entity.SelectMany(e=>e.Domain).Should().BeEmpty();
    }

    [Fact]
    public void TestServicesWithReturnValueCanBeParsed()
    {
      var sample = """
          {
             "weather": {
               "foo_with_return_value": {
                 "name": "Foo",
                   "description": "Foo with return value",
                   "fields": {},
                   "target": {
                     "entity": {}
                   },
                   "response": {
                     "optional": false
                   }
               },
               "foo_with_optional_return_value": {
                 "name": "Foo optional",
                   "description": "Foo with optional return value",
                   "fields": {},
                   "target": {
                     "entity": {}
                   },
                   "response": {
                     "optional": true
                   }
               }
             }
          }
          """;
      var res = Parse(sample);
      res.Should().HaveCount(1);
      res.First().Domain.Should().Be("weather");
      res.First().Services.ElementAt(0).Response!.Optional.Should().BeFalse();
      res.First().Services.ElementAt(1).Response!.Optional.Should().BeTrue();
    }

    [Fact]
    public void TestMultiDomainTarget()
    {
      var sample = """
         {
         "wiser": {
           "get_schedule": {
             "name": "Save Schedule to File",
             "description": "Read the schedule from a room or device and write to an output file in yaml\n",
             "fields": {
               "entity_id": {
                 "name": "Entity",
                 "description": "A wiser entity",
                 "required": true,
                 "selector": {
                   "entity": {
                     "integration": "wiser",
                     "domain": [
                       "climate",
                       "select"
                       ]
                     }
                   }
                 }
               }
             }
           }
         }
         """;
      var result = Parse(sample);

      result.First().Services.First().Fields!.First().Selector.Should()
        .BeAssignableTo<EntitySelector>().Which.Domain.Should().BeEquivalentTo("climate", "select");
    }

    [Fact]
    public void TestMultiDomainTargetWithRequiredFieldAsString()
    {
      var sample = """
         {
         "wiser": {
           "get_schedule": {
             "name": "Save Schedule to File",
             "description": "Read the schedule from a room or device and write to an output file in yaml\n",
             "fields": {
               "entity_id": {
                 "name": "Entity",
                 "description": "A wiser entity",
                 "required": "true",
                 "selector": {
                   "entity": {
                     "integration": "wiser",
                     "domain": [
                       "climate",
                       "select"
                       ]
                     }
                   }
                 }
               }
             }
           }
         }
         """;
      var result = Parse(sample);

      result.First().Services.First().Fields!.First().Required.Should().BeTrue();
    }

    [Fact]
    public void DeserializeTargetEntityArray()
    {
       var sample = """
         {
            "testdomain": {
              "purge_entities":{
              "name":"Purge Entities",
              "fields":{
              },
              "target":{
                 "entity":[
                    {
                      "domain":"targetdomain1"
                    },
                    {
                      "domain":["targetdomain2", "targetdomain3"]
                    }

                 ]
              }
           }
        }
      }
      """;
       var result = Parse(sample);
       result.First().Services.First().Target!.Entity.Should().HaveCount(2);
       result.First().Services.First().Target!.Entity[0].Domain.Should().Equal("targetdomain1");
       result.First().Services.First().Target!.Entity[1].Domain.Should().Equal("targetdomain2", "targetdomain3");

    }

    [Fact]
    public void NumericStepCanBeAny()
    {
        var sample = """
                     {
                         "homeassistant":
                         {
                            "set_location":{
                               "name":"Set location",
                               "description":"Updates the Home Assistant location.",
                               "fields":{
                                  "latitude":{
                                     "required":true,
                                     "example":32.87336,
                                     "selector":{
                                        "number":{
                                           "mode":"box",
                                           "min":-90,
                                           "max":90,
                                           "step":"any"
                                        }
                                     },
                                     "name":"Latitude",
                                     "description":"Latitude of your location."
                                  },
                                  "longitude":{
                                     "required":true,
                                     "example":117.22743,
                                     "selector":{
                                        "number":{
                                           "mode":"box",
                                           "min":-180,
                                           "max":180,
                                           "step":"any"
                                        }
                                     },
                                     "name":"Longitude",
                                     "description":"Longitude of your location."
                                  },
                                 "elevation":{
                                    "required":false,
                                    "example":120,
                                    "selector":{
                                       "number":{
                                          "mode":"box",
                                          "step":"1"
                                       }
                                    },
                                    "name":"Elevation",
                                    "description":"Elevation of your location."
                                 },
                                 "testNumberStep":{
                                    "required":false,
                                    "example":120,
                                    "selector":{
                                       "number":{
                                          "mode":"box",
                                          "step":0.01
                                       }
                                    },
                                    "name":"Elevation",
                                    "description":"Elevation of your location."
                                 }
                               }
                            }
                         }
                     }
                     """;
        var result = Parse(sample);

        var steps = result.Single().Services.Single().Fields!.Select(f => (f.Selector as NumberSelector)!.Step).ToArray();
        steps.Should().Equal(null, null, 1, 0.01d); // any is mapped to null
    }


        [Fact]
    public void JsonError()
    {
        var sample = """
                     {
                       "orbiter_services": {
                         "invalid_json_service": {
                           "name": "Observe Planet",
                           "description": ["Array is not allowed here!"],
                           "fields": {
                             "frequency": {
                               "required": 1.1,
                               "example": false,
                               "selector": {
                                 "number": {
                                   "multiple": false,
                                   "mode": "box",
                                   "min": "N/A",
                                   "max": "N/A",
                                   "step": "any"
                                 }
                               }
                             }
                           }
                         },
                         "navigate": {
                           "name": "Navigates to a new location",
                           "fields": {
                             "latitude": {
                               "required": true,
                               "example": 32.87336,
                               "selector": {
                                 "number": {
                                   "mode": 212,
                                   "min": -90,
                                   "max": 90,
                                   "step": "any"
                                 }
                               },
                               "name": "Latitude",
                               "description": "Latitude of your location."
                             },
                             "longitude": {
                               "required": true,
                               "example": 117.22743,
                               "selector": {
                                 "number": {
                                   "mode": "box",
                                   "min": -180,
                                   "max": 180,
                                   "step": "any"
                                 }
                               },
                               "name": "Longitude",
                               "description": "Longitude of your location."
                             }
                           }
                         }
                       }
                     }
                     """;
        var element = JsonDocument.Parse(sample).RootElement;
        var result = ServiceMetaDataParser.Parse(element, out var errors);

        errors.Should().HaveCount(1, because: "We should get an error for the failed service");
        errors.Single().Context.Should().Be("orbiter_services.invalid_json_service");

        result.Should().HaveCount(1, because:"The service that is valid should still be parsed ");
        result.Single().Services.Should().HaveCount(1);

        // Just to manually validate the console output while running in the in the IDE
        Controller.CheckParseErrors(errors);
    }

    private static IReadOnlyCollection<HassServiceDomain> Parse(string sample)
    {
        var element = JsonDocument.Parse(sample).RootElement;
        var result = ServiceMetaDataParser.Parse(element, out var errors);
        errors.Should().BeEmpty();
        return result;
    }
}
