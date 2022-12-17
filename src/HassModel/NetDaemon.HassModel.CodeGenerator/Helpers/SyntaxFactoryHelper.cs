using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis.CSharp;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace NetDaemon.HassModel.CodeGenerator.Helpers;

internal static class SyntaxFactoryHelper
{
    public static GlobalStatementSyntax ParseMethod(string code)
    {
        return Parse<GlobalStatementSyntax>(code);
    }

    public static PropertyDeclarationSyntax ParseProperty(string code)
    {
        return Parse<PropertyDeclarationSyntax>(code);
    }

    public static RecordDeclarationSyntax ParseRecord(string code)
    {
        return Parse<RecordDeclarationSyntax>(code);
    }

    public static MemberDeclarationSyntax WithJsonPropertyName(this MemberDeclarationSyntax input, string name)
    {
        return input.WithAttribute<JsonPropertyNameAttribute>(name);
   }

    private static MemberDeclarationSyntax WithAttribute<T>(this MemberDeclarationSyntax property, string value) where T: Attribute
    {
        var name = (NamingHelper.SimplifyTypeName(typeof(T)));

        name = Regex.Replace(name, "Attribute$", "");
        var args = ParseAttributeArgumentList($"(\"{value}\")");
        var attribute = Attribute(ParseName(name), args);

        return property.WithAttributes(attribute);
    }

    private static MemberDeclarationSyntax WithAttributes(this MemberDeclarationSyntax property, params AttributeSyntax[]? attributeSyntaxes)
    {
        var attributes = property.AttributeLists.Add(
            AttributeList(SeparatedList(attributeSyntaxes)).NormalizeWhitespace());

        return property.WithAttributeLists(attributes);
    }

    public static PropertyDeclarationSyntax Property(string typeName, string propertyName, bool init = true)
    {
        return ParseProperty($"{typeName} {propertyName} {{ get; {( init ? "init; " : string.Empty )}}}");
    }

    public static ClassDeclarationSyntax ClassWithInjected<TInjected>(string className)
    {
        var (typeName, variableName) = NamingHelper.GetNames<TInjected>();

        var classCode = $@"class {className}
                          {{
                              private readonly {typeName} _{variableName};

                              public {className}( {typeName} {variableName})
                              {{
                                  _{variableName} = {variableName};
                              }}
                          }}";
        
        return ParseClass(classCode);
    }

    public static ClassDeclarationSyntax Class(string name)
    {
        return ClassDeclaration(name);
    }

    public static TypeDeclarationSyntax Interface(string name)
    {
        return InterfaceDeclaration(name);
    }

    public static RecordDeclarationSyntax Record(string name, IEnumerable<MemberDeclarationSyntax> properties)
    {
        return RecordDeclaration(Token(SyntaxKind.RecordKeyword), name)
            .WithOpenBraceToken(Token(SyntaxKind.OpenBraceToken))
            .AddMembers(properties.ToArray())
            .WithCloseBraceToken(Token(SyntaxKind.CloseBraceToken));
    }

    public static T ToPublic<T>(this T member)
        where T: MemberDeclarationSyntax
    {
        return (T)member.AddModifiers(Token(SyntaxKind.PublicKeyword));
    }

    public static T ToStatic<T>(this T member)
        where T: MemberDeclarationSyntax
    {
        return (T)member.AddModifiers(Token(SyntaxKind.StaticKeyword));
    }

    public static T WithBase<T>(this T member, string baseTypeName)
        where T: TypeDeclarationSyntax
    {
        return (T)member.WithBaseList(BaseList(SingletonSeparatedList<BaseTypeSyntax>(SimpleBaseType(IdentifierName(baseTypeName)))));
    }

    public  static T WithSummaryComment<T>(this T node, string? summary)
        where T : SyntaxNode =>
        string.IsNullOrWhiteSpace(summary) ? node : node.AppendTrivia(Comment($"///<summary>{summary.ReplaceLineEndings(" ")}</summary>"));

    public static T WithParameterComments<T>(this T node, ServiceArguments arguments)
        where T : SyntaxNode 
        => arguments.Arguments.Aggregate(node, (n, f) => n.WithParameterComment(f.VariableName, f.Comment));

    public  static T WithParameterComment<T>(this T node, string? name, string? description)
        where T : SyntaxNode
        => node.AppendTrivia(ParameterComment(name, description));

    public static SyntaxTrivia ParameterComment(string? paramName, string? comment)
        => Comment($"///<param name=\"{paramName?.ReplaceLineEndings(" ")}\">{comment?.ReplaceLineEndings(" ")}</param>");

    public static T AppendTrivia<T>(this T node, SyntaxTrivia? trivia)
        where T : SyntaxNode =>
        trivia is null ? node : node.WithLeadingTrivia(node.GetLeadingTrivia().Add(trivia.Value));

    private static T Parse<T>(string text)
    {
        var node = CSharpSyntaxTree.ParseText(text).GetRoot().ChildNodes().OfType<T>().FirstOrDefault();

        if (node is null)
            throw new ArgumentException($@"Text ""{text}"" contains invalid code", nameof(text));

        return node;
    }

    private static ClassDeclarationSyntax ParseClass(string code)
    {
        return Parse<ClassDeclarationSyntax>(code);
    }
}