using Microsoft.CodeAnalysis.CSharp;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace NetDaemon.HassModel.CodeGenerator.CodeGeneration;

internal static class AttributeTypeGenerator
{
    /// <summary>
    /// Generates a record with all the attributes found in a set of entities.
    /// </summary>
    /// <example>
    /// public record LightAttributes : LightAttributesBase
    /// {
    ///     [JsonPropertyName("brightness")]
    ///     public double? Brightness { get; init; }
    ///
    ///     [JsonPropertyName("color_mode")]
    ///     public string? ColorMode { get; init; }
    ///    
    ///     [JsonPropertyName("color_temp")]
    ///     public double? ColorTemp { get; init; }
    /// }   
    /// </example>
    public static RecordDeclarationSyntax GenerateAttributeRecord(EntityDomainMetadata domain)
    {
        var propertyDeclarations = domain.Attributes
            .Select(a => Property($"{a.ClrType.GetFriendlyName()}?", a.CSharpName)
                .ToPublic()
                .WithJsonPropertyName(a.JsonName));

        var record = Record(domain.AttributesClassName, propertyDeclarations)
            .ToPublic()
            .AddModifiers(Token(SyntaxKind.PartialKeyword));

        return domain.AttributesBaseClass != null ? record.WithBase(SimplifyTypeName(domain.AttributesBaseClass)) : record;
    }
}