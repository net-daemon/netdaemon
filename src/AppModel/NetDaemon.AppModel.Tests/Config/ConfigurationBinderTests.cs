using Microsoft.Extensions.Configuration;
using NetDaemon.AppModel.Internal.Config;

namespace NetDaemon.AppModel.Tests.Config;

public class ConfigurationBinderTests
{
    [Fact]
    public void TestAddYamlConfigGetsSettingsCorrectly()
    {
        // ARRANGE
        // ACT
        var config = GetObjectFromSection<CollectionBindingTestClass>("TestCollections");
        // CHECK
        config!.SomeEnumerable.Should().HaveCount(2);
        config.SomeList.Should().HaveCount(2);
        config.SomeReadOnlyList.Should().HaveCount(2);
        config.SomeReadOnlyCollection.Should().HaveCount(2);
        config.SomeCollection.Should().HaveCount(2);
        config.SomeReadOnlyDictionary.Should().HaveCount(2);
        config.SomeDictionary.Should().HaveCount(2);
    }

    [Fact]
    public void TestWhenInitIEnumerableThrowsException()
    {
        // ARRANGE
        // ACT
        // CHECK
        Assert.Throws<InvalidOperationException>(() => GetObjectFromSection<CollectionBindingFaultyIEnumerableTestClass>("TestCollections"));
    }

    [Fact]
    public void TestWhenInitIReadOnlyListThrowsException()
    {
        // ARRANGE
        // ACT
        // CHECK
        Assert.Throws<InvalidOperationException>(() => GetObjectFromSection<CollectionBindingFaultyIReadOnlyListTestClass>("TestCollections"));
    }

    [Fact]
    public void TestWhenInitIReadOnlyCollectionThrowsException()
    {
        // ARRANGE
        // ACT
        // CHECK
        Assert.Throws<InvalidOperationException>(() => GetObjectFromSection<CollectionBindingFaultyIReadOnlyCollectionTestClass>("TestCollections"));
    }

    [Fact]
    public void TestWhenInitIReadOnlyDictionaryThrowsException()
    {
        // ARRANGE
        // ACT
        Assert.Throws<InvalidOperationException>(() =>
            GetObjectFromSection<CollectionBindingFaultyIReadOnlyDictionaryTestClass>("TestCollections"));
        // CHECK
    }

    [Fact]
    public void TestAddYamlConfigGetsCollectionsCorrectly()
    {
        // ARRANGE
        // ACT
        var config = GetObjectFromSection<IEnumerable<string>>("AnotherTestCollections");
        // CHECK
        config!.Should().HaveCount(3);
    }

    [Fact]
    public void TestAddYamlConfigGetsArrayCorrectly()
    {
        // ARRANGE
        // ACT
        var config = GetObjectFromSection<string[]>("AnotherTestArray");
        // CHECK
        config!.Should().HaveCount(2);
    }

    [Fact]
    public void TestAddYamlConfigGetsEnumCorrectly()
    {
        // ARRANGE
        // ACT
        var config = GetObjectFromSection<IEnumerable<TestEnum>>("AnotherTestEnum");
        // CHECK
        config!.Should().HaveCount(2);
    }

    [Fact]
    public void TestAddYamlConfigGetsDictionaryCorrectly()
    {
        // ARRANGE
        // ACT
        var config = GetObjectFromSection<IDictionary<string, string>>("AnotherTestDictionary");
        // CHECK
        config!.Should().HaveCount(3);
    }

    [Fact]
    public void TestAddYamlConfigGetsDictionaryOfListsCorrectly()
    {
        // ARRANGE
        // ACT
        var config = GetObjectFromSection<IDictionary<string, ICollection<string>>>("AnotherTestDictionaryOfLists");
        // CHECK
        config!.Should().HaveCount(2);
    }

    [Fact]
    public void TestAddYamlConfigGetsListOfDictionaryCorrectly()
    {
        // ARRANGE
        // ACT
        var config = GetObjectFromSection<ICollection<IDictionary<string, string>>>("AnotherTestListOfDictionary");
        // CHECK
        config!.Should().HaveCount(2);
    }

    [Fact]
    public void TestAddYamlConfigBadClassShouldThrowException()
    {
        // ARRANGE
        var configurationBuilder = new ConfigurationBuilder() as IConfigurationBuilder;

        configurationBuilder.AddYamlAppConfig(
            Path.Combine(AppContext.BaseDirectory,
                "Config/Fixtures"));
        configurationBuilder.Build();
        // ACT
        // CHECK
        Assert.Throws<InvalidOperationException>(() => GetObjectFromSection<IAppModel>("TestCollections"));
        Assert.Throws<InvalidOperationException>(() =>
            GetObjectFromSection<AbstractShouldNotSerialize>("TestCollections"));
        Assert.Throws<InvalidOperationException>(() =>
            GetObjectFromSection<ClassWithoutDefaultConstructorShouldNotSerialize>("TestCollections"));
        Assert.Throws<InvalidOperationException>(() =>
            GetObjectFromSection<ClassThatThrowsOnConstructor>("TestCollections"));
        Assert.Throws<InvalidOperationException>(() => GetObjectFromSection<string[,]>("TestCollections"));
        GetObjectFromSection<IDictionary<int, int>>("TestCollections").Should().BeEmpty();
    }

    private static T? GetObjectFromSection<T>(string sectionName)
    {
        var providerMock = new Mock<IServiceProvider>();
        var configurationBuilder = new ConfigurationBuilder() as IConfigurationBuilder;
        configurationBuilder.AddYamlAppConfig(
            Path.Combine(AppContext.BaseDirectory,
                "Config/Fixtures"));
        var configRoot = configurationBuilder.Build();
        var section = configRoot.GetSection(sectionName);
        // ACT
        var configBinding = new ConfigurationBinding(providerMock.Object);
        return configBinding.ToObject<T>(section);
    }

    internal enum TestEnum
    {
        Enum1,
        Enum2
    }

    #region -- failing class declarations --

    internal abstract class AbstractShouldNotSerialize
    {
    }

    internal class ClassWithoutDefaultConstructorShouldNotSerialize
    {
        private ClassWithoutDefaultConstructorShouldNotSerialize()
        {
        }
    }

    internal class ClassThatThrowsOnConstructor
    {
        public ClassThatThrowsOnConstructor()
        {
            throw new InvalidOperationException();
        }
    }

    #endregion
}

public record CollectionBindingTestClass
{
    public IEnumerable<string>? SomeEnumerable { get; init; }
    public List<string>? SomeList { get; init; }
    public IReadOnlyList<string>? SomeReadOnlyList { get; init; }
    public IReadOnlyCollection<string>? SomeReadOnlyCollection { get; init; }
    public ICollection<string>? SomeCollection { get; init; }
    public IReadOnlyDictionary<string, string>? SomeReadOnlyDictionary { get; init; }
    public IDictionary<string, string>? SomeDictionary { get; init; }
}

public record CollectionBindingFaultyIEnumerableTestClass
{
    public IEnumerable<string>? SomeEnumerable { get; init; } = new List<string>();
}

public record CollectionBindingFaultyIReadOnlyListTestClass
{
    public IReadOnlyList<string>? SomeReadOnlyList { get; init; } = new List<string>();
}

public record CollectionBindingFaultyIReadOnlyCollectionTestClass
{
    public IReadOnlyCollection<string>? SomeReadOnlyCollection { get; init; } = new List<string>();
}

public record CollectionBindingFaultyIReadOnlyDictionaryTestClass
{
    public IReadOnlyDictionary<string, string>? SomeReadOnlyDictionary { get; init; } =
        new Dictionary<string, string>();
}
