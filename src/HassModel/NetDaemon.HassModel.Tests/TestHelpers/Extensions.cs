using System;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Moq;

namespace NetDaemon.HassModel.Tests.TestHelpers;

internal static class Extensions
{
    public static JsonElement AsJsonElement(this string value)
    {
        var reader = new Utf8JsonReader(
            Encoding.UTF8.GetBytes(value));

        return JsonElement.ParseValue(ref reader);
    }

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