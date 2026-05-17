using System.Net;
using System.Text;
using Cheetah.FileStorage;
using Cheetah.FileStorage.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;

namespace Cheetah.FileStorage.Tests;

public class HttpFileStorageTests
{
    private static HttpFileStorage Build(Func<HttpRequestMessage, HttpResponseMessage> respond)
    {
        var handler = new StubHandler(respond);
        var factory = new SingleClientFactory(handler);
        return new HttpFileStorage(
            factory,
            Microsoft.Extensions.Options.Options.Create(new HttpFileStorageOptions
            {
                BaseUrl = "https://storage.example",
                ApiKey = "tok"
            }),
            NullLogger<HttpFileStorage>.Instance);
    }

    [Fact]
    public async Task SaveAsync_PUT_на_правильный_url_с_Bearer_и_Content_Type()
    {
        HttpRequestMessage? captured = null;
        var sut = Build(req => { captured = req; return new HttpResponseMessage(HttpStatusCode.OK); });

        await sut.SaveAsync("docs/x.txt", new MemoryStream(Encoding.UTF8.GetBytes("hi")), "text/plain",
            new Dictionary<string, string> { ["author"] = "ant" });

        captured.ShouldNotBeNull();
        captured!.Method.ShouldBe(HttpMethod.Put);
        captured.RequestUri!.ToString().ShouldBe("https://storage.example/files/docs/x.txt");
        captured.Headers.Authorization!.Scheme.ShouldBe("Bearer");
        captured.Headers.Authorization.Parameter.ShouldBe("tok");
        captured.Headers.TryGetValues("X-Meta-author", out var meta).ShouldBeTrue();
        meta!.First().ShouldBe("ant");
        captured.Content!.Headers.ContentType!.MediaType.ShouldBe("text/plain");
    }

    [Fact]
    public async Task OpenReadAsync_404_бросает_FileStorageNotFoundException()
    {
        var sut = Build(_ => new HttpResponseMessage(HttpStatusCode.NotFound));
        await Should.ThrowAsync<FileStorageNotFoundException>(() => sut.OpenReadAsync("k").AsTask());
    }

    [Fact]
    public async Task OpenReadAsync_возвращает_содержимое()
    {
        var sut = Build(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(Encoding.UTF8.GetBytes("payload"))
        });

        await using var stream = await sut.OpenReadAsync("k");
        using var reader = new StreamReader(stream);
        (await reader.ReadToEndAsync()).ShouldBe("payload");
    }

    [Fact]
    public async Task DeleteAsync_404_не_бросает()
    {
        var sut = Build(_ => new HttpResponseMessage(HttpStatusCode.NotFound));
        await Should.NotThrowAsync(() => sut.DeleteAsync("missing").AsTask());
    }

    [Fact]
    public async Task ExistsAsync_по_HEAD_200_true_404_false()
    {
        var sutOk = Build(req =>
        {
            req.Method.ShouldBe(HttpMethod.Head);
            return new HttpResponseMessage(HttpStatusCode.OK);
        });
        (await sutOk.ExistsAsync("k")).ShouldBeTrue();

        var sutNo = Build(_ => new HttpResponseMessage(HttpStatusCode.NotFound));
        (await sutNo.ExistsAsync("k")).ShouldBeFalse();
    }

    [Fact]
    public async Task GetMetadataAsync_возвращает_null_при_404()
    {
        var sut = Build(_ => new HttpResponseMessage(HttpStatusCode.NotFound));
        (await sut.GetMetadataAsync("k")).ShouldBeNull();
    }

    [Fact]
    public async Task GetMetadataAsync_парсит_JSON()
    {
        var sut = Build(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                """{"size":42,"contentType":"text/plain","lastModified":"2026-05-17T10:00:00Z"}""",
                Encoding.UTF8, "application/json")
        });

        var meta = await sut.GetMetadataAsync("k");
        meta.ShouldNotBeNull();
        meta!.Size.ShouldBe(42);
        meta.ContentType.ShouldBe("text/plain");
    }

    [Fact]
    public void Ctor_бросает_при_пустом_BaseUrl()
    {
        Should.Throw<InvalidOperationException>(() => new HttpFileStorage(
            new SingleClientFactory(new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.OK))),
            Microsoft.Extensions.Options.Options.Create(new HttpFileStorageOptions { BaseUrl = "" }),
            NullLogger<HttpFileStorage>.Instance));
    }

    private sealed class StubHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _respond;
        public StubHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) => _respond = respond;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(_respond(request));
    }

    private sealed class SingleClientFactory : IHttpClientFactory
    {
        private readonly HttpMessageHandler _handler;
        public SingleClientFactory(HttpMessageHandler handler) => _handler = handler;
        public HttpClient CreateClient(string name) => new(_handler, disposeHandler: false);
    }
}
