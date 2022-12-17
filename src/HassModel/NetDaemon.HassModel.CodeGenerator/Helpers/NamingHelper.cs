using System.Text.Json.Serialization;
using NetDaemon.HassModel.Entities.Core;

namespace NetDaemon.HassModel.CodeGenerator.Helpers;

internal static class NamingHelper
{
    public static string GetEntitiesForDomainClassName(string prefix)
    {
        var normalizedDomain = prefix.ToNormalizedPascalCase();

        return $"{normalizedDomain}Entities";
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

        return (SimplifyTypeName(type)!, variableName);
    }

    public static string SimplifyTypeName(Type type)
    {
        // Use short name if the type is in one of the using namespaces
        return UsingNamespaces.Any(u => type.Namespace == u) ? type.Name : type.FullName!;
    }

    public static readonly string[] UsingNamespaces =
    {
        "System",
        "System.Collections.Generic",
        typeof(JsonPropertyNameAttribute).Namespace!,
        typeof(IHaContext).Namespace!,
        typeof(Entity).Namespace!,
        typeof(LightAttributesBase).Namespace!
    };

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