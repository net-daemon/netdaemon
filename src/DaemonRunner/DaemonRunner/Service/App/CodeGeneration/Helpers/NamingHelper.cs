using System;
using NetDaemon.Daemon.Config;
using NetDaemon.Service.App.CodeGeneration.Extensions;
namespace NetDaemon.Service.App.CodeGeneration.Helpers
{
    internal static class NamingHelper
    {
        public static string GetEntitiesTypeName(string prefix)
        {
            var normalizedDomain = prefix.ToNormalizedPascalCase();

            return $"{normalizedDomain}Entities";
        }

        public static string GetAttributesTypeName(string prefix)
        {
            var normalizedEntityId = prefix.ToNormalizedPascalCase();

            return $"{normalizedEntityId}Attributes";
        }

        public static string GetDomainEntityTypeName(string prefix)
        {
            var normalizedDomain = prefix.ToNormalizedPascalCase();

            return $"{normalizedDomain}Entity";
        }

        public static string GetServicesTypeName(string prefix)
        {
            var normalizedDomain = prefix.ToNormalizedPascalCase();

            return $"{normalizedDomain}Services";
        }

        public static string GetEntityDomainExtensionMethodClassName(string prefix)
        {
            var normalizedDomain = prefix.ToNormalizedPascalCase();

            return $"{normalizedDomain}EntityExtensionMethods";
        }

        public static string GetServiceMethodName(string serviceName)
        {
            serviceName = serviceName.ToNormalizedPascalCase();

            return $"{serviceName}";
        }

        public static string GetServiceArgumentsTypeName(string domain, string serviceName)
        {
            return $"{domain.ToNormalizedPascalCase()}{GetServiceMethodName(serviceName)}Parameters";
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
            var variableName = GetVariableName(type.Name, variablePrefix);

            return (type.FullName!, variableName);
        }

        private static string GetVariableName(string typeName, string variablePrefix)
        {
            if (typeName.Length == 0)
            {
                return typeName;
            }

            if (char.ToLowerInvariant(typeName[0]) == 'i')
            {
                typeName = typeName[1..];
            }

            return $"{variablePrefix}{typeName.ToCamelCase()}";
        }
    }
}