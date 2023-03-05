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
      var element = JsonDocument.Parse(sample).RootElement;
      var res = ServiceMetaDataParser.Parse(element);
      res.Should().HaveCount(1);
      res.First().Domain.Should().Be("homeassistant");
      res.First().Services.ElementAt(1).Target!.Entity!.Domain.Should().BeEmpty();
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

    private static IReadOnlyCollection<HassServiceDomain> Parse(string sample)
    {
      var element = JsonDocument.Parse(sample).RootElement;
      return ServiceMetaDataParser.Parse(element);
    }
}