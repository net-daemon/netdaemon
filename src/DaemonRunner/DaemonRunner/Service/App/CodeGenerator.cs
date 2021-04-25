using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using JoySoftware.HomeAssistant.Client;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NetDaemon.Common.Reactive;
using NetDaemon.Common.Reactive.Services;
using NetDaemon.Daemon.Config;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

[assembly: InternalsVisibleTo("NetDaemon.Daemon.Tests")]

namespace NetDaemon.Service.App
{
    public static class CodeGenerator
    {
        private static readonly Dictionary<string, string[]> _skipDomainServices = new()
        {
            {"lock", new[] {"lock", "unlock", "open"}},
            {"light", new[] {"turn_on", "turn_off", "toggle"}},
            {"script", new[] {"reload"}},
            {"automation", new[] {"turn_on", "turn_off", "toggle", "trigger", "reload"}},
            {"binary_sensor", new[] {"turn_on", "turn_off", "toggle"}},
            {
                "camera",
                new[]
                {
                    "turn_on", "turn_off", "toggle", "enable_motion_detection", "disable_motion_detection",
                    "play_stream", "record", "snapshot"
                }
            },
            {
                "climate",
                new[]
                {
                    "turn_on", "turn_off", "toggle",
                    "set_aux_heat", "set_preset_mode", "set_temperature",
                    "set_humidity", "set_fan_mode", "set_hvac_mode", "set_swing_mode"
                }
            },
            {
                "cover",
                new[]
                {
                    "open_cover", "close_cover", "stop_cover", "toggle", "open_cover_tilt", "close_cover_tilt",
                    "stop_cover_tilt", "set_cover_position", "set_cover_tilt_position", "toggle_cover_tilt"
                }
            },
            {"device_tracker", new[] {"see"}},
            {"group", new[] {"reload", "set", "remove"}},
            {"image_processing", new[] {"scan"}},
            {"input_boolean", new[] {"turn_on", "turn_off", "toggle", "reload"}},
            {
                "media_player",
                new[]
                {
                    "turn_on", "turn_off", "toggle", "volume_up", "volume_down", "volume_set", "volume_mute",
                    "media_play_pause", "media_play", "media_pause", "media_stop", "media_next_track",
                    "media_previous_track", "clear_playlist", "shuffle_set", "repeat_set", "play_media",
                    "select_source", "select_sound_mode", "media_seek"
                }
            },
            {"person", new[] {"reload"}},
            {"zone", new[] {"reload"}},
            {"scene", new[] {"reload", "apply", "create", "turn_on"}},
            {"sensor", new[] {"turn_on", "turn_off", "toggle"}},
            {"persistent_notification", new[] {"create", "dismiss", "mark_read"}},
            {"sun", new string[0]},
            {"weather", new string[0]},
            {"switch", new[] {"turn_on", "turn_off", "toggle"}},
            {
                "vacuum",
                new[]
                {
                    "turn_on", "turn_off", "start_pause", "start", "pause", "stop", "return_to_base", "locate",
                    "clean_spot", "set_fan_speed", "send_command", "toggle"
                }
            },
            {
                "alarm_control_panel",
                new[]
                {
                    "alarm_arm_home", "alarm_disarm", "alarm_arm_away", "alarm_arm_night", "alarm_arm_custom_bypass",
                    "alarm_trigger"
                }
            }
        };

        public static string GenerateCodeRx(string nameSpace, IReadOnlyCollection<string> entities,
            IReadOnlyCollection<HassServiceDomain> serviceDomains)
        {
            var entityDomains = GetDomainsFromEntities(entities).OrderBy(s => s).ToList();
            var serviceDomainsWithOperations =
                serviceDomains.Where(sd => sd.Services?.Any(s => !HasEntityIdArgument(s)) == true);

            var allDomains = entityDomains.Union(serviceDomainsWithOperations.Select(sd => sd.Domain)).OfType<string>()
                .OrderBy(s => s).ToList();

            var code = CompilationUnit()
                       .AddUsings(UsingDirective(ParseName("System.Collections.Generic")))
                       .AddUsings(UsingDirective(ParseName(typeof(RxEntityBase).Namespace!)))
                       .AddUsings(UsingDirective(ParseName(typeof(NetDaemonRxApp).Namespace!)));

           var namespaceDeclaration = NamespaceDeclaration(ParseName(nameSpace)).NormalizeWhitespace();

           // One Base class
           var appBaseClassDeclaration = GenerateAppBaseClass(allDomains);

            namespaceDeclaration = namespaceDeclaration.AddMembers(appBaseClassDeclaration);

            var serviceDomainByName = serviceDomains.ToLookup(sd => sd.Domain);

            // Typed entity classes
            foreach (var entityDomain in entityDomains)
            {
                var entityClass = GenerateEntityClass(entityDomain, serviceDomainByName[entityDomain].FirstOrDefault());

                namespaceDeclaration = namespaceDeclaration.AddMembers(entityClass);
            }

            // Domain classes
            foreach (var entityDomain in allDomains)
            {
                var entityClass = GenerateDomainClass(entityDomain, serviceDomainByName[entityDomain].FirstOrDefault(), entities);

                namespaceDeclaration = namespaceDeclaration.AddMembers(entityClass);
            }

            code = code.AddMembers(namespaceDeclaration);

            return code.NormalizeWhitespace(indentation: "    ", eol: "\n").ToFullString();
        }

