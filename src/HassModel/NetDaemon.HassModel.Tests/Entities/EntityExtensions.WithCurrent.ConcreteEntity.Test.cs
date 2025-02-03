
using NetDaemon.HassModel.Entities;
using System.Reactive.Subjects;
using NetDaemon.HassModel.Tests.TestHelpers.HassClient;

namespace NetDaemon.HassModel.Tests.Entities;

public sealed class EntityExtensionsWithCurrentConcreteEntityTest : IDisposable
{
    private const string InitialState = "Initial";
    private const string TestEntityId = "domain.testEntity";

    private readonly TestEntity _testEntity;
    private readonly Mock<IHaContext> _haContextMock;
    private readonly Subject<StateChange<TestEntity, EntityState<TestEntityAttributes>>> _subject;

    public EntityExtensionsWithCurrentConcreteEntityTest()
    {
        _haContextMock = new Mock<IHaContext>();

        var initialEntityState = new EntityState { State = InitialState };
        _subject = new Subject<StateChange<TestEntity, EntityState<TestEntityAttributes>>>();

        _haContextMock.Setup(t => t.GetState(TestEntityId)).Returns(initialEntityState);

        _testEntity = new TestEntity(_haContextMock.Object, TestEntityId);
    }

    private IObservable<StateChange<TestEntity, EntityState<TestEntityAttributes>>> StateChanges => _subject;

    private void ChangeEntityState(string newState)
    {
        var old = _testEntity.EntityState;
        _haContextMock.Setup(t => t.GetState(TestEntityId)).Returns(new EntityState { State = newState });
        _subject.OnNext(new StateChange<TestEntity, EntityState<TestEntityAttributes>>(_testEntity, old, _testEntity.EntityState));
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
        const string newState = "newState";
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
        const string newState = "newState";
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
        const string newState = "newState";
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
        const string newState = "newState";
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

    public void Dispose()
    {
        _subject.Dispose();
    }
}
