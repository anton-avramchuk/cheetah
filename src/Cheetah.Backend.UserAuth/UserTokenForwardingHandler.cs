using Cheetah.Core.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace Cheetah.Backend.UserAuth;

/// <summary>
/// <see cref="DelegatingHandler"/>, пробрасывающий <c>Authorization</c> заголовок текущего
/// HTTP-запроса в исходящий вызов другого сервиса (схема on-behalf-of). Целевой сервис видит
/// реального пользователя и его роли — авторизация работает «как есть».
/// </summary>
/// <remarks>
/// Работает ТОЛЬКО в контексте входящего запроса (есть <see cref="HttpContext"/>). Для фоновых
/// задач, обработчиков событий и Outbox используйте сервисный токен (Cheetah.Backend.ServiceAuth).
/// Если у исходящего запроса уже выставлен <c>Authorization</c> — он не перезаписывается.
/// </remarks>
[Export(LifetimeType.Transient, typeof(UserTokenForwardingHandler))]
public sealed class UserTokenForwardingHandler(IHttpContextAccessor httpContextAccessor) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.Headers.Authorization is null)
        {
            var incoming = httpContextAccessor.HttpContext?.Request.Headers[HeaderNames.Authorization].ToString();
            if (!string.IsNullOrWhiteSpace(incoming))
                request.Headers.TryAddWithoutValidation(HeaderNames.Authorization, incoming);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
