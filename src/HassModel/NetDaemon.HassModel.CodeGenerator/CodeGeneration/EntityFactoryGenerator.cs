using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace NetDaemon.HassModel.CodeGenerator.CodeGeneration;

internal static class EntityFactoryGenerator
{
    public static MemberDeclarationSyntax Generate(IReadOnlyCollection<EntityDomainMetadata> domains)
    {
        var switchArms = domains
            .OrderBy(d => (d.Domain, d.IsNumeric ? 0 : 1)) // make sure numeric comes before non-numeric
            .Select(d => EntityIdHelper.MixedDomains.Contains(d.Domain) && d.IsNumeric
                ? $""" "{d.Domain}" when IsNumeric() => new {d.EntityClassName}(haContext, entityId), """
                : $""" "{d.Domain}" => new {d.EntityClassName}(haContext, entityId),""");

        var needsIsNumericHelper = domains.Any(d => d.IsNumeric && EntityIdHelper.MixedDomains.Contains(d.Domain));

        return ParseMemberDeclaration(
              $$"""
              /// <summary>
              /// Allows HassModel to instantiate the correct generated Entity types
              /// </summary>
              public class GeneratedEntityFactory : IEntityFactory
              {
                  public Entity CreateEntity(IHaContext haContext, string entityId)
                  {
                      var dot = entityId.IndexOf('.', StringComparison.Ordinal);
                      var domain = dot < 0 ? entityId.AsSpan() : entityId[..dot];

                      return domain switch
                      {
                      {{
                          string.Join("\r\n", switchArms)
                      }}
                          _ => new Entity(haContext, entityId)
                      };

                      {{(needsIsNumericHelper ? "bool IsNumeric() => haContext.GetState(entityId)?.AttributesJson?.TryGetProperty(\"unit_of_measurement\", out _) ?? false;" : "")}}
                  }
              }
              """)!;
    }
}
