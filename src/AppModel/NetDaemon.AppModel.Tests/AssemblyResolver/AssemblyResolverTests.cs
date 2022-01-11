using System.Reflection;
using LocalApps;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using NetDaemon.AppModel.Common.TypeResolver;
using NetDaemon.AppModel.Internal;
using NetDaemon.AppModel.Internal.Compiler;

namespace NetDaemon.AppModel.Tests.Internal.TypeResolver;

internal class ResolvedLocalApp
{
}

public class AssemblyResolverTests
{
    [Fact]
    public void TestLocalAssemblyShouldBeResolved()
    {
        var serviceCollection = new ServiceCollection();

        // get apps from test project
        serviceCollection.AddAppsFromAssembly(Assembly.GetCallingAssembly());

        serviceCollection.AddLogging();
        var provider = serviceCollection.BuildServiceProvider();

        var assemblyResolvers = provider.GetService<IEnumerable<IAssemblyResolver>>() ??
                                throw new NullReferenceException("Not expected null");
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
                        @"
                        public class FakeClass
                        {
                            
                        }
                        "
                        , Encoding.UTF8);
                    var syntaxTree = SyntaxFactory.ParseSyntaxTree(sourceText, path: "fakepath.cs")
                                     ?? throw new NullReferenceException("unexpected null reference");

                    result.Add(
                        syntaxTree
                    );
                    return result;
                }
            );

        var serviceCollection = new ServiceCollection();

        serviceCollection.AddAppsFromSource();
        serviceCollection
            .AddSingleton(_ => syntaxTreeResolverMock.Object);

        serviceCollection.AddLogging();
        var provider = serviceCollection.BuildServiceProvider();

        var assemblyResolvers = provider.GetService<IEnumerable<IAssemblyResolver>>() ??
                                throw new NullReferenceException("Not expected null");
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
                        @"
                        public class FakeClass
                        {
                            
                        }
                        "
                        , Encoding.UTF8);
                    var syntaxTree = SyntaxFactory.ParseSyntaxTree(sourceText, path: "fakepath.cs")
                                     ?? throw new NullReferenceException("unexpected null reference");

                    result.Add(
                        syntaxTree
                    );
                    return result;
                }
            );

        var serviceCollection = new ServiceCollection();
        // get apps from test project
        serviceCollection.AddAppsFromAssembly(Assembly.GetCallingAssembly());
        serviceCollection.AddAppsFromSource();
        serviceCollection
            .AddSingleton(_ => syntaxTreeResolverMock.Object);

        serviceCollection.AddLogging();
        var provider = serviceCollection.BuildServiceProvider();

        var assemblyResolvers = provider.GetService<IEnumerable<IAssemblyResolver>>() ??
                                throw new NullReferenceException("Not expected null");
        assemblyResolvers.Should().HaveCount(2);
    }
}