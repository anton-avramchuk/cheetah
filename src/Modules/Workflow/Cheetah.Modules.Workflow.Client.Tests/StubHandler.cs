namespace Cheetah.Modules.Workflow.Client.Tests;

/// <summary>Перехватчик HTTP для проверки запросов клиента без реальной сети.</summary>
internal sealed class StubHandler(Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> responder)
    : HttpMessageHandler
{
    public HttpRequestMessage? LastRequest { get; private set; }
    public string? LastBody { get; private set; }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        LastRequest = request;
        if (request.Content is not null)
            LastBody = await request.Content.ReadAsStringAsync(cancellationToken);
        return responder(request, cancellationToken);
    }
}
