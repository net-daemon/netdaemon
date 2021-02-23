using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using JoySoftware.HomeAssistant.Client;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NetDaemon.Common;
using NetDaemon.Common.Exceptions;
using NetDaemon.Common.Reactive;
using NetDaemon.Common.Reactive.Services;
using NetDaemon.Daemon.Config;

[assembly: InternalsVisibleTo("NetDaemon.Daemon.Tests")]

namespace NetDaemon.Service.App
{
    public static class CodeGenerator
    {
        private static readonly Dictionary<string, string[]> _skipDomainServices = new Dictionary<string, string[]>()
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
                    "turn_on", "turn_off", "toggle", "set_aux_heat", "set_preset_mode", "set_temperature",
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

        public static string? GenerateCodeRx(string nameSpace, IEnumerable<string> entities,
            IEnumerable<HassServiceDomain> services)
        {
            var code = SyntaxFactory.CompilationUnit();

            // Add Usings statements
            code = code.AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System.Collections.Generic")));
            code = code.AddUsings(
                SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(typeof(RxEntityBase).Namespace!)));
            code = code.AddUsings(
                SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(typeof(NetDaemonRxApp).Namespace!)));

            // Add namespace
            var namespaceDeclaration = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(nameSpace))
                .NormalizeWhitespace();

            // Add support for extensions for entities
            var extensionClass = SyntaxFactory.ClassDeclaration("GeneratedAppBase");

            extensionClass = extensionClass.AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));
            extensionClass = extensionClass.AddBaseListTypes(
                SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName(nameof(NetDaemonRxApp))));

            // Get all available domains, this is used to create the extensionmethods
            var domains = GetDomainsFromEntities(entities);

            var singleServiceDomains = new string[] { "script" };
            foreach (var domain in domains)
            {
                var camelCaseDomain = domain.ToCamelCase();

                var isSingleServiceDomain = Array.IndexOf(singleServiceDomains, domain) != 0;

                var property = isSingleServiceDomain
                    ? $"public {camelCaseDomain}Entities {camelCaseDomain} => new(this);"
                    : $@"public {camelCaseDomain}Entity {camelCaseDomain} => new(this, new string[] {{""""}});";

                var propertyDeclaration = CSharpSyntaxTree.ParseText(property).GetRoot().ChildNodes()
                                              .OfType<PropertyDeclarationSyntax>().FirstOrDefault()
                                          ?? throw new NetDaemonNullReferenceException(
                                              $"Parse of property {camelCaseDomain} Entities/Entity failed");
                extensionClass = extensionClass.AddMembers(propertyDeclaration);
            }

            namespaceDeclaration = namespaceDeclaration.AddMembers(extensionClass);

            foreach (var domain in GetDomainsFromEntities(entities))
            {
                if (!ShouldGenerateDomainEntity(domain, services)) continue;
                var baseClass = _skipDomainServices.ContainsKey(domain)
                    ? $"{typeof(RxEntityBase).Namespace}.{domain.ToCamelCase()}Entity"
                    : $"{typeof(RxEntityBase).Namespace}.RxEntityBase";

                var classDeclaration = $@"public partial class {domain.ToCamelCase()}Entity : {baseClass}
{{
        public {domain.ToCamelCase()}Entity(INetDaemonRxApp daemon, IEnumerable<string> entityIds) : base(daemon, entityIds)
        {{
        }}
    }}";
                var entityClass = CSharpSyntaxTree.ParseText(classDeclaration).GetRoot().ChildNodes()
                                      .OfType<ClassDeclarationSyntax>().FirstOrDefault()
                                  ?? throw new NetDaemonNullReferenceException("Failed to parse class declaration");

                // They already have default implementation
                var skipServices = new string[] { "turn_on", "turn_off", "toggle" };

                foreach (var s in services.Where(n => n.Domain == domain)
                    .SelectMany(n => n.Services ?? new List<HassService>()))
                {
                    if (s.Service is null)
                        continue;

                    var name = s.Service[(s.Service.IndexOf(".", StringComparison.InvariantCultureIgnoreCase) + 1)..];

                    if (Array.IndexOf(skipServices, name) >= 0)
                        continue;

                    if (_skipDomainServices.ContainsKey(domain) && _skipDomainServices[domain].Contains(name))
                        continue;

                    // Quick check to make sure the name is a valid C# identifier. Should really check to make
                    // sure it doesn't collide with a reserved keyword as well.
                    if (!char.IsLetter(name[0]) && (name[0] != '_'))
                    {
                        name = "s_" + name;
                    }

                    var hasEntityId = s.Fields is not null && s.Fields.Any(c => c.Field == "entity_id");
                    var hasEntityIdString = hasEntityId ? "true" : "false";
                    var methodCode = $@"public void {name.ToCamelCase()}(dynamic? data=null)
                    {{
                        CallService(""{domain}"", ""{s.Service}"", data,{hasEntityIdString});
                    }}
                    ";
                    var methodDeclaration = CSharpSyntaxTree.ParseText(methodCode).GetRoot().ChildNodes()
                                                .OfType<GlobalStatementSyntax>().FirstOrDefault()
                                            ?? throw new NetDaemonNullReferenceException("Failed to parse method");
                    entityClass = entityClass.AddMembers(methodDeclaration);
                }

                namespaceDeclaration = namespaceDeclaration.AddMembers(entityClass);
            }

            // Add the classes implementing the specific entities
            foreach (var domain in GetDomainsFromEntities(entities))
            {
                var classDeclaration = $@"public partial class {domain.ToCamelCase()}Entities
    {{
        private readonly {nameof(NetDaemonRxApp)} _app;

        public {domain.ToCamelCase()}Entities( {nameof(NetDaemonRxApp)} app)
        {{
            _app = app;
        }}
    }}";
                var entityClass = CSharpSyntaxTree.ParseText(classDeclaration).GetRoot().ChildNodes()
                                      .OfType<ClassDeclarationSyntax>().FirstOrDefault()
                                  ?? throw new NetDaemonNullReferenceException("Failed to parse entity class");
                foreach (var entity in entities.Where(n =>
                    n.StartsWith(domain, StringComparison.InvariantCultureIgnoreCase)))
                {
                    var name = entity[(entity.IndexOf(".", StringComparison.InvariantCultureIgnoreCase) + 1)..];
                    // Quick check to make sure the name is a valid C# identifier. Should really check to make
                    // sure it doesn't collide with a reserved keyword as well.
                    if (!char.IsLetter(name[0]) && (name[0] != '_'))
                    {
                        name = "e_" + name;
                    }

                    var propertyCode =
                        $@"public {domain.ToCamelCase()}Entity {name.ToCamelCase()} => new(_app, new string[] {{""{entity}""}});";
                    var propDeclaration = CSharpSyntaxTree.ParseText(propertyCode).GetRoot().ChildNodes()
                                              .OfType<PropertyDeclarationSyntax>().FirstOrDefault()
                                          ?? throw new NetDaemonNullReferenceException("Failed to parse property");
                    entityClass = entityClass.AddMembers(propDeclaration);
                }

                namespaceDeclaration = namespaceDeclaration.AddMembers(entityClass);
            }

            code = code.AddMembers(namespaceDeclaration);

            return code.NormalizeWhitespace(indentation: "    ", eol: "\n").ToFullString();
        }

        /// <summary>
        ///     Returns a list of domains from all entities
        /// </summary>
        /// <param name="entities">A list of entities</param>
        internal static IEnumerable<string> GetDomainsFromEntities(IEnumerable<string> entities) =>
            entities.Select(n => n[0..n.IndexOf(".", StringComparison.InvariantCultureIgnoreCase)]).Distinct();

        private static bool ShouldGenerateDomainEntity(string domain, IEnumerable<HassServiceDomain> services)
        {
            if (!_skipDomainServices.ContainsKey(domain)) return true;
            var domainServiceNames = services.Where(n => n.Domain == domain)
                .SelectMany(n => n.Services ?? new List<HassService>()).Select(s =>
                    s.Service![(s.Service.IndexOf(".", StringComparison.InvariantCultureIgnoreCase) + 1)..]);
            return domainServiceNames.Except(_skipDomainServices[domain]).Any();
        }
    }
}