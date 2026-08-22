using System.Net;
using System.Text;
using Cheetah.Modules.Notes.Contracts;
using Cheetah.Modules.Notes.Shared;
using Shouldly;

namespace Cheetah.Modules.Notes.Client.Tests;

public sealed record TestCreateRequest : CreateNoteRequestBase;

public sealed record TestUpdateRequest : UpdateNoteRequestBase;

public sealed record TestNoteDto : NoteDtoBase;

public class HttpNotesClientTests
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

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(new HttpResponseMessage(_status)
            {
                Content = new StringContent(_json, Encoding.UTF8, "application/json")
            });
        }
    }

    private static HttpNotesClient<TestCreateRequest, TestUpdateRequest, TestNoteDto> Client(StubHandler handler)
        => new(new HttpClient(handler) { BaseAddress = new Uri("https://notes.test/") });

    [Fact]
    public async Task CreateAsync_ParsesReturnedId()
    {
        var id = Guid.NewGuid();
        var handler = new StubHandler(HttpStatusCode.Created, $$"""{"id":"{{id}}"}""");

        var result = await Client(handler).CreateAsync(new TestCreateRequest
        {
            EntityType = EntityRefKeys.Deal,
            EntityId = Guid.NewGuid(),
            AuthorId = Guid.NewGuid(),
            Body = "hello"
        });

        result.ShouldBe(id);
        handler.LastRequest!.Method.ShouldBe(HttpMethod.Post);
        handler.LastRequest.RequestUri!.AbsolutePath.ShouldBe("/api/notes");
    }

    [Fact]
    public async Task GetByIdAsync_NotFound_ReturnsNull()
    {
        var handler = new StubHandler(HttpStatusCode.NotFound);

        (await Client(handler).GetByIdAsync(Guid.NewGuid())).ShouldBeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ParsesDto()
    {
        var id = Guid.NewGuid();
        var handler = new StubHandler(HttpStatusCode.OK,
            $$"""{"id":"{{id}}","entityType":"crm.deal","body":"hi"}""");

        var dto = await Client(handler).GetByIdAsync(id);

        dto.ShouldNotBeNull();
        dto!.Id.ShouldBe(id);
        dto.Body.ShouldBe("hi");
    }

    [Fact]
    public async Task GetByEntityAsync_BuildsQueryString()
    {
        var entityId = Guid.NewGuid();
        var handler = new StubHandler(HttpStatusCode.OK, "[]");

        var result = await Client(handler).GetByEntityAsync(EntityRefKeys.Customer, entityId,
            pinnedOnly: true, skip: 10, take: 20);

        result.ShouldBeEmpty();
        var query = handler.LastRequest!.RequestUri!.Query;
        query.ShouldContain("entityType=crm.customer");
        query.ShouldContain($"entityId={entityId}");
        query.ShouldContain("pinnedOnly=true");
        query.ShouldContain("skip=10");
        query.ShouldContain("take=20");
    }

    [Fact]
    public async Task GetByEntityAsync_DefaultTake_FallsBackToDefaultPageSize()
    {
        var handler = new StubHandler(HttpStatusCode.OK, "[]");

        await Client(handler).GetByEntityAsync(EntityRefKeys.Deal, Guid.NewGuid());

        handler.LastRequest!.RequestUri!.Query.ShouldContain($"take={NotesConstants.DefaultPageSize}");
    }

    [Fact]
    public async Task CreateAsync_EmptyBody_Throws()
    {
        var handler = new StubHandler(HttpStatusCode.Created, "");

        await Should.ThrowAsync<HttpRequestException>(async () =>
            await Client(handler).CreateAsync(new TestCreateRequest
            {
                EntityType = EntityRefKeys.Deal,
                EntityId = Guid.NewGuid(),
                AuthorId = Guid.NewGuid(),
                Body = "hello"
            }));
    }

    [Fact]
    public async Task GetRepliesAsync_HitsThreadRoute_WithPaging()
    {
        var id = Guid.NewGuid();
        var handler = new StubHandler(HttpStatusCode.OK, "[]");

        await Client(handler).GetRepliesAsync(id, skip: 20, take: 5);

        handler.LastRequest!.RequestUri!.AbsolutePath.ShouldBe($"/api/notes/{id}/replies");
        handler.LastRequest.RequestUri.Query.ShouldContain("skip=20");
        handler.LastRequest.RequestUri.Query.ShouldContain("take=5");
    }

    [Fact]
    public async Task GetRepliesAsync_DefaultTake_FallsBackToDefaultPageSize()
    {
        var handler = new StubHandler(HttpStatusCode.OK, "[]");

        await Client(handler).GetRepliesAsync(Guid.NewGuid());

        handler.LastRequest!.RequestUri!.Query.ShouldContain($"take={NotesConstants.DefaultPageSize}");
    }

    [Fact]
    public async Task PinAsync_PostsToPinRoute()
    {
        var id = Guid.NewGuid();
        var handler = new StubHandler(HttpStatusCode.NoContent);

        await Client(handler).PinAsync(id);

        handler.LastRequest!.Method.ShouldBe(HttpMethod.Post);
        handler.LastRequest.RequestUri!.AbsolutePath.ShouldBe($"/api/notes/{id}/pin");
    }

    [Fact]
    public async Task RemoveAsync_SendsDelete()
    {
        var id = Guid.NewGuid();
        var handler = new StubHandler(HttpStatusCode.NoContent);

        await Client(handler).RemoveAsync(id);

        handler.LastRequest!.Method.ShouldBe(HttpMethod.Delete);
        handler.LastRequest.RequestUri!.AbsolutePath.ShouldBe($"/api/notes/{id}");
    }

    [Fact]
    public async Task UpdateAsync_SendsPut()
    {
        var id = Guid.NewGuid();
        var handler = new StubHandler(HttpStatusCode.NoContent);

        await Client(handler).UpdateAsync(id, new TestUpdateRequest { Id = id, Body = "edited" });

        handler.LastRequest!.Method.ShouldBe(HttpMethod.Put);
        handler.LastRequest.RequestUri!.AbsolutePath.ShouldBe($"/api/notes/{id}");
    }

    [Fact]
    public async Task FailedRequest_ThrowsWithStatusCode()
    {
        var handler = new StubHandler(HttpStatusCode.BadRequest, """{"error":"body is required"}""");

        var ex = await Should.ThrowAsync<HttpRequestException>(async () =>
            await Client(handler).RemoveAsync(Guid.NewGuid()));

        ex.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        ex.Message.ShouldContain("body is required");
    }
}
