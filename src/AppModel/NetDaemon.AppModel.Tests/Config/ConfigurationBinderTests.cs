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
    public void TestWhenInitIEnumerableShouldCorrectHaveCount()
    {
        // ARRANGE
        // ACT
        // CHECK
        var config = GetObjectFromSection<CollectionBindingIEnumerableWithInitTestClass>("TestCollections");
        config!.SomeEnumerable.Should().HaveCount(2);
    }

    [Fact]
    public void TestWhenInitIReadOnlyListShouldHaveCorrectCount()
    {
        // ARRANGE
        // ACT
        // CHECK
        var config = GetObjectFromSection<CollectionBindingIReadOnlyListWithInitTestClass>("TestCollections");
        config!.SomeReadOnlyList.Should().HaveCount(2);
    }

    [Fact]
    public void TestWhenInitIReadOnlyCollectionShouldHaveCorrectCount()
    {
        // ARRANGE
        // ACT
        // CHECK
        var config = GetObjectFromSection<CollectionBindingIReadOnlyCollectionWithInitTestClass>("TestCollections");
        config!.SomeReadOnlyCollection.Should().HaveCount(2);
    }

    [Fact]
    public void TestWhenInitIReadOnlyDictionaryShouldHaveCount()
    {
        // ARRANGE
        // ACT
        var config = GetObjectFromSection<CollectionBindingIReadOnlyDictionaryWithInitTestClass>("TestCollections");
        config!.SomeReadOnlyDictionary.Should().HaveCount(2);
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

    [Fact]
    public void TestAddYamlConfigGetsEnumWithUnderlyingTypeCorrectly()
    {
        // ARRANGE
        // ACT
        var config = GetObjectFromSection<IEnumerable<TestShortEnum>>("AnotherTestShortEnum");
        // CHECK
        config!.Should().HaveCount(2);
        Enum.GetUnderlyingType(config!.First().GetType()).Should().Be<ushort>();
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

    internal enum TestShortEnum : ushort
    {
        Enum1,
        Enum2
    }

    #region -- failing class declarations --

    internal abstract class AbstractShouldNotSerialize
    {
    }

    internal sealed class ClassWithoutDefaultConstructorShouldNotSerialize
    {
        private ClassWithoutDefaultConstructorShouldNotSerialize()
        {
        }
    }

    internal sealed class ClassThatThrowsOnConstructor
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

public record CollectionBindingIEnumerableWithInitTestClass
{
    public IEnumerable<string>? SomeEnumerable { get; init; } = [];
}

public record CollectionBindingIReadOnlyListWithInitTestClass
{
    public IReadOnlyList<string>? SomeReadOnlyList { get; init; } = [];
}

public record CollectionBindingIReadOnlyCollectionWithInitTestClass
{
    public IReadOnlyCollection<string>? SomeReadOnlyCollection { get; init; } = [];
}

public record CollectionBindingIReadOnlyDictionaryWithInitTestClass
{
    public IReadOnlyDictionary<string, string>? SomeReadOnlyDictionary { get; init; } =
        new Dictionary<string, string>();
}


