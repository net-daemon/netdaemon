using System.Reflection;
using LocalApps;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using NetDaemon.AppModel.Common.TypeResolver;
using NetDaemon.AppModel.Internal;
using NetDaemon.AppModel.Internal.Compiler;
using NetDaemon.AppModel.Tests.Helpers;

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
        serviceCollection.AddLogging();
        serviceCollection.AddTransient<IOptions<AppConfigurationLocationSetting>>(
            _ => new FakeOptions<AppConfigurationLocationSetting>(new AppConfigurationLocationSetting
            { ApplicationConfigurationFolder= Path.Combine(AppContext.BaseDirectory,
                Path.Combine(AppContext.BaseDirectory, "Fixtures/Dynamic"))}));
        serviceCollection.AddTransient<IOptions<AppSourceLocationSetting>>(
            _ => new FakeOptions<AppSourceLocationSetting>(new AppSourceLocationSetting
            { ApplicationSourceFolder= Path.Combine(AppContext.BaseDirectory,
                Path.Combine(AppContext.BaseDirectory, "Fixtures/Dynamic"))}));
        serviceCollection.AddSingleton(_ => syntaxTreeResolverMock.Object);
        serviceCollection.AddAppModelIfNotExist();
        serviceCollection.AddAppTypeResolverIfNotExist();
        serviceCollection.AddSingleton<CompilerFactory>();
        serviceCollection.AddSingleton<ICompilerFactory>(s => s.GetRequiredService<CompilerFactory>());
        serviceCollection.AddSingleton<DynamicallyCompiledAssemblyResolver>();
        serviceCollection.AddSingleton<IAssemblyResolver>(s =>
            s.GetRequiredService<DynamicallyCompiledAssemblyResolver>());


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

        serviceCollection.AddLogging();
        serviceCollection.AddTransient<IOptions<AppSourceLocationSetting>>(
            _ => new FakeOptions<AppSourceLocationSetting>(new AppSourceLocationSetting
            { ApplicationSourceFolder= Path.Combine(AppContext.BaseDirectory,
                Path.Combine(AppContext.BaseDirectory, "Fixtures/Dynamic"))}));
        serviceCollection.AddSingleton(_ => syntaxTreeResolverMock.Object);
        serviceCollection.AddAppModelIfNotExist();
        serviceCollection.AddAppTypeResolverIfNotExist();
        serviceCollection.AddSingleton<CompilerFactory>();
        serviceCollection.AddSingleton<ICompilerFactory>(s => s.GetRequiredService<CompilerFactory>());
        serviceCollection.AddSingleton<DynamicallyCompiledAssemblyResolver>();
        serviceCollection.AddSingleton<IAssemblyResolver>(s =>
            s.GetRequiredService<DynamicallyCompiledAssemblyResolver>());


        var provider = serviceCollection.BuildServiceProvider();

        var typeResolver = provider.GetService<IAppTypeResolver>() ??
                           throw new NullReferenceException("Not expected null");

        var t = typeResolver.GetTypes().Where(n => n.Name == "FakeClass").ToList();
        t.Should().HaveCount(1);
        t = typeResolver.GetTypes().Where(n => n.Name == "FakeClass").ToList();
        t.Should().HaveCount(1);
    }

    [Fact]
    public void AssemblyResolverShouldDisposeWithoutError()
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

        serviceCollection.AddLogging();
        
        serviceCollection.AddTransient<IOptions<AppSourceLocationSetting>>(
            _ => new FakeOptions<AppSourceLocationSetting>(new AppSourceLocationSetting
            { ApplicationSourceFolder= Path.Combine(AppContext.BaseDirectory,
                Path.Combine(AppContext.BaseDirectory, "Fixtures/Dynamic"))}));
        serviceCollection.AddSingleton(_ => syntaxTreeResolverMock.Object);
        serviceCollection.AddAppModelIfNotExist();
        serviceCollection.AddAppTypeResolverIfNotExist();
        serviceCollection.AddSingleton<CompilerFactory>();
        serviceCollection.AddSingleton<ICompilerFactory>(s => s.GetRequiredService<CompilerFactory>());
        serviceCollection.AddSingleton<DynamicallyCompiledAssemblyResolver>();
        serviceCollection.AddSingleton<IAssemblyResolver>(s =>
            s.GetRequiredService<DynamicallyCompiledAssemblyResolver>());


        var provider = serviceCollection.BuildServiceProvider();

        var typeResolver = provider.GetService<IAppTypeResolver>() ??
                           throw new NullReferenceException("Not expected null");

        var t = typeResolver.GetTypes().Where(n => n.Name == "FakeClass").ToList();
        t.Should().HaveCount(1);

        var assemblyResolver = provider.GetRequiredService<DynamicallyCompiledAssemblyResolver>();

        // Assert Dispose does not throw
        var exception = Record.Exception(() => assemblyResolver.Dispose());
        Assert.Null(exception);
    }

    [Fact]
    public void TestAddAppFromTypeShouldLoadSingleApp()
    {
        var serviceCollection = new ServiceCollection();

        // get apps from test project
        serviceCollection.AddAppFromType(typeof(MyAppLocalApp));

        serviceCollection.AddLogging();
        var provider = serviceCollection.BuildServiceProvider();

        var appResolvers = provider.GetRequiredService<IEnumerable<IAppTypeResolver>>() ??
                           throw new NullReferenceException("Not expected null");
        appResolvers.Should().HaveCount(1);
        appResolvers.First().GetTypes().Should().BeEquivalentTo(new[] {typeof(MyAppLocalApp)});
    }
}