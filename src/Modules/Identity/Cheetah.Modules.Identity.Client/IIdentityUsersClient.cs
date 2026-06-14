using Cheetah.Modules.Identity.Contracts.Response;

namespace Cheetah.Modules.Identity.Client;

/// <summary>
/// HTTP-клиент к Identity API для server-to-server интеграции. Используется другими
/// .NET-модулями/сервисами, которым нужны данные пользователей Identity.
/// </summary>
public interface IIdentityUsersClient
{
    /// <summary>Получить полный список пользователей (без пагинации).</summary>
    ValueTask<IReadOnlyList<UserGridViewModel>> GetUsersAsync(CancellationToken ct = default);
}