        private static ClassDeclarationSyntax GenerateDomainClass(string domainName, HassServiceDomain? serviceDomain, IEnumerable<string> entities)
        {

            var classDeclaration = $@"public partial class {domainName.ToCamelCase()}Entities
                                      {{
                                          private readonly {nameof(NetDaemonRxApp)} _app;

                                          public {domainName.ToCamelCase()}Entities( {nameof(NetDaemonRxApp)} app)
                                          {{
                                              _app = app;
                                          }}
                                      }}";

            var entityClassDeclaration = Parse<ClassDeclarationSyntax>(classDeclaration);

            var domainEntities = entities.Where(n => n.StartsWith(domainName + ".", StringComparison.InvariantCultureIgnoreCase)).ToList();

            var propertyDeclaration = domainEntities.Select(e => GenerateEntityProperty(e, domainName)).ToArray();

            entityClassDeclaration = entityClassDeclaration.AddMembers(propertyDeclaration);

            var usedNames = propertyDeclaration.Select(p => p.Identifier.Text).ToHashSet();

            // Generate methods for all services that do not require an entity ID
            if (serviceDomain?.Services is null) return entityClassDeclaration;

            var nonEntityMethods = serviceDomain.Services.Where(s => !HasEntityIdArgument(s)).SelectMany(s =>
                GenerateServiceMethods(domainName, s, usedNames, "_app."));

            entityClassDeclaration = entityClassDeclaration.AddMembers(nonEntityMethods.ToArray());

            return entityClassDeclaration;
        }

        private static T Parse<T>(string text) =>  CSharpSyntaxTree.ParseText(text).GetRoot().ChildNodes().OfType<T>().First();

        /// <summary>
        /// Generate property to get type entity
        /// </summary>
        /// <returns></returns>
        private static PropertyDeclarationSyntax GenerateEntityProperty(string entity, string domain)
        {
            // public SwitchEntity LivingRoomSwitch => new(_app, new string[] { "switch.living_room_switch" });

            var name = entity[(entity.IndexOf(".", StringComparison.InvariantCultureIgnoreCase) + 1)..];
            name = MakeValidName(name, null, "e_", null);

            var propertyCode =  $@"public {domain.ToCamelCase()}Entity {name.ToCamelCase()} => new(_app, new string[] {{""{entity}""}});";

            var propDeclaration = Parse<PropertyDeclarationSyntax>(propertyCode);

            return propDeclaration;
        }

        private static ClassDeclarationSyntax GenerateEntityClass(string domainName, HassServiceDomain? serviceDomain)
        {
            var baseClass = _skipDomainServices.ContainsKey(domainName)
                ? $"{typeof(RxEntityBase).Namespace}.{domainName.ToCamelCase()}Entity"
                : $"{typeof(RxEntityBase).Namespace}.RxEntityBase";

            var classDeclaration = $@"  public partial class {domainName.ToCamelCase()}Entity : {baseClass}
                                        {{
                                                public {domainName.ToCamelCase()}Entity(INetDaemonRxApp daemon, IEnumerable<string> entityIds) : base(daemon, entityIds)
                                                {{
                                                }}
                                            }}";

            var entityClass = Parse<ClassDeclarationSyntax>(classDeclaration);

            // Generate methods for all services that do require an entity ID
            if (serviceDomain?.Services is null) return entityClass;

            var nonEntityMethods = serviceDomain.Services.Where(HasEntityIdArgument).SelectMany(s =>
                GenerateServiceMethods(domainName, s, new()));

            entityClass = entityClass.AddMembers(nonEntityMethods.ToArray());

            return entityClass;
        }

