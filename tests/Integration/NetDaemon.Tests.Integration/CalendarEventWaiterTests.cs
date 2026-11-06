using System.Text.Json;
using FluentAssertions;
using NetDaemon.Client;
using NetDaemon.Client.HomeAssistant.Model;
using NetDaemon.Client.Internal.HomeAssistant.Commands;
using NetDaemon.Tests.Integration.Helpers;
using Xunit;

namespace NetDaemon.Tests.Integration;

public class CalendarEventWaiterTests
{
    [Fact]
    public async Task WaitForEventAsync_ShouldRetryUntilExpectedEventIsReturned()
    {
        var connection = new StubHomeAssistantConnection(
            [
                CreateResult("Other", "Different"),
                CreateResult("Expected", "Description")
            ]);

        await CalendarEventWaiter.WaitForEventAsync(
            connection,
            "Expected",
            "Description",
            TimeSpan.FromMilliseconds(200),
            TimeSpan.FromMilliseconds(10));

        connection.CallCount.Should().Be(2);
    }

    [Fact]
    public async Task WaitForEventAsync_ShouldThrowTimeoutException_WhenExpectedEventIsNotReturned()
    {
        var connection = new StubHomeAssistantConnection(
            [
                CreateResult("Other", "Different")
            ]);

        var act = async () => await CalendarEventWaiter.WaitForEventAsync(
            connection,
            "Expected",
            "Description",
            TimeSpan.FromMilliseconds(50),
            TimeSpan.FromMilliseconds(10));

        await act.Should().ThrowAsync<TimeoutException>();
    }

    private static HassServiceResult CreateResult(string summary, string description)
    {
        return new HassServiceResult
        {
            Response = JsonSerializer.SerializeToElement(new Dictionary<string, object>
            {
                ["calendar.cal"] = new
                {
                    events = new[]
                    {
                        new
                        {
                            start = "2023-07-21T22:00:00+00:00",
                            end = "2023-07-21T23:00:00+00:00",
                            summary,
                            description
                        }
                    }
                }
            })
        };
    }

    private sealed class StubHomeAssistantConnection(IReadOnlyList<HassServiceResult> results) : IHomeAssistantConnection
    {
        private int _index;

        public int CallCount { get; private set; }

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;

        public Task<T?> GetApiCallAsync<T>(string apiPath, CancellationToken cancelToken) => throw new NotImplementedException();

        public Task<T?> PostApiCallAsync<T>(string apiPath, CancellationToken cancelToken, object? data = null) => throw new NotImplementedException();

        public Task<IObservable<HassEvent>> SubscribeToHomeAssistantEventsAsync(string? eventType, CancellationToken cancelToken) => throw new NotImplementedException();

        public Task SendCommandAsync<T>(T command, CancellationToken cancelToken) where T : CommandMessage => throw new NotImplementedException();

        public Task<TResult?> SendCommandAndReturnResponseAsync<T, TResult>(T command, CancellationToken cancelToken) where T : CommandMessage
        {
            CallCount++;

            if (cancelToken.IsCancellationRequested)
            {
                return Task.FromCanceled<TResult?>(cancelToken);
            }

            if (typeof(TResult) == typeof(HassServiceResult))
            {
                var result = results[Math.Min(_index, results.Count - 1)];
                _index++;
                return Task.FromResult((TResult?)(object?)result);
            }

            throw new NotSupportedException();
        }

        public Task<JsonElement?> SendCommandAndReturnResponseRawAsync<T>(T command, CancellationToken cancelToken) where T : CommandMessage => throw new NotImplementedException();

        public Task<HassMessage?> SendCommandAndReturnHassMessageResponseAsync<T>(T command, CancellationToken cancelToken) where T : CommandMessage => throw new NotImplementedException();

        public Task WaitForConnectionToCloseAsync(CancellationToken cancelToken) => throw new NotImplementedException();
    }
}
