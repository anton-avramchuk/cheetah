namespace Cheetah.Modules.Identity.Client;

/// <summary>
/// Снимок пользователя Identity для server-to-server сценариев. Конкретный (неабстрактный)
/// transport-DTO: несёт только базовые поля, поэтому потребителям (Teams/Tags и т.п.) не нужно
/// знать расширенную grid-ViewModel конкретного хоста — лишние поля JSON просто игнорируются.
/// </summary>
public sealed record IdentityUserSummary(Guid Id, string UserName, string Email);

/// <summary>
/// HTTP-клиент к Identity API для server-to-server интеграции. Используется другими
/// .NET-модулями/сервисами, которым нужны данные пользователей Identity.
/// </summary>
public interface IIdentityUsersClient
{
    /// <summary>Получить полный список пользователей (без пагинации).</summary>
    ValueTask<IReadOnlyList<IdentityUserSummary>> GetUsersAsync(CancellationToken ct = default);
}
