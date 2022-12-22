namespace NetDaemon.HassModel.CodeGenerator.Model;

internal static class ServiceMetaDataParser
{
    public static readonly JsonSerializerOptions SnakeCaseNamingPolicySerializerOptions = new()
    {
        PropertyNamingPolicy = SnakeCaseNamingPolicy.Instance
    };

    /// <summary>
    ///     Parses all json elements to instance result from GetServices call
    /// </summary>
    /// <param name="element">JsonElement containing the result data</param>
    public static IReadOnlyCollection<HassServiceDomain> Parse(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Object)
            throw new InvalidOperationException("Not expected result from the GetServices result");

        var result = element.EnumerateObject().Select(property =>
            new HassServiceDomain
            {
                Domain = property.Name,
                Services = GetServices(property.Value)
            }).ToList();

        return result;
    }

    private static IReadOnlyCollection<HassService> GetServices(JsonElement element)
    {
        return element.EnumerateObject()
            .Select(serviceDomainProperty =>
                GetServiceFields(serviceDomainProperty.Name, serviceDomainProperty.Value)).ToList();
    }

    private static HassService GetServiceFields(string service, JsonElement element)
    {
        var result = element.Deserialize<HassService>(SnakeCaseNamingPolicySerializerOptions)! with
        {
            Service = service,
        };

        if (element.TryGetProperty("fields", out var fieldProperty))
        {
            result = result with
            {
                Fields = fieldProperty.EnumerateObject().Select(p => GetField(p.Name, p.Value)).ToList()
            };
        }

        return result;
    }

    private static HassServiceField GetField(string fieldName, JsonElement element)
    {
        return element.Deserialize<HassServiceField>(SnakeCaseNamingPolicySerializerOptions)! with
        {
            Field = fieldName,
        };
    }
}