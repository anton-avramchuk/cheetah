using Cheetah.AspNetCore.Blazor.Auth.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.AspNetCore.Blazor.Auth;

public static class BffHttpClientExtensions
{
    /// <summary>
    /// Регистрирует именованный <see cref="HttpClient"/> для вызова микросервиса с
    /// автоматической подстановкой Bearer-токена текущего пользователя (<see cref="BearerTokenHandler"/>).
    /// </summary>
    public static IHttpClientBuilder AddBffHttpClient(
        this IServiceCollection services,
        string name,
        Action<HttpClient>? configureClient = null)
    {
        var builder = configureClient is null
            ? services.AddHttpClient(name)
            : services.AddHttpClient(name, configureClient);

        return builder.AddHttpMessageHandler<BearerTokenHandler>();
    }

    /// <summary>
    /// Добавляет <see cref="BearerTokenHandler"/> к уже сконфигурированному <see cref="IHttpClientBuilder"/>
    /// (например, для типизированного клиента).
    /// </summary>
    public static IHttpClientBuilder AddBffBearerToken(this IHttpClientBuilder builder)
        => builder.AddHttpMessageHandler<BearerTokenHandler>();
}
