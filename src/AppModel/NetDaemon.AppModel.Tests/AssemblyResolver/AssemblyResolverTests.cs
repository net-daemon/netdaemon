using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using NetDaemon.AppModel.Internal;
using NetDaemon.AppModel.Internal.Compiler;

namespace NetDaemon.AppModel.Tests.Internal.TypeResolver;

internal class ResolvedLocalApp { }

public class AssemblyResolverTests
{
    [Fact]
    public void TestLocalAssemblyShouldBeResolved()
    {
        var serviceCollection = new ServiceCollection();

        serviceCollection.AddAppModelLocalAssembly();

        serviceCollection.AddLogging();
        var provider = serviceCollection.BuildServiceProvider();

        var assemblyResolvers = provider.GetService<IEnumerable<IAssemblyResolver>>() ?? throw new NullReferenceException("Not expected null");
        assemblyResolvers.Should().HaveCount(1);
    }

    [Fact]
    public void TestDynamicallyCompiledAssemblyShouldBeResolved()
    {
        var syntaxTreeResolverMock = new Mock<ISyntaxTreeResolver>();
        // We setup the mock to return a pre-built syntax tree with a fake class
        syntaxTreeResolverMock
            .Setup(
                n => n.GetSyntaxTrees()
            )
            .Returns(
                () =>
                {
                    var result = new List<SyntaxTree>();
                    var sourceText = SourceText.From(
                        text:
                        @"
                        public class FakeClass
                        {
                            
                        }
                        "
                        , encoding: Encoding.UTF8);
                    var syntaxTree = SyntaxFactory.ParseSyntaxTree(sourceText, path: "fakepath.cs")
                        ?? throw new NullReferenceException("unexpected null reference");

                    result.Add(
                        syntaxTree
                    );
                    return result;
                }
            );

        var serviceCollection = new ServiceCollection();

        serviceCollection.AddAppModelDynamicCompliedAssembly();
        serviceCollection
            .AddSingleton(_ => syntaxTreeResolverMock.Object);

        serviceCollection.AddLogging();
        var provider = serviceCollection.BuildServiceProvider();

        var assemblyResolvers = provider.GetService<IEnumerable<IAssemblyResolver>>() ?? throw new NullReferenceException("Not expected null");
        assemblyResolvers.Should().HaveCount(1);

    }

    [Fact]
    public void TestBothLocalAndDynamicallyResolvedAssembliesShouldBeResolved()
    {
        var syntaxTreeResolverMock = new Mock<ISyntaxTreeResolver>();
        // We setup the mock to return a prebuilt syntax tree with a fake class
        syntaxTreeResolverMock
            .Setup(
                n => n.GetSyntaxTrees()
            )
            .Returns(
                () =>
                {
                    var result = new List<SyntaxTree>();
                    var sourceText = SourceText.From(
                        text:
                        @"
                        public class FakeClass
                        {
                            
                        }
                        "
                        , encoding: Encoding.UTF8);
                    var syntaxTree = SyntaxFactory.ParseSyntaxTree(sourceText, path: "fakepath.cs")
                        ?? throw new NullReferenceException("unexpected null reference");

                    result.Add(
                        syntaxTree
                    );
                    return result;
                }
            );

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddAppModelLocalAssembly();
        serviceCollection.AddAppModelDynamicCompliedAssembly();
        serviceCollection
            .AddSingleton(_ => syntaxTreeResolverMock.Object);

        serviceCollection.AddLogging();
        var provider = serviceCollection.BuildServiceProvider();

        var assemblyResolvers = provider.GetService<IEnumerable<IAssemblyResolver>>() ?? throw new NullReferenceException("Not expected null");
        assemblyResolvers.Should().HaveCount(2);
    }
}