        private static IEnumerable<MemberDeclarationSyntax> GenerateServiceMethods(string domain, HassService service, HashSet<string> usedNames, string callPrefix = "")
        {
            var serviceMethodName = service.Service?[(service.Service.IndexOf(".", StringComparison.InvariantCultureIgnoreCase) + 1)..];

            List<MemberDeclarationSyntax> result = new();

            serviceMethodName = MakeValidName(serviceMethodName!, usedNames, "s_", "Service");

            var argumentsRecord = GenerateServiceArgsRecord(serviceMethodName, service);
            var argsTypeName = argumentsRecord.Identifier.Text;

            var hasEntityId = HasEntityIdArgument(service);
            var hasEntityIdString = hasEntityId ? "true" : "false";
            var methodCode = $@"public void {serviceMethodName}({argsTypeName}? data=null)
                                {{
                                    {callPrefix}CallService(""{domain}"", ""{service.Service}"", data,{hasEntityIdString});
                                }}";

            var methodDeclaration = Parse<GlobalStatementSyntax>(methodCode);

            result.Add(methodDeclaration);

            if (service.Fields?.Count() == (hasEntityId ? 2 : 1))
            {
                var paramName = service.Fields.Single(f => f.Field != "entity_id").Field;

                // We have a single prop besides the id, generate a simple wrapper method with one argument
                var methodCode2 = $@"public void {serviceMethodName}(string {paramName})
                                     {{
                                         {callPrefix}CallService(""{domain}"", ""{service.Service}"", new {{ {paramName} = {paramName} }} ,{hasEntityIdString});
                                     }}";

                var methodDeclaration2 = Parse<GlobalStatementSyntax>(methodCode2);

                result.Add(methodDeclaration2);
            }

            result.Add(argumentsRecord);

            return result;
        }

        private static ClassDeclarationSyntax GenerateAppBaseClass(IEnumerable<string> domains)
        {
            var extensionClass = ClassDeclaration("GeneratedAppBase");

            extensionClass = extensionClass.AddModifiers(Token(SyntaxKind.PublicKeyword));
            extensionClass = extensionClass.AddBaseListTypes(
                SimpleBaseType(ParseTypeName(nameof(NetDaemonRxApp))));

            // Get all available domains, this is used to create the extension methods

            var singleServiceDomains = new [] {"script"};
            foreach (var domain in domains)
            {
                var camelCaseDomain = domain.ToCamelCase();

                var isSingleServiceDomain = Array.IndexOf(singleServiceDomains, domain) != 0;

                var property = isSingleServiceDomain
                    ? $"public {camelCaseDomain}Entities {camelCaseDomain} => new(this);"
                    : $@"public {camelCaseDomain}Entity {camelCaseDomain} => new(this, new string[] {{""""}});";

                var propertyDeclaration = Parse<PropertyDeclarationSyntax>(property);
                extensionClass = extensionClass.AddMembers(propertyDeclaration);
            }

            return extensionClass;
        }

        private static TypeDeclarationSyntax GenerateServiceArgsRecord(string name, HassService s)
        {
            var record = RecordDeclaration(Token(SyntaxKind.RecordKeyword), name + "Args")
                .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                .WithOpenBraceToken(Token(SyntaxKind.OpenBraceToken));

            foreach (var field in s.Fields ?? Enumerable.Empty<HassServiceField>())
            {
                // TODO: make the prop camel case but serialize with original name
                var propCode = //$"[JsonProperty(propertyName:{name})]" +
                               $"public object {field.Field} {{ get; init; }}";

                var prop = Parse<MemberDeclarationSyntax>(propCode);
                record = record.AddMembers(prop);
            }

            record = record.WithCloseBraceToken(
                Token(SyntaxKind.CloseBraceToken));
            return record.NormalizeWhitespace();
        }

        private static bool HasEntityIdArgument(HassService service) => service.Fields?.Any(IsEntityId) == true;

        private static bool IsEntityId(HassServiceField f) => f.Field == "entity_id";

        /// <summary>
        ///     Returns a list of domains from all entities
        /// </summary>
        /// <param name="entities">A list of entities</param>
        internal static IEnumerable<string> GetDomainsFromEntities(IEnumerable<string> entities) =>
            entities.Select(n => n[..n.IndexOf(".", StringComparison.InvariantCultureIgnoreCase)]).Distinct();

        private static string MakeValidName(string name, HashSet<string>? usedNames, string prefix, string? suffix)
        {
            name = name.ToCamelCase();
            name = Regex.Replace(name, @"[^\w]", "");

            if (!char.IsLetter(name[0]) && (name[0] != '_'))
            {
                name = prefix + name;
            }

            if (usedNames !=null && !usedNames.Add(name))
            {
                name += suffix;
            }

            return name;
        }
    }
}