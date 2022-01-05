using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using NetDaemon.AppModel.Common.TypeResolver;
using NetDaemon.AppModel.Internal.Compiler;

namespace NetDaemon.AppModel.Tests.Internal.TypeResolver;

internal class LocalApp
{
}

public class TypeResolverTests
{
    [Fact]
    public void TestLocalTypeResolverHasType()
    {
        var serviceCollection = new ServiceCollection();

        // get apps from test project
        serviceCollection.AddAppsFromAssembly(Assembly.GetExecutingAssembly());
        serviceCollection.AddLogging();
        var provider = serviceCollection.BuildServiceProvider();

        var typeResolver = provider.GetService<IAppTypeResolver>() ??
                           throw new NullReferenceException("Not expected null");

        var t = typeResolver.GetTypes().Where(n => n.Name == "LocalApp").ToList();
        t.Should().HaveCount(1);
    }

    [Fact]
    public void TestDynamicCompiledTypeResolverHasType()
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

        serviceCollection.AddAppsFromSource();
        serviceCollection
            .AddSingleton(_ => syntaxTreeResolverMock.Object);

        serviceCollection.AddLogging();
        var provider = serviceCollection.BuildServiceProvider();

        var typeResolver = provider.GetService<IAppTypeResolver>() ??
                           throw new NullReferenceException("Not expected null");

        var t = typeResolver.GetTypes().Where(n => n.Name == "FakeClass").ToList();
        t.Should().HaveCount(1);
    }

    [Fact]
    public void TestDynamicCompiledTypeResolverUsedMultipleTimesHasType()
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

        serviceCollection.AddAppsFromSource();
        serviceCollection
            .AddSingleton(_ => syntaxTreeResolverMock.Object);

        serviceCollection.AddLogging();
        var provider = serviceCollection.BuildServiceProvider();

        var typeResolver = provider.GetService<IAppTypeResolver>() ??
                           throw new NullReferenceException("Not expected null");

        var t = typeResolver.GetTypes().Where(n => n.Name == "FakeClass").ToList();
        t.Should().HaveCount(1);
        t = typeResolver.GetTypes().Where(n => n.Name == "FakeClass").ToList();
        t.Should().HaveCount(1);
    }
}