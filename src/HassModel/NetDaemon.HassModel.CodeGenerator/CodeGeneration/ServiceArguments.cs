using Microsoft.CodeAnalysis.CSharp;

namespace NetDaemon.HassModel.CodeGenerator;

internal record ServiceArgument
{
    public required string HaName { get; init; }
    public required Type ClrType { get; init; }
    public bool Required { get; init; }

    public string? Comment { get; init; }

    public string TypeName => ClrType.GetFriendlyName();

    public string ParameterTypeName => Required ? TypeName : $"{TypeName}?";

    public string PropertyName => HaName.ToNormalizedPascalCase();

    public string ParameterName => HaName.ToNormalizedCamelCase();

    
    public string ParameterDefault => Required ? "" : " = null";
}

internal class ServiceArguments
{
    private readonly string _serviceName;
    private readonly string _domain;

    public static ServiceArguments? Create(string domain, HassService service)
    {
        if (service.Fields is null || service.Fields.Count == 0)
        {
            return null;
        }

        return new ServiceArguments(domain, service.Service, service.Fields);
    }

    private ServiceArguments(string domain, string serviceName, IReadOnlyCollection<HassServiceField> serviceFields)
    {
        _domain = domain;
        _serviceName = serviceName!;
        Arguments = serviceFields.Select(HassServiceArgumentMapper.Map).ToArray();
    }

    public IEnumerable<ServiceArgument> Arguments { get; }

    public string TypeName => $"{_domain.ToNormalizedPascalCase()}{GetServiceMethodName(_serviceName)}Parameters";

    public string GetParametersList()
    {
        var argumentList = Arguments.OrderByDescending(arg => arg.Required);

        var anonymousVariableStr = argumentList.Select(x => $"{x.ParameterTypeName} {EscapeIfRequired(x.ParameterName)}{x.ParameterDefault}");

        return $"{string.Join(", ", anonymousVariableStr)}";
    }

    public string GetNewServiceArgumentsTypeExpression()
    {
        var propertyInitializers = Arguments.Select(x => $"{x.PropertyName} = {EscapeIfRequired(x.ParameterName)}");

        return $"new {TypeName} {{  { string.Join(", ", propertyInitializers) }  }}";
    }
    
    private static string EscapeIfRequired(string name)
    {
        var match = SyntaxFacts.GetKeywordKind(name) != SyntaxKind.None ||
                    SyntaxFacts.GetContextualKeywordKind(name) != SyntaxKind.None;

        return match ? "@" + name : name;
    }

}