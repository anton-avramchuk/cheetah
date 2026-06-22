using System.Net;
using System.Net.Http.Headers;
using Cheetah.AspNetCore.Blazor.Auth.Tokens;
using Cheetah.Core.DependencyInjection;

namespace Cheetah.AspNetCore.Blazor.Auth.Http;

/// <summary>
/// DelegatingHandler, подставляющий <c>Authorization: Bearer &lt;jwt&gt;</c> в исходящие
/// запросы к микросервисам. При 401 пробует обновить токен через refresh и повторяет запрос один раз.
/// </summary>
[Export(LifetimeType.Transient, typeof(BearerTokenHandler))]
public sealed class BearerTokenHandler(IAccessTokenProvider tokenProvider) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        // Буферизуем тело заранее, чтобы при 401 можно было безопасно повторить запрос.
        byte[]? body = request.Content is null ? null : await request.Content.ReadAsByteArrayAsync(ct);
        var contentType = request.Content?.Headers.ContentType;

        var token = await tokenProvider.GetAccessTokenAsync(ct);
        if (token is not null)
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await base.SendAsync(request, ct);

        if (response.StatusCode != HttpStatusCode.Unauthorized || token is null)
            return response;

        var refreshed = await tokenProvider.RefreshAccessTokenAsync(ct);
        if (refreshed is null)
            return response;

        response.Dispose();

        var retry = Clone(request, body, contentType);
        retry.Headers.Authorization = new AuthenticationHeaderValue("Bearer", refreshed);
        return await base.SendAsync(retry, ct);
    }

    private static HttpRequestMessage Clone(HttpRequestMessage request, byte[]? body, MediaTypeHeaderValue? contentType)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri) { Version = request.Version };

        foreach (var header in request.Headers)
        {
            if (header.Key == "Authorization")
                continue;
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        foreach (var option in request.Options)
            ((IDictionary<string, object?>)clone.Options)[option.Key] = option.Value;

        if (body is not null)
        {
            clone.Content = new ByteArrayContent(body);
            if (contentType is not null)
                clone.Content.Headers.ContentType = contentType;
        }

        return clone;
    }
}
