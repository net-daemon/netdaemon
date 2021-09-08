using System;
using NetDaemon.Daemon.Config;
namespace NetDaemon.Service.App.CodeGeneration.Helpers
{
    internal static class NamingHelper
    {
        public static string GetEntitiesTypeName(string prefix)
        {
            var normalizedDomain = Extensions.StringExtensions.ToNormalizedPascalCase(prefix);

            return $"{normalizedDomain}Entities";
        }

        public static string GetAttributesTypeName(string prefix)
        {
            var normalizedEntityId = Extensions.StringExtensions.ToNormalizedPascalCase(prefix);

            return $"{normalizedEntityId}Attributes";
        }

        public static string GetDomainEntityTypeName(string prefix)
        {
            var normalizedDomain = Extensions.StringExtensions.ToNormalizedPascalCase(prefix);

            return $"{normalizedDomain}Entity";
        }

        public static string GetServicesTypeName(string prefix)
        {
            var normalizedDomain = Extensions.StringExtensions.ToNormalizedPascalCase(prefix);

            return $"{normalizedDomain}Services";
        }

        public static string GetEntityDomainExtensionMethodClassName(string prefix)
        {
            var normalizedDomain = Extensions.StringExtensions.ToNormalizedPascalCase(prefix);

            return $"{normalizedDomain}EntityExtensionMethods";
        }

        public static string GetServiceMethodName(string serviceName)
        {
            serviceName = Extensions.StringExtensions.ToNormalizedPascalCase(serviceName);

            return $"{serviceName}";
        }

        public static string GetServiceArgumentsTypeName(string domain, string serviceName)
        {
            return $"{Extensions.StringExtensions.ToNormalizedPascalCase(domain)}{GetServiceMethodName(serviceName)}Parameters";
        }

        public static (string TypeName, string VariableName) GetNames<T>(string variablePrefix = "")
        {
            return GetNames(typeof(T), variablePrefix);
        }

        public static string GetVariableName<T>(string variablePrefix = "")
        {
            return GetNames<T>(variablePrefix).VariableName;
        }

        private static (string TypeName, string VariableName) GetNames(Type type, string variablePrefix = "")
        {
            // if it's an interface, remove an 'I' from start
            var className = type.IsInterface ? type.Name[1..] : type.Name;

            var variableName = GetVariableName(className, variablePrefix);

            return (type.FullName!, variableName);
        }

        private static string GetVariableName(string typeName, string variablePrefix = "")
        {
            return $"{variablePrefix}{typeName.ToCamelCase()}";
        }
    }
}