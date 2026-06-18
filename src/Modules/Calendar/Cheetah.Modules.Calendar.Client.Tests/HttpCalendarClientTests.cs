using System.Net;
using System.Text;
using Cheetah.Modules.Calendar.Client;
using Cheetah.Modules.Calendar.Contracts;
using Cheetah.Modules.Calendar.Shared;
using Shouldly;

namespace Cheetah.Modules.Calendar.Client.Tests;

public class HttpCalendarClientTests
{
    private sealed class StubHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _status;
        private readonly string _json;
        public HttpRequestMessage? LastRequest { get; private set; }

        public StubHandler(HttpStatusCode status, string json = "")
        {
            _status = status;
            _json = json;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(new HttpResponseMessage(_status)
            {
                Content = new StringContent(_json, Encoding.UTF8, "application/json")
            });
        }
    }

    private static HttpCalendarClient Client(StubHandler handler)
        => new(new HttpClient(handler) { BaseAddress = new Uri("https://calendar.test/") });

    [Fact]
    public async Task CreateEventAsync_ParsesReturnedId()
    {
        var id = Guid.NewGuid();
        var handler = new StubHandler(HttpStatusCode.Created, $"{{\"id\":\"{id}\"}}");
        var client = Client(handler);

        var calendarId = Guid.NewGuid();
        var result = await client.CreateEventAsync(
            calendarId,
            new CreateEventRequest(calendarId, "M", DateTime.UtcNow, DateTime.UtcNow.AddHours(1), Guid.NewGuid()));

        result.ShouldBe(id);
        handler.LastRequest!.Method.ShouldBe(HttpMethod.Post);
        handler.LastRequest.RequestUri!.AbsolutePath.ShouldEndWith("/events");
    }

    [Fact]
    public async Task GetByEntityAsync_ParsesOccurrences()
    {
        var json = "[{\"eventId\":\"" + Guid.NewGuid() +
                   "\",\"title\":\"M\",\"location\":null,\"startUtc\":\"2026-01-05T09:00:00Z\"," +
                   "\"endUtc\":\"2026-01-05T09:30:00Z\",\"isAllDay\":false,\"occurrenceKey\":\"K\",\"isOverride\":false}]";
        var handler = new StubHandler(HttpStatusCode.OK, json);
        var client = Client(handler);

        var items = await client.GetByEntityAsync("crm.deal", Guid.NewGuid(), DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(1));

        items.ShouldHaveSingleItem();
        items[0].Title.ShouldBe("M");
        handler.LastRequest!.RequestUri!.AbsolutePath.ShouldContain("by-entity");
    }

    [Fact]
    public async Task GetUserBusyAsync_ParsesIntervalsAndBuildsUrl()
    {
        var json = "[{\"startUtc\":\"2026-01-05T09:00:00Z\",\"endUtc\":\"2026-01-05T10:00:00Z\"}," +
                   "{\"startUtc\":\"2026-01-05T11:00:00Z\",\"endUtc\":\"2026-01-05T11:30:00Z\"}]";
        var handler = new StubHandler(HttpStatusCode.OK, json);
        var client = Client(handler);

        var hostId = Guid.NewGuid();
        var items = await client.GetUserBusyAsync(
            hostId, DateTimeOffset.Parse("2026-01-05T00:00:00Z"), DateTimeOffset.Parse("2026-01-06T00:00:00Z"));

        items.Count.ShouldBe(2);
        items[0].StartUtc.ShouldBe(new DateTime(2026, 1, 5, 9, 0, 0, DateTimeKind.Utc));
        items[1].EndUtc.ShouldBe(new DateTime(2026, 1, 5, 11, 30, 0, DateTimeKind.Utc));
        handler.LastRequest!.Method.ShouldBe(HttpMethod.Get);
        handler.LastRequest.RequestUri!.AbsolutePath.ShouldContain($"users/{hostId}/busy");
        handler.LastRequest.RequestUri.Query.ShouldContain("from=");
        handler.LastRequest.RequestUri.Query.ShouldContain("to=");
    }

    [Fact]
    public async Task GetUserBusyAsync_OnError_Throws()
    {
        var handler = new StubHandler(HttpStatusCode.InternalServerError, "boom");
        var client = Client(handler);

        await Should.ThrowAsync<HttpRequestException>(() =>
            client.GetUserBusyAsync(Guid.NewGuid(), DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(1)).AsTask());
    }

    [Fact]
    public async Task RescheduleEventAsync_PostsToRescheduleRoute()
    {
        var handler = new StubHandler(HttpStatusCode.NoContent);
        var client = Client(handler);
        var eventId = Guid.NewGuid();

        await client.RescheduleEventAsync(eventId,
            DateTimeOffset.Parse("2026-02-01T09:00:00Z"), DateTimeOffset.Parse("2026-02-01T10:00:00Z"));

        handler.LastRequest!.Method.ShouldBe(HttpMethod.Post);
        handler.LastRequest.RequestUri!.AbsolutePath.ShouldEndWith($"/calendar-events/{eventId}/reschedule");
    }

    [Fact]
    public async Task CancelEventAsync_DeletesEvent()
    {
        var handler = new StubHandler(HttpStatusCode.NoContent);
        var client = Client(handler);
        var eventId = Guid.NewGuid();

        await client.CancelEventAsync(eventId);

        handler.LastRequest!.Method.ShouldBe(HttpMethod.Delete);
        handler.LastRequest.RequestUri!.AbsolutePath.ShouldEndWith($"/calendar-events/{eventId}");
    }

    [Fact]
    public async Task SyncRegistryAsync_OnError_Throws()
    {
        var handler = new StubHandler(HttpStatusCode.BadRequest, "bad");
        var client = Client(handler);

        await Should.ThrowAsync<HttpRequestException>(() =>
            client.SyncRegistryAsync(new CalendarRegistrySyncRequest("svc",
                new[] { new CalendarableEntityTypeRegistration("crm.deal", "Deal") })).AsTask());
    }
}
