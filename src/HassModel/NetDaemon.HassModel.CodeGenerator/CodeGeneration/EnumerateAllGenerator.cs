using Microsoft.CodeAnalysis.CSharp;

namespace NetDaemon.HassModel.CodeGenerator;

internal static class EnumerateAllGenerator
{
    public static MemberDeclarationSyntax[] GenerateEnumerateMethods(string domainPrefix, string entityClassName)
    {
        var enumerateAllMethod = SyntaxFactory.ParseMemberDeclaration($"""
                                                                       /// <summary>Enumerates all {domainPrefix} entities currently registered (at runtime) in Home Assistant as {entityClassName}</summary>
                                                                       public IEnumerable<{entityClassName}> EnumerateAll() =>
                                                                           _haContext.GetAllEntities()
                                                                               .Where(e => e.EntityId.StartsWith("{domainPrefix}."))
                                                                               .Select(e => new {entityClassName}(e));
                                                                       """);

        return domainPrefix == "sensor" ? [enumerateAllMethod, ..GenerateEnumerateAllSensor()] : [enumerateAllMethod];
    }

    /// <summary>
    /// For sensors we also add EnumerateAllNonNumeric and EnumerateAllNumeric
    /// </summary>
    /// <returns></returns>
    private static MemberDeclarationSyntax[] GenerateEnumerateAllSensor() =>
    [
        SyntaxFactory.ParseMemberDeclaration("""
                                             /// <summary>Enumerates all non-numeric sensor entities currently registered (at runtime) in Home Assistant as SensorEntity</summary>
                                             public IEnumerable<SensorEntity> EnumerateAllNonNumeric() =>
                                                 _haContext.GetAllEntities()
                                                     .Where(e => e.EntityId.StartsWith("sensor.")
                                                             && !(e.EntityState?.AttributesJson?.TryGetProperty("unit_of_measurement", out _) ?? false))
                                                     .Select(e => new SensorEntity(e));
                                             """),

        SyntaxFactory.ParseMemberDeclaration("""
                                             /// <summary>Enumerates all numeric sensor entities currently registered (at runtime) in Home Assistant as NumericSensorEntity</summary>
                                             public IEnumerable<NumericSensorEntity> EnumerateAllNumeric() =>
                                                 _haContext.GetAllEntities()
                                                     .Where(e => e.EntityId.StartsWith("sensor.")
                                                                 && (e.EntityState?.AttributesJson?.TryGetProperty("unit_of_measurement", out _) ?? false))
                                                     .Select(e => new NumericSensorEntity(e));
                                             """)
    ];
}
