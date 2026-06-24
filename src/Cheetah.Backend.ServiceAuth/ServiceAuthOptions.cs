namespace Cheetah.Backend.ServiceAuth;

/// <summary>
/// Настройки получения сервисного токена (machine-to-machine) у Identity.
/// Биндится из секции <c>ServiceAuth</c>.
/// </summary>
public sealed class ServiceAuthOptions
{
    public const string SectionName = "ServiceAuth";

    /// <summary>Абсолютный URL эндпоинта выпуска токена, например <c>https://identity/api/auth/service-token</c>.</summary>
    public string TokenEndpoint { get; set; } = string.Empty;

    /// <summary>Идентификатор этого сервиса как клиента Identity.</summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>Секрет этого сервиса.</summary>
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>
    /// Запас перед истечением токена: токен обновляется за это время до реального expiry,
    /// чтобы не словить 401 на границе. Default = 30 секунд.
    /// </summary>
    public TimeSpan RefreshSkew { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>Тайм-аут запроса токена. Default = 5 секунд.</summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(5);
}
