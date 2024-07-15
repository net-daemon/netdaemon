
namespace NetDaemon.Tests.Performance;


internal static class Messages
{
    public static string AuthRequired =>
        $$"""
        {
          "type": "auth_required"
        }
        """;
    public static string AuthOk =>
        $$"""
        {
          "type": "auth_ok",
          "ha_version": "22.1.0"
        }
        """;

    public static string ResultMsg(int id) =>
        $$"""
        {
          "id": {{id}},
          "type": "result",
          "success": true,
          "result": null
        }
        """;

    public static string GetStatesResultMsg(int id) =>
        $$"""
        {
          "id": {{id}},
          "type": "result",
          "success": true,
          "result": [
            {
              "entity_id": "zone.test",
              "state": "zoning",
              "attributes": {
                "hidden": true,
                "latitude": 61.388466,
                "longitude": 12.295939,
                "radius": 200.0,
                "friendly_name": "Test",
                "icon": "mdi:briefcase"
              },
              "last_changed": "2019-02-16T18:11:44.183673+00:00",
              "last_updated": "2019-02-16T18:11:44.183673+00:00",
              "context": {
                "id": "f48f9a312c68402a81230b9f14a00a23",
                "parent_id": null,
                "user_id": null
              }
            },
            {
              "entity_id": "zone.test2",
              "state": "zoning",
              "attributes": {
                "hidden": true,
                "latitude": 63.438067,
                "longitude": 19.321611,
                "radius": 300.0,
                "friendly_name": "Test2",
                "icon": "mdi:hospital"
              },
              "last_changed": "2019-02-16T18:11:44.184844+00:00",
              "last_updated": "2019-02-16T18:11:44.184844+00:00",
              "context": {
                "id": "40a40e1aebe14464b2f7097c0a522810",
                "parent_id": null,
                "user_id": null
              }
            },
            {
              "entity_id": "zone.home",
              "state": "zoning",
              "attributes": {
                "hidden": true,
                "latitude": 61.4348599,
                "longitude": 16.1413237,
                "radius": 100,
                "friendly_name": "Home",
                "icon": "mdi:home"
              },
              "last_changed": "2019-02-16T18:11:44.187462+00:00",
              "last_updated": "2019-02-16T18:11:44.187462+00:00",
              "context": {
                "id": "cd7715bcd37b4b528540dc4bfa634de9",
                "parent_id": null,
                "user_id": null
              }
            },
            {
              "entity_id": "sun.sun",
              "state": "below_horizon",
              "attributes": {
                "next_dawn": "2019-02-17T05:47:33+00:00",
                "next_dusk": "2019-02-17T16:24:13+00:00",
                "next_midnight": "2019-02-16T23:05:52+00:00",
                "next_noon": "2019-02-17T11:05:53+00:00",
                "next_rising": "2019-02-17T06:35:00+00:00",
                "next_setting": "2019-02-17T15:36:46+00:00",
                "elevation": -39.06,
                "azimuth": 348.2,
                "friendly_name": "Sun"
              },
              "last_changed": "2019-02-16T18:11:44.204384+00:00",
              "last_updated": "2019-02-16T22:28:30.002772+00:00",
              "context": {
                "id": "2f6c1d0cee6a454d853150c7df4104c5",
                "parent_id": null,
                "user_id": null
              }
            },
            {
              "entity_id": "group.default_view",
              "state": "on",
              "attributes": {
                "entity_id": [
                  "group.climate",
                  "group.uteclimate",
                  "group.upper_floor",
                  "group.lower_floor",
                  "group.outside",
                  "sensor.my_alarm",
                  "input_select.house_mode_select",
                  "input_boolean.good_night_house",
                  "sensor.house_mode",
                  "group.frysar_temperatur",
                  "group.people_status",
                  "sensor.occupancy",
                  "weather.smhi_hemma",
                  "weather.yweather"
                ],
                "order": 0,
                "view": true,
                "friendly_name": "default_view",
                "icon": "mdi:home",
                "hidden": true,
                "assumed_state": true
              },
              "last_changed": "2019-02-16T18:11:45.104467+00:00",
              "last_updated": "2019-02-16T18:11:45.691968+00:00",
              "context": {
                "id": "d3b3e20ea93f49b190f56e08d5af5e80",
                "parent_id": null,
                "user_id": null
              }
            },
            {
              "entity_id": "scene.stadning",
              "state": "scening",
              "attributes": {
                "entity_id": [
                  "group.dummy"
                ],
                "friendly_name": "St\u00e4dning"
              },
              "last_changed": "2019-02-16T18:11:44.703590+00:00",
              "last_updated": "2019-02-16T18:11:44.703590+00:00",
              "context": {
                "id": "441f99ad4a4d44358b3aaa73e248a187",
                "parent_id": null,
                "user_id": null
              }
            },
            {
              "entity_id": "sensor.house_mode",
              "state": "Kv\u00e4ll",
              "attributes": {
                "friendly_name": "Hus status"
              },
              "last_changed": "2019-02-16T18:11:49.853247+00:00",
              "last_updated": "2019-02-16T18:11:49.853247+00:00",
              "context": {
                "id": "ff7c757b76f746da800158f70f1639a1",
                "parent_id": null,
                "user_id": null
              }
            },
            {
              "entity_id": "sensor.sally_phone_mqtt_bt",
              "state": "home",
              "attributes": {
                "confidence": "100",
                "friendly_name": "Sallys mobil BT",
                "entity_picture": "/local/sally.jpg"
              },
              "last_changed": "2019-02-16T18:32:22.167753+00:00",
              "last_updated": "2019-02-16T18:32:22.167753+00:00",
              "context": {
                "id": "0c9978ae1ce24a208c9a25272c44950b",
                "parent_id": null,
                "user_id": null
              }
            },
            {
              "entity_id": "binary_sensor.vardagsrum_pir",
              "state": "on",
              "attributes": {
                "battery_level": 100,
                "on": true,
                "friendly_name": "R\u00f6relsedetektor vardagsrum uppe",
                "device_class": "motion",
                "icon": "mdi:run-fast"
              },
              "last_changed": "2019-02-16T21:57:39.435927+00:00",
              "last_updated": "2019-02-16T21:57:39.435927+00:00",
              "context": {
                "id": "5f88441613664f2f94bfcb7202dabb3b",
                "parent_id": null,
                "user_id": null
              }
            },
            {
              "entity_id": "group.room_view",
              "state": "on",
              "attributes": {
                "entity_id": [
                  "group.room_sally",
                  "group.room_bedroom"
                ],
                "order": 1,
                "view": true,
                "friendly_name": "room_view",
                "icon": "mdi:home-circle",
                "hidden": true,
                "assumed_state": true
              },
              "last_changed": "2019-02-16T18:11:45.102432+00:00",
              "last_updated": "2019-02-16T18:11:45.689337+00:00",
              "context": {
                "id": "3f5092353d354d16abc4383e2b859278",
                "parent_id": null,
                "user_id": null
              }
            },
            {
              "entity_id": "group.ligths_view",
              "state": "on",
              "attributes": {
                "entity_id": [
                  "light.tomas_fonster",
                  "light.tvrummet"
                ],
                "order": 5,
                "view": true,
                "friendly_name": "ligths_view",
                "icon": "mdi:lightbulb-outline",
                "hidden": true,
                "assumed_state": true
              },
              "last_changed": "2019-02-16T18:11:46.025370+00:00",
              "last_updated": "2019-02-16T18:11:46.025370+00:00",
              "context": {
                "id": "b1806cf9774149c38d72d27c221539c1",
                "parent_id": null,
                "user_id": null
              }
            },
            {
              "entity_id": "switch.switch1",
              "state": "off",
              "attributes": {
                "friendly_name": "switch1",
                "assumed_state": true
              },
              "last_changed": "2019-02-16T18:11:45.613212+00:00",
              "last_updated": "2019-02-16T18:11:45.613212+00:00",
              "context": {
                "id": "9b38bb49ad474ed1a839eb36030e89c6",
                "parent_id": null,
                "user_id": null
              }
            },
            {
              "entity_id": "group.all_switches",
              "state": "on",
              "attributes": {
                "entity_id": [
                  "switch.switch1",
                  "switch.tv"
                ],
                "order": 35,
                "auto": true,
                "friendly_name": "all switches",
                "hidden": true,
                "assumed_state": true
              },
              "last_changed": "2019-02-16T18:11:46.665404+00:00",
              "last_updated": "2019-02-16T18:11:46.665404+00:00",
              "context": {
                "id": "94d4ee3f3503466388c43b435237b0d4",
                "parent_id": null,
                "user_id": null
              }
            },
            {
              "entity_id": "group.all_lights",
              "state": "on",
              "attributes": {
                "entity_id": [
                  "light.sallys_rum",
                  "light.tvrummet"
                ],
                "order": 36,
                "auto": true,
                "friendly_name": "all lights",
                "hidden": true
              },
              "last_changed": "2019-02-16T18:11:45.806440+00:00",
              "last_updated": "2019-02-16T18:12:02.869362+00:00",
              "context": {
                "id": "3cab55e957bd45d083229c9c441e806b",
                "parent_id": null,
                "user_id": null
              }
            },
            {
              "entity_id": "light.tvrummet_vanster",
              "state": "on",
              "attributes": {
                "min_mireds": 153,
                "max_mireds": 500,
                "brightness": 63,
                "color_temp": 442,
                "is_deconz_group": false,
                "friendly_name": "tvrummet_vanster",
                "supported_features": 43
              },
              "last_changed": "2019-02-16T18:11:45.977388+00:00",
              "last_updated": "2019-02-16T18:11:45.977388+00:00",
              "context": {
                "id": "ae2a620ae48d48fb91907d6f6e379419",
                "parent_id": null,
                "user_id": null
              }
            },
            {
              "entity_id": "light.sovrum_fonster",
              "state": "on",
              "attributes": {
                "brightness": 63,
                "is_deconz_group": false,
                "friendly_name": "sovrum_fonster",
                "supported_features": 41
              },
              "last_changed": "2019-02-16T18:11:45.978600+00:00",
              "last_updated": "2019-02-16T18:11:45.978600+00:00",
              "context": {
                "id": "c7f79ba696334de7a18c895c0491d499",
                "parent_id": null,
                "user_id": null
              }
            },
            {
              "entity_id": "sensor.occupancy",
              "state": "home",
              "attributes": {
                "last_seen": "R\u00f6relsedetektor Tomas rum 3 minutes ago",
                "time_last_seen": "2019-02-16 23:24"
              },
              "last_changed": "2019-02-16T18:11:53.596418+00:00",
              "last_updated": "2019-02-16T22:28:00.107816+00:00",
              "context": {
                "id": "f26aa4b08eb34fa58680bc2a0ca3b7bd",
                "parent_id": null,
                "user_id": "5a6c8d01ee2f4e4b90c7df8ea1ddd526"
              }
            },
            {
              "entity_id": "sensor.presence_tomas",
              "state": "Home",
              "attributes": {
                "entity_picture": "/local/tomas.jpg",
                "friendly_name": "Tomas tracker",
                "latitude": 61.233804,
                "longitude": 16.0312271,
                "proxi_direction": "stationary",
                "proxi_distance": 0,
                "source_type": "gps",
                "gps_accuracy": 16
              },
              "last_changed": "2019-02-16T18:11:53.650806+00:00",
              "last_updated": "2019-02-16T19:01:07.960903+00:00",
              "context": {
                "id": "8e768b8dc56a4257bb0540f9734a06de",
                "parent_id": null,
                "user_id": "5a6c8d01ee2f4e4b90c7df8ea1ddd526"
              }
            },
            {
              "entity_id": "persistent_notification.http_login",
              "state": "notifying",
              "attributes": {
                "title": "Login attempt failed",
                "message": "Login attempt or request with invalid authentication from 111.22.3.4"
              },
              "last_changed": "2019-02-16T21:45:59.552536+00:00",
              "last_updated": "2019-02-16T21:45:59.552536+00:00",
              "context": {
                "id": "e7f1576b60f1406397b9a8cfc75f1329",
                "parent_id": null,
                "user_id": null
              }
            }
          ]
        }
        """;

