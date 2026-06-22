using System.Security.Claims;

namespace Cheetah.AspNetCore.Blazor.Auth.Abstractions;

/// <summary>Учётные данные для входа, переданные из формы логина.</summary>
public sealed record BffCredentials(string UserName, string Password);

/// <summary>Набор токенов пользователя для вызова микросервисов.</summary>
/// <param name="AccessToken">JWT access-токен (Bearer).</param>
/// <param name="RefreshToken">Refresh-токен для обновления; <c>null</c>, если не используется.</param>
/// <param name="ExpiresAt">Момент истечения access-токена (UTC).</param>
public sealed record BffTokenSet(string AccessToken, string? RefreshToken, DateTimeOffset ExpiresAt);

/// <summary>Профиль аутентифицированного пользователя для построения cookie-principal.</summary>
public sealed record BffUser(
    string Id,
    string UserName,
    string? Email = null,
    IReadOnlyCollection<string>? Roles = null,
    IReadOnlyCollection<Claim>? AdditionalClaims = null);

/// <summary>Результат аутентификации, возвращаемый <see cref="IBffAuthenticator"/>.</summary>
public sealed record BffAuthResult
{
    public bool Succeeded { get; private init; }
    public string? Error { get; private init; }
    public BffUser? User { get; private init; }
    public BffTokenSet? Tokens { get; private init; }

    public static BffAuthResult Success(BffUser user, BffTokenSet tokens)
        => new() { Succeeded = true, User = user, Tokens = tokens };

    public static BffAuthResult Fail(string error)
        => new() { Succeeded = false, Error = error };
}
