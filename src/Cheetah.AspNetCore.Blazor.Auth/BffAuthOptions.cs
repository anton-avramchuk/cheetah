namespace Cheetah.AspNetCore.Blazor.Auth;

/// <summary>Настройки BFF-аутентификации (секция <c>BffAuth</c> в конфигурации).</summary>
public sealed class BffAuthOptions
{
    public const string SectionName = "BffAuth";

    /// <summary>Путь страницы входа (куда редиректит при 401/Challenge).</summary>
    public string LoginPath { get; set; } = "/login";

    /// <summary>Путь страницы «доступ запрещён».</summary>
    public string AccessDeniedPath { get; set; } = "/access-denied";

    /// <summary>Имя auth-cookie.</summary>
    public string CookieName { get; set; } = "cheetah.bff.auth";

    /// <summary>Время жизни cookie-сессии.</summary>
    public TimeSpan ExpireTimeSpan { get; set; } = TimeSpan.FromHours(8);

    /// <summary>Скользящее продление cookie.</summary>
    public bool SlidingExpiration { get; set; } = true;

    /// <summary>
    /// Запас (сек) до истечения access-токена, при котором он считается «протухшим» и
    /// обновляется заранее. Защищает от гонки на границе срока.
    /// </summary>
    public int RefreshSkewSeconds { get; set; } = 30;

    /// <summary>Интервал ревалидации auth-state (проверки, что сессия ещё жива в сторе).</summary>
    public TimeSpan RevalidationInterval { get; set; } = TimeSpan.FromMinutes(5);
}