    public static string GetConfigResultMsg(int id) =>
        $$"""
        {
          "id": {{id}},
          "type": "result",
          "success": true,
          "result": {
            "latitude": 63.1394549,
            "longitude": 12.3412267,
            "elevation": 29,
            "state": "RUNNING",
            "unit_system": {
              "length": "km",
              "mass": "g",
              "temperature": "\u00b0C",
              "volume": "L"
            },
            "location_name": "Home",
            "time_zone": "Europe/Stockholm",
            "components": [
              "config.auth",
              "binary_sensor.deconz",
              "websocket_api",
              "deconz",
              "input_boolean",
              "api",
              "config.automation",
              "zone",
              "automation",
              "media_player",
              "scene.homeassistant",
              "system_health",
              "auth",
              "config.config_entries",
              "logbook",
              "config.script",
              "influxdb",
              "logger",
              "input_number",
              "sensor.template",
              "config.core",
              "light",
              "tts",
              "system_log",
              "sensor",
              "switch",
              "binary_sensor",
              "recorder",
              "input_select",
              "weather",
              "onboarding",
              "media_player.cast",
              "hassio",
              "history",
              "script",
              "group",
              "config.entity_registry",
              "map",
              "config.device_registry",
              "mqtt",
              "config",
              "discovery",
              "cast",
              "http",
              "device_tracker",
              "notify",
              "config.customize",
              "config.auth_provider_homeassistant",
              "switch.template",
              "scene",
              "config.group",
              "camera",
              "updater",
              "sensor.systemmonitor",
              "proximity",
              "cover",
              "sensor.time_date",
              "frontend",
              "sun",
              "config.area_registry",
              "lovelace"
            ],
            "config_dir": "/config",
            "whitelist_external_dirs": [
              "/config/www"
            ],
            "version": "0.87.0"
          }
        }
        """;

