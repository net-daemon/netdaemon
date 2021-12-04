using System.Reactive.Subjects;
using Moq;
using NetDaemon.HassModel.Common;
using NetDaemon.HassModel.Entities;

namespace NetDaemon.HassModel.Tests.Entities
{
    internal class HaContextMock : Mock<IHaContext>
    {
        public HaContextMock()
        {
            Setup(m => m.StateAllChanges()).Returns(StateAllChangeSubject);
        }

        public Subject<StateChange> StateAllChangeSubject { get;  } = new();
    }
}