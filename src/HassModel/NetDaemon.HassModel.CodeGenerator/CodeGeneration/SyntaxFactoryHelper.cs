using System.Security;
using System.Text.Json.Serialization;
using Microsoft.CodeAnalysis.CSharp;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace NetDaemon.HassModel.CodeGenerator.Helpers;

internal static class SyntaxFactoryHelper
{
    // [JsonPropertyName(name)]
    public static MemberDeclarationSyntax WithJsonPropertyName(this MemberDeclarationSyntax input, string name)
    {
        var jsonPropertyName = SimplifyTypeName(typeof(JsonPropertyNameAttribute))[..^ "Attribute".Length];

        var attributes = input.AttributeLists.Add(AttributeList(SeparatedList(new[] 
        {
            Attribute(ParseName(jsonPropertyName), 
                    AttributeArgumentList(SingletonSeparatedList(
                        AttributeArgument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(name)))                        
                        )))
        })));

        return input.WithAttributeLists(attributes);
    }

    /// <summary>
    /// Creates a class with an injected IHaContext
    /// </summary>
    /// <returns>
    /// public partial class Entities
    /// {
    ///     private readonly IHaContext _haContext;
    ///     public Entities(IHaContext haContext)
    ///     {
    ///         _haContext = haContext;
    ///     }
    /// }
    ///  </returns>
    public static ClassDeclarationSyntax ClassWithInjectedHaContext(string className)
    {
        var newNameToken = Identifier(className);
        return ClassTemplate.ReplaceTokens(CtorNameTokens, (_,_) => newNameToken);
    }    
    
    private static readonly ClassDeclarationSyntax ClassTemplate = (ClassDeclarationSyntax)ParseMemberDeclaration("""
        public partial class __TypeName__
        {
            private readonly IHaContext _haContext;
        
            public __TypeName__(IHaContext haContext)
            {
                _haContext = haContext;
            }
        }
        """)!;

    private static readonly SyntaxToken[] CtorNameTokens = ClassTemplate.DescendantTokens().Where(t => t.Text == "__TypeName__").ToArray();

    
    public static RecordDeclarationSyntax Record(string name, IEnumerable<MemberDeclarationSyntax> properties)
    {
        return RecordDeclaration(Token(SyntaxKind.RecordKeyword), name)
            .WithOpenBraceToken(Token(SyntaxKind.OpenBraceToken))
            .WithMembers(List(properties))
            .WithCloseBraceToken(Token(SyntaxKind.CloseBraceToken));
    }

    public static T ToPublic<T>(this T member) where T: MemberDeclarationSyntax => 
        (T)member.AddModifiers(Token(SyntaxKind.PublicKeyword));

    public static T ToStatic<T>(this T member) where T: MemberDeclarationSyntax => 
        (T)member.AddModifiers(Token(SyntaxKind.StaticKeyword));

    public static T WithBase<T>(this T member, string baseTypeName) where T: TypeDeclarationSyntax => 
        (T)member.WithBaseList(BaseList(SingletonSeparatedList<BaseTypeSyntax>(SimpleBaseType(IdentifierName(baseTypeName)))));

    public  static T WithSummaryComment<T>(this T node, string? summary) where T : SyntaxNode =>
        string.IsNullOrWhiteSpace(summary) ? node : node.WithLeadingTrivia(Comment($"///<summary>{SecurityElement.Escape(summary.ReplaceLineEndings(" "))}</summary>"));

    public static T AppendParameterComments<T>(this T node, ServiceArguments arguments) where T : SyntaxNode 
        => node.WithLeadingTrivia(node.GetLeadingTrivia().Concat(arguments.Arguments.Select(a => ParameterComment(a.ParameterName, a.Comment))));

    public  static T AppendParameterComment<T>(this T node, string? name, string? description)
        where T : SyntaxNode
        => node.AppendTrivia(ParameterComment(name, description));

    public static SyntaxTrivia ParameterComment(string? paramName, string? comment)
        => Comment($"///<param name=\"{SecurityElement.Escape(paramName?.ReplaceLineEndings(" "))}\">{SecurityElement.Escape(comment?.ReplaceLineEndings(" "))}</param>");

    public static T AppendTrivia<T>(this T node, SyntaxTrivia? trivia)
        where T : SyntaxNode =>
        trivia is null ? node : node.WithLeadingTrivia(node.GetLeadingTrivia().Add(trivia.Value));

    /// <summary>
    /// Generates 'public {type} {name} => new(arg1, arg2);'
    /// </summary>
    public static MemberDeclarationSyntax PropertyWithExpressionBodyNew(string type, string name, params string[] args)
    {
        return 
            PropertyDeclaration(IdentifierName(type),  Identifier(name))
                .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword))) 
                .WithExpressionBody(ArrowExpressionClause(
                        ImplicitObjectCreationExpression()
                            .WithArgumentList(ArgumentList(
                                    SeparatedList(
                                        args.Select(a => Argument(IdentifierName(a))))))))                                    
                .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));        
    }
    
    /// <summary>
    /// Generates '{type} {name} {{get; init;}}'
    /// </summary>
    public static PropertyDeclarationSyntax AutoPropertyGetInit(string typeName, string propertyName)
    {
        return PropertyDeclaration(IdentifierName(typeName), Identifier(propertyName))
            .WithAccessorList(AccessorList(List(new[]
                    {
                        AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),
                        AccessorDeclaration(SyntaxKind.InitAccessorDeclaration)
                            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))
                    })));
    }

    /// <summary>
    /// Generates: `{type} {name} {{get;}}`
    /// </summary>
    public static PropertyDeclarationSyntax AutoPropertyGet(string typeName, string propertyName)
    {
        return PropertyDeclaration(IdentifierName(typeName), Identifier(propertyName))
            .WithAccessorList(AccessorList(SingletonList(
                            AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                                .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))
                        )));
    }
}
