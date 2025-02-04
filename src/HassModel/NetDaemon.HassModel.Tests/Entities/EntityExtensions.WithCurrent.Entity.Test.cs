
using NetDaemon.HassModel.Entities;
using System.Reactive.Subjects;
using System.Text.Json;

namespace NetDaemon.HassModel.Tests.Entities;

public sealed class EntityExtensionsWithCurrentEntityTest : IDisposable
{
    private const string InitialState = nameof(InitialState);
    private const string AttributePropertyName = "name";
    private const string InitialAttributeName = nameof(InitialAttributeName);
    private const string TestEntityId = "domain.testEntity";

    private readonly Entity _testEntity;
    private readonly Mock<IHaContext> _haContextMock;
    private readonly Subject<StateChange> _subject;
    private IObservable<StateChange> StateChanges => _subject;

    public EntityExtensionsWithCurrentEntityTest()
    {
        _haContextMock = new Mock<IHaContext>();

        var initialEntityState = new EntityState { State = InitialState, AttributesJson = CreateNameElement(InitialAttributeName) };
        _subject = new Subject<StateChange>();

        _haContextMock.Setup(t => t.StateAllChanges()).Returns(_subject);
        _haContextMock.Setup(t => t.GetState(TestEntityId)).Returns(initialEntityState);

        _testEntity = new Entity(_haContextMock.Object, TestEntityId);
    }

    [Fact]
    public void SubscribeStateChangesWithCurrent_InitiallyReturnsState()
    {
        // Arrange
        var results = new List<StateChange>();

        // Act
        StateChanges.WithCurrent(_testEntity).Subscribe(results.Add);

        // Assert
        AssertStates([InitialState], results);
    }

    [Fact]
    public void SubscribeStateChangesWithCurrent_InitiallyReturnsState_NotInitial()
    {
        // Arrange
        const string newState = nameof(newState);
        var results = new List<StateChange>();
        ChangeEntityState(newState);

        // Act
        StateChanges.WithCurrent(_testEntity).Subscribe(results.Add);

        // Assert
        AssertStates([newState], results);
    }

    [Fact]
    public void SubscribeStateChangesWithCurrent_StateIsUpdated()
    {
        // Arrange
        const string newState = nameof(newState);
        var results = new List<StateChange>();
        StateChanges.WithCurrent(_testEntity).Subscribe(results.Add);

        // Act
        ChangeEntityState(newState);

        // Assert
        AssertStates([InitialState, newState], results);
    }

    [Fact]
    public void SubscribeStateChangesWithCurrent_StateIsUpdatedBeforeSubscribe()
    {
        // Arrange
        const string newState = nameof(newState);
        var results = new List<StateChange>();
        var stateChangesWithCurrentObservable = StateChanges.WithCurrent(_testEntity);

        // Act
        ChangeEntityState(newState);
        stateChangesWithCurrentObservable.Subscribe(results.Add);

        // Assert
        AssertStates([newState], results);
    }

    [Fact]
    public void SubscribeStateChangesWithCurrent_MultipleSubscriptions()
    {
        // Arrange
        const string newState = nameof(newState);
        var subscription1Results = new List<StateChange>();
        var subscription2Results = new List<StateChange>();
        var stateChangesWithCurrentObservable = StateChanges.WithCurrent(_testEntity);

        // Act
        stateChangesWithCurrentObservable.Subscribe(subscription1Results.Add);
        ChangeEntityState(newState);
        stateChangesWithCurrentObservable.Subscribe(subscription2Results.Add);

        // Assert
        AssertStates([InitialState, newState], subscription1Results);
        AssertStates([newState], subscription2Results);
    }

