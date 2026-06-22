namespace Cheetah.AspNetCore.Blazor.Auth.Abstractions;

/// <summary>
/// Шов аутентификации BFF — единственная точка, которую реализует приложение.
/// Одна реализация может проверять учётные данные и выдавать/получать JWT по-разному:
/// ходить в удалённый Identity-микросервис, в локальную БД или к внешнему провайдеру
/// (Keycloak/OIDC). Фреймворк не привязан к конкретной схеме.
/// </summary>
public interface IBffAuthenticator
{
    /// <summary>Проверяет учётные данные и возвращает профиль пользователя + набор токенов.</summary>
    Task<BffAuthResult> AuthenticateAsync(BffCredentials credentials, CancellationToken ct = default);

    /// <summary>
    /// Обновляет набор токенов по refresh-токену. Возвращает <c>null</c>, если обновление
    /// невозможно (refresh истёк/отозван) — в этом случае пользователь должен войти заново.
    /// </summary>
    Task<BffTokenSet?> RefreshAsync(string refreshToken, CancellationToken ct = default);
}
