using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Backend.UserAuth;

public static class UserAuthHttpClientExtensions
{
    /// <summary>
    /// Добавляет к <see cref="HttpClient"/> проброс <c>Authorization</c> заголовка текущего
    /// пользователя (<see cref="UserTokenForwardingHandler"/>, схема on-behalf-of).
    /// </summary>
    public static IHttpClientBuilder AddUserTokenForwarding(this IHttpClientBuilder builder)
        => builder.AddHttpMessageHandler<UserTokenForwardingHandler>();
}
