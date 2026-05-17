using System.Net;
using System.Net.Http.Json;
using Cheetah.Notifications;
using Cheetah.Notifications.Sms;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;

namespace Cheetah.Notifications.Tests;

public class HttpSmsSenderTests
{
    [Fact]
    public async Task SendAsync_шлёт_POST_с_to_и_body_и_Bearer_если_ApiKey_задан()
    {
        var handler = new RecordingHandler(HttpStatusCode.OK);
        var factory = new TestHttpClientFactory(handler);
        var sut = new HttpSmsSender(
            factory,
            Microsoft.Extensions.Options.Options.Create(new SmsOptions
            {
                ProviderUrl = "https://sms.example/send",
                ApiKey = "secret-token"
            }),
            NullLogger<HttpSmsSender>.Instance);

        await sut.SendAsync(new SmsMessage("+79991234567", "hello"));

        handler.LastRequest.ShouldNotBeNull();
        handler.LastRequest!.Method.ShouldBe(HttpMethod.Post);
        handler.LastRequest.RequestUri!.ToString().ShouldBe("https://sms.example/send");
        handler.LastRequest.Headers.Authorization!.Scheme.ShouldBe("Bearer");
        handler.LastRequest.Headers.Authorization.Parameter.ShouldBe("secret-token");

        var body = await handler.LastRequest.Content!.ReadFromJsonAsync<SmsPayload>();
        body!.to.ShouldBe("+79991234567");
        body.body.ShouldBe("hello");
    }

    [Fact]
    public async Task SendAsync_бросает_если_ProviderUrl_пустой()
    {
        var sut = new HttpSmsSender(
            new TestHttpClientFactory(new RecordingHandler(HttpStatusCode.OK)),
            Microsoft.Extensions.Options.Options.Create(new SmsOptions { ProviderUrl = "" }),
            NullLogger<HttpSmsSender>.Instance);

        await Should.ThrowAsync<InvalidOperationException>(
            () => sut.SendAsync(new SmsMessage("+7", "x")).AsTask());
    }

    [Fact]
    public async Task SendAsync_бросает_при_не_2xx_ответе()
    {
        var sut = new HttpSmsSender(
            new TestHttpClientFactory(new RecordingHandler(HttpStatusCode.InternalServerError)),
            Microsoft.Extensions.Options.Options.Create(new SmsOptions { ProviderUrl = "https://sms.example/send" }),
            NullLogger<HttpSmsSender>.Instance);

        await Should.ThrowAsync<HttpRequestException>(
            () => sut.SendAsync(new SmsMessage("+7", "x")).AsTask());
    }

    private record SmsPayload(string to, string body);

    private sealed class RecordingHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _code;
        public HttpRequestMessage? LastRequest;

        public RecordingHandler(HttpStatusCode code) => _code = code;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Сохраняем копию: оригинал будет disposed после возврата.
            LastRequest = new HttpRequestMessage(request.Method, request.RequestUri);
            foreach (var h in request.Headers) LastRequest.Headers.TryAddWithoutValidation(h.Key, h.Value);
            if (request.Content != null)
            {
                var bytes = await request.Content.ReadAsByteArrayAsync(cancellationToken);
                LastRequest.Content = new ByteArrayContent(bytes);
                foreach (var h in request.Content.Headers) LastRequest.Content.Headers.TryAddWithoutValidation(h.Key, h.Value);
            }
            return new HttpResponseMessage(_code);
        }
    }

    private sealed class TestHttpClientFactory : IHttpClientFactory
    {
        private readonly HttpMessageHandler _handler;
        public TestHttpClientFactory(HttpMessageHandler handler) => _handler = handler;
        public HttpClient CreateClient(string name) => new(_handler, disposeHandler: false);
    }
}
