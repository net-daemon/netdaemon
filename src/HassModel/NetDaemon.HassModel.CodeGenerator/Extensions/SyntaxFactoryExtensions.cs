using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace NetDaemon.HassModel.CodeGenerator.Extensions;

internal static class SyntaxFactoryExtensions
{
    public static PropertyDeclarationSyntax WithAttribute<T>(this PropertyDeclarationSyntax property, string value) where T: Attribute
    {
        if (property is null)
        {
            throw new ArgumentNullException(nameof(property));
        }

        var name = ParseName(typeof(T).FullName!);
        var args = ParseAttributeArgumentList($"(\"{value}\")");
        var attribute = Attribute(name, args);

        return property.WithAttributes(attribute);
    }

    private static PropertyDeclarationSyntax WithAttributes(this PropertyDeclarationSyntax property, params AttributeSyntax[]? attributeSyntaxes)
    {
        var attributes = property.AttributeLists.Add(
            AttributeList(SeparatedList(attributeSyntaxes)).NormalizeWhitespace());

        return property.WithAttributeLists(attributes);
    }
}