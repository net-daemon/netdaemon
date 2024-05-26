namespace NetDaemon.HassModel.CodeGenerator.Model;

public record DeserializationError(Exception Exception, string? Context, JsonElement Element);

internal static class ServiceMetaDataParser
{

    public static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = SnakeCaseNamingPolicy.Instance,
        Converters = { new StringAsDoubleConverter() }
    };

    public static IReadOnlyCollection<HassServiceDomain> Parse(JsonElement element) => Parse(element, out _);

    /// <summary>
    ///     Parses all json elements to instance result from GetServices call
    /// </summary>
    /// <param name="element">JsonElement containing the result data</param>
    /// <param name="errors">Outputs Any Exceptions during deserialization</param>
    public static IReadOnlyCollection<HassServiceDomain> Parse(JsonElement element, out List<DeserializationError> errors)
    {
        errors = [];
        if (element.ValueKind != JsonValueKind.Object)
            throw new InvalidOperationException("Not expected result from the GetServices result");

        var hassServiceDomains = new List<HassServiceDomain>();
        foreach (var domainProperty in element.EnumerateObject())
        {
            try
            {
                var hassServiceDomain = new HassServiceDomain
                {
                    Domain = domainProperty.Name,
                    Services = GetServices(domainProperty.Value, errors, domainProperty.Name)
                };
                hassServiceDomains.Add(hassServiceDomain);
            }
            catch (JsonException e)
            {
                errors.Add(new (e, domainProperty.Name, domainProperty.Value));
            }
        }
        return hassServiceDomains;
    }

    private static IReadOnlyCollection<HassService> GetServices(JsonElement domainElement, List<DeserializationError> errors, string context)
    {
        return domainElement.EnumerateObject()
            .Select(serviceDomainProperty =>
                GetService(serviceDomainProperty.Name, serviceDomainProperty.Value, errors, context))
            .OfType<HassService>().ToList();
  }

    private static HassService? GetService(string serviceName, JsonElement serviceElement, List<DeserializationError> errors, string context)
    {
        try
        {
            var result = serviceElement.Deserialize<HassService>(SerializerOptions)! with
            {
                Service = serviceName,
            };

            if (serviceElement.TryGetProperty("fields", out var fieldProperty))
            {
                result = result with
                {
                    Fields = fieldProperty.EnumerateObject().Select(p => GetField(p.Name, p.Value)).ToList()
                };
            }

            return result;
        }
        catch (Exception ex)
        {
            errors.Add(new (ex, $"{context}.{serviceName}", serviceElement));
            return null;
        }
    }

    private static HassServiceField GetField(string fieldName, JsonElement element)
    {
        return element.Deserialize<HassServiceField>(SerializerOptions)! with
        {
            Field = fieldName,
        };
    }
}
