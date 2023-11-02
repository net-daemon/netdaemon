using System.Collections.Generic;
using System.Text.Json;
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
                                  }
                               }
                            }
                         }
                     }
                     """;
        var result = Parse(sample);

        var steps = result.Single().Services.Single().Fields!.Select(f => (f.Selector as NumberSelector)!.Step).ToArray();
        steps.Should().Equal(null, null, 1); // any is mapped to null
    }

    private static IReadOnlyCollection<HassServiceDomain> Parse(string sample)
    {
        var element = JsonDocument.Parse(sample).RootElement;
        var result = ServiceMetaDataParser.Parse(element, out var errors);
        errors.Should().BeEmpty();
        return result;
    }
}