    public static string GetAreasResultMsg(int id) =>
        $$"""
        {
          "id": {{id}},
          "type": "result",
          "success": true,
          "result": [
            {
              "name": "Bedroom",
              "area_id": "5a30cdc2fd7f44d5a77f2d6f6d2ccd76"
            },
            {
              "name": "Kitchen",
              "area_id": "42a6048dc0404595b136545f6745c5d1"
            },
            {
              "name": "Livingroom",
              "area_id": "4e65b6fe3cea4604ab318b0f9c2b8432"
            }
          ]
        }
        """;

    public static string GetLabelsResultMsg(int id) =>
        $$"""
        {
            "id": {{id}},
            "type": "result",
            "success": true,
            "result": [
                {
                    "color": "green",
                    "description": null,
                    "icon": "mdi:chair-rolling",
                    "label_id": "label1",
                    "name": "Label 1"
                },
                {
                    "color": "indigo",
                    "description": null,
                    "icon": "mdi:lightbulb-night",
                    "label_id": "nightlights",
                    "name": "NightLights"
                }
            ]
        }
        """;

    public static string GetFloorsResultMsg(int id) =>
        $$"""
        {
            "id": {{id}},
            "type": "result",
            "success": true,
            "result": [
                {
                    "aliases": [],
                    "floor_id": "floor0",
                    "icon": null,
                    "level": 0,
                    "name": "Floor 0"
                },
                {
                    "aliases": [],
                    "floor_id": "floor1",
                    "icon": null,
                    "level": 1,
                    "name": "Floor 1"
                }
            ]
        }
        """;

