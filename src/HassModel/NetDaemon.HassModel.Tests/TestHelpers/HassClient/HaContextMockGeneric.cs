// using System.Reactive.Subjects;
// using NetDaemon.HassModel.Entities;
// using NetDaemon.Client.HomeAssistant.Model;

// namespace NetDaemon.HassModel.Tests.TestHelpers.HassClient;

// internal class HaContextMockGeneric<TState, TAttributes> : Mock<IHaContext>
// {
//     public HaContextMockGeneric()
//     {
//         Setup(m => m.StateAllChangesGeneric(It.IsAny<Func<IHaContext, HassStateChangeEventData, IStateChange<TState, TAttributes>>> mapper)).Returns<Func<IHaContext, HassStateChangeEventData, IStateChange<TState, TAttributes>>>(mapper => mapper(HassStateChangedEventDataSubject));
//         Setup(m => m.Events).Returns(EventsSubject);
//     }

//     public Subject<HassStateChangedEventData> HassStateChangedEventDataSubject { get; } = new();
//     public Subject<Event> EventsSubject { get; } = new();
// }