    [Fact]
    public void StateChangesVariants_DifferentResults()
    {
        // Arrange
        const string state1 = nameof(state1);
        const string attribute1 = nameof(attribute1);
        const string state2 = nameof(state2);

        var stateChangesResults = new List<StateChange>();
        var stateAllChangesResults = new List<StateChange>();
        var stateChangesWithCurrentResults = new List<StateChange>();
        var stateAllChangesWithCurrentResults = new List<StateChange>();

        _testEntity.StateChanges().Subscribe(stateChangesResults.Add);
        _testEntity.StateAllChanges().Subscribe(stateAllChangesResults.Add);
        _testEntity.StateChangesWithCurrent().Subscribe(stateChangesWithCurrentResults.Add);
        _testEntity.StateAllChangesWithCurrent().Subscribe(stateAllChangesWithCurrentResults.Add);

        // Act
        ChangeEntityState(state1);
        ChangeEntityAttribute(attribute1);
        ChangeEntityState(state2);

        // Assert
        AssertStateAndAttributes([(state1, InitialAttributeName), (state2, attribute1)], stateChangesResults, expectStateChangeUponSubscribing: false);
        AssertStateAndAttributes([(state1, InitialAttributeName), (state1, attribute1), (state2, attribute1)], stateAllChangesResults, expectStateChangeUponSubscribing: false);
        AssertStateAndAttributes([(InitialState, InitialAttributeName), (state1, InitialAttributeName), (state2, attribute1)], stateChangesWithCurrentResults);
        AssertStateAndAttributes([(InitialState, InitialAttributeName), (state1, InitialAttributeName), (state1, attribute1), (state2, attribute1)], stateAllChangesWithCurrentResults);
    }

    private void ChangeEntityState(string newState)
    {
        var old = _testEntity.EntityState!;
        _haContextMock.Setup(t => t.GetState(TestEntityId)).Returns(new EntityState { State = newState, AttributesJson = old.AttributesJson });
        _subject.OnNext(new StateChange(_testEntity, old, _testEntity.EntityState));
    }

    private void ChangeEntityAttribute(string newNameValue)
    {
        var old = _testEntity.EntityState!;
        _haContextMock.Setup(t => t.GetState(TestEntityId)).Returns(new EntityState { State = old.State, AttributesJson = CreateNameElement(newNameValue) });
        _subject.OnNext(new StateChange(_testEntity, old, _testEntity.EntityState));
    }

    private static JsonElement CreateNameElement(string value) => JsonDocument.Parse($"{{\"{AttributePropertyName}\":\"{value}\"}}").RootElement;

    private static void AssertStates(
        string[] expectedStates,
        List<StateChange> stateChanges)
    {
        Assert.Equal(expectedStates.Length, stateChanges.Count);

        for (var i = 0; i < stateChanges.Count; i++)
        {
            var stateChange = stateChanges[i];

            if (i == 0)
            {
                Assert.Null(stateChange.Old);
            }
            else
            {
                Assert.Equal(stateChanges[i - 1].New?.State, stateChange.Old?.State);
            }
            Assert.Equal(expectedStates[i], stateChange.New?.State);
        }
    }

    private static void AssertStateAndAttributes(
        (string, string)[] expectedStateAndNameAttributes,
        List<StateChange> stateChanges,
        bool expectStateChangeUponSubscribing = true)
    {
        Assert.Equal(expectedStateAndNameAttributes.Length, stateChanges.Count);

        for (var i = 0; i < stateChanges.Count; i++)
        {
            if (!expectStateChangeUponSubscribing)
            {
                continue;
            }
            var stateChange = stateChanges[i];

            if (i == 0)
            {
                Assert.Null(stateChange.Old);
            }
            else
            {
                Assert.Equal(stateChanges[i - 1].New?.State, stateChange.Old?.State);
            }
            Assert.Equal(expectedStateAndNameAttributes[i].Item1, stateChange.New?.State);
            Assert.Equal(expectedStateAndNameAttributes[i].Item2, stateChange.New?.AttributesJson!.Value.GetProperty(AttributePropertyName).ToString());
        }
    }

    public void Dispose()
    {
        _subject.Dispose();
    }
}
