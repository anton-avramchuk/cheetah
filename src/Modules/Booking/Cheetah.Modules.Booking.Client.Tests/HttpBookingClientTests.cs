using System.Net;
using System.Text;
using Cheetah.Modules.Booking.Client;
using Cheetah.Modules.Booking.Contracts;
using Shouldly;

namespace Cheetah.Modules.Booking.Client.Tests;

public class HttpBookingClientTests
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

    private static HttpBookingClient Client(StubHandler handler)
        => new(new HttpClient(handler) { BaseAddress = new Uri("https://booking.test/") });

    [Fact]
    public async Task CreateBookingAsync_ParsesReturnedId()
    {
        var id = Guid.NewGuid();
        var handler = new StubHandler(HttpStatusCode.Created, $"\"{id}\"");
        var client = Client(handler);

        var result = await client.CreateBookingAsync("intro",
            new CreatePublicBookingRequest(DateTimeOffset.UtcNow, "Jane", "jane@example.com", null, "UTC",
                Array.Empty<BookingAnswerDto>()));

        result.ShouldBe(id);
        handler.LastRequest!.Method.ShouldBe(HttpMethod.Post);
        handler.LastRequest.RequestUri!.AbsolutePath.ShouldEndWith("/public/booking/intro");
    }

    [Fact]
    public async Task GetSlotsAsync_ParsesAndBuildsUrl()
    {
        var json = "[{\"startUtc\":\"2026-07-01T09:00:00+00:00\",\"endUtc\":\"2026-07-01T10:00:00+00:00\"," +
                   "\"inviteeLocalTime\":\"2026-07-01T09:00:00\"}]";
        var handler = new StubHandler(HttpStatusCode.OK, json);
        var client = Client(handler);

        var slots = await client.GetSlotsAsync("intro", new DateOnly(2026, 7, 1), new DateOnly(2026, 7, 1), "UTC");

        slots.ShouldHaveSingleItem();
        slots[0].InviteeLocalTime.ShouldBe("2026-07-01T09:00:00");
        handler.LastRequest!.RequestUri!.Query.ShouldContain("from=2026-07-01");
        handler.LastRequest.RequestUri.Query.ShouldContain("tz=UTC");
    }

    [Fact]
    public async Task GetPageAsync_NotFound_ReturnsNull()
    {
        var handler = new StubHandler(HttpStatusCode.NotFound);
        var client = Client(handler);

        (await client.GetPageAsync("missing")).ShouldBeNull();
    }

    [Fact]
    public async Task CancelAsync_OnConflict_Throws()
    {
        var handler = new StubHandler(HttpStatusCode.Conflict, "{\"error\":\"x\"}");
        var client = Client(handler);

        await Should.ThrowAsync<HttpRequestException>(() => client.CancelAsync("tok", "reason").AsTask());
    }

    [Fact]
    public async Task RescheduleAsync_BuildsManageUrl()
    {
        var handler = new StubHandler(HttpStatusCode.NoContent);
        var client = Client(handler);

        await client.RescheduleAsync("tok123", DateTimeOffset.UtcNow.AddDays(1));

        handler.LastRequest!.Method.ShouldBe(HttpMethod.Post);
        handler.LastRequest.RequestUri!.AbsolutePath.ShouldEndWith("/manage/tok123/reschedule");
    }
}
