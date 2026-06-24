using System.Net.Http.Headers;
using Cheetah.Core.DependencyInjection;

namespace Cheetah.Backend.ServiceAuth;

/// <summary>
/// <see cref="DelegatingHandler"/>, подставляющий действующий сервисный токен в заголовок
/// <c>Authorization: Bearer</c> исходящих запросов. Вешается на типизированные/именованные
/// <see cref="HttpClient"/> клиентов через <c>.AddHttpMessageHandler&lt;ServiceTokenHandler&gt;()</c>.
/// </summary>
[Export(LifetimeType.Transient, typeof(ServiceTokenHandler))]
public sealed class ServiceTokenHandler(IServiceTokenProvider tokenProvider) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await tokenProvider.GetTokenAsync(cancellationToken);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await base.SendAsync(request, cancellationToken);
    }
}
