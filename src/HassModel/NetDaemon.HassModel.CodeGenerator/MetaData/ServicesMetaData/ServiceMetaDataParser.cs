namespace NetDaemon.HassModel.CodeGenerator.Model;

internal static class ServiceMetaDataParser
{
    public static readonly JsonSerializerOptions SnakeCaseNamingPolicySerializerOptions = new()
    {
        PropertyNamingPolicy = SnakeCaseNamingPolicy.Instance
    };


    public static IReadOnlyCollection<HassServiceDomain> Parse(JsonElement element) => Parse(element, out _);

    /// <summary>
    ///     Parses all json elements to instance result from GetServices call
    /// </summary>
    /// <param name="element">JsonElement containing the result data</param>
    /// <param name="errors">Outputs Any Exceptions during deserialization</param>
    public static IReadOnlyCollection<HassServiceDomain> Parse(JsonElement element, out List<Exception> errors)
    {
        errors = new List<Exception>();
        if (element.ValueKind != JsonValueKind.Object)
            throw new InvalidOperationException("Not expected result from the GetServices result");

        var hassServiceDomains = new List<HassServiceDomain>();
        foreach (var property in element.EnumerateObject())
        {
            try
            {
                var hassServiceDomain = new HassServiceDomain
                {
                    Domain = property.Name,
                    Services = GetServices(property.Value)
                };
                hassServiceDomains.Add(hassServiceDomain);
            }
            catch (JsonException e)
            {
                Console.Error.WriteLine($"JSON deserialization of {nameof(HassServiceDomain)} failed: {e.Message}");
                Console.Error.WriteLine($"Deserialization source was: {property.Value}");
                errors.Add(e);
            }
        }
        return hassServiceDomains;
    }

    private static IReadOnlyCollection<HassService> GetServices(JsonElement element)
    {
        return element.EnumerateObject()
            .Select(serviceDomainProperty =>
                GetService(serviceDomainProperty.Name, serviceDomainProperty.Value)).ToList();
    }

    private static HassService GetService(string service, JsonElement element)
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
