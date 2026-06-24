namespace Cheetah.Modules.Identity.Application.Services;

/// <summary>Аутентифицированный сервисный клиент (machine-to-machine).</summary>
public sealed record ServiceClientPrincipal(string ClientId, IReadOnlyCollection<string> Roles);

/// <summary>
/// Проверяет учётные данные сервисного клиента (client_credentials) и возвращает его роли.
/// Реализация (например, на основе конфигурации) живёт в Api/Infrastructure.
/// </summary>
public interface IServiceClientAuthenticator
{
    /// <summary>Возвращает principal при валидных учётных данных, иначе <c>null</c>.</summary>
    ServiceClientPrincipal? Authenticate(string clientId, string clientSecret);
}
