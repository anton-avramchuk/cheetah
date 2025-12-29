using System.Net;
using System.Text;
using System.Text.Json;

namespace Cheetah.Tenants.ApiClient.Tests.TestHelpers;

/// <summary>
/// Mock HttpMessageHandler for testing HTTP client calls.
/// </summary>
public class HttpMessageHandlerMock : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _sendAsyncFunc;

    public HttpMessageHandlerMock(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> sendAsyncFunc)
    {
        _sendAsyncFunc = sendAsyncFunc;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return _sendAsyncFunc(request, cancellationToken);
    }

    /// <summary>
    /// Creates a successful response with JSON content.
    /// </summary>
    public static HttpResponseMessage CreateJsonResponse<T>(T content, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        var json = JsonSerializer.Serialize(content);
        return new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
    }

    /// <summary>
    /// Creates a response with no content.
    /// </summary>
    public static HttpResponseMessage CreateEmptyResponse(HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        return new HttpResponseMessage(statusCode);
    }
}
