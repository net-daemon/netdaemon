using System;
using System.Reactive.Linq;
using System.Text.Json;
using Moq;

namespace NetDaemon.HassModel.Tests.TestHelpers
{
    internal static class Extensions
    {
        public static JsonElement AsJsonElement(this object value)
        {
            var jsonString = JsonSerializer.Serialize(value);
            return JsonSerializer.Deserialize<JsonElement>(jsonString);
        }

        public static Mock<IObserver<T>> SubscribeMock<T>(this IObservable<T> observable)
        {
            var observerMock = new Mock<IObserver<T>>();

            observable.Subscribe(observerMock.Object);

            return observerMock;
        }
    }
}