    public static string GetDevicesResultMsg(int id) =>
        $$"""
        {
          "id": {{id}},
          "type": "result",
          "success": true,
          "result": [
            {
              "config_entries": [],
              "connections": [],
              "manufacturer": "Google Inc.",
              "model": "Chromecast",
              "name": "My TV",
              "sw_version": null,
              "id": "42cdda32a2a3428e86c2e27699d79ead",
              "via_device_id": null,
              "area_id": null,
              "name_by_user": null
            },
            {
              "config_entries": [
                "4b85129c61c74b27bd90e593f6b7482e"
              ],
              "connections": [],
              "manufacturer": "Plex",
              "model": "Plex Web",
              "name": "Plex (Plex Web - Chrome)",
              "sw_version": "4.22.3",
              "id": "49b27477238a4c8fb6cc8fbac32cebbc",
              "via_device_id": "6e17380a6d2744d18045fe4f627db706",
              "area_id": null,
              "name_by_user": null
            }
          ]
        }
        """;

    public static string GetEntitiesResultMsg(int id) =>
        $$"""
        {
          "id": {{id}},
          "type": "result",
          "success": true,
          "result": [
            {
              "config_entry_id": null,
              "device_id": "42cdda32a2a3428e86c2e27699d79ead",
              "disabled_by": null,
              "entity_id": "media_player.tv_uppe2",
              "area_id": "42cdda1212a3428e86c2e27699d79ead",
              "name": null,
              "icon": null,
              "platform": "cast"
            },
            {
              "config_entry_id": "4b85129c61c74b27bd90e593f6b7482e",
              "device_id": "6e17380a6d2744d18045fe4f627db706",
              "disabled_by": null,
              "entity_id": "sensor.plex_plex",
              "name": null,
              "icon": null,
              "platform": "plex"
            }
          ]
        }
        """;

    internal static string InputBooleanCreateResultMsg(int id) =>
        $$"""
        {
            "id": {{id}},
            "type": "result",
            "success": true,
            "result": null
        }
        """;
}

