namespace Cheetah.AspNetCore.Blazor.Auth;

/// <summary>Типы claim'ов, специфичные для BFF-аутентификации.</summary>
public static class BffClaimTypes
{
    /// <summary>
    /// Идентификатор серверной сессии. Кладётся в cookie-principal при входе и используется
    /// как ключ доступа к токенам пользователя в <see cref="Tokens.IUserTokenStore"/> (внутри
    /// Blazor-контура HttpContext недоступен, поэтому токены достаются по этому claim'у).
    /// </summary>
    public const string SessionId = "bff_session_id";
}
