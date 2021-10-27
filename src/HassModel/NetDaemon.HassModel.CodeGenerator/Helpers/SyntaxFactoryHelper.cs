using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace NetDaemon.HassModel.CodeGenerator.Helpers
{
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
            return input.WithAttributeLists(
                SingletonList(
                    AttributeList(
                        SingletonSeparatedList(
                            Attribute(IdentifierName("System.Text.Json.Serialization.JsonPropertyName"))
                                .WithArgumentList(
                                    AttributeArgumentList(
                                        SingletonSeparatedList(
                                            AttributeArgument(
                                                LiteralExpression(
                                                    SyntaxKind.StringLiteralExpression,
                                                    Literal(name))))))))));
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

        // public static T WithSummary<T>(this T member, string summary)
        //     where T: SyntaxNode
        // {
        //      // parseLeadingTrivia($"/// <summary>{summary}<summary/>");
        //     var t = DocumentationCommentTrivia(SyntaxKind.SingleLineDocumentationCommentTrivia, 
        //         new SyntaxList<XmlNodeSyntax>(
        //             new XmlNodeSyntax[}XmlText().WithTextTokens(
        //         TokenList(XmlTextLiteral(TriviaList(DocumentationCommentExterior("///")))))]));
        //     return member.WithLeadingTrivia(Trivia(t));
        // }

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
}