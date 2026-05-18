namespace Cheetah.Permissions.Catalog.Client;

/// <summary>
/// HTTP-клиент к Permissions.Catalog.Api. Используется микросервисами для
/// регистрации собственных permissions в общем каталоге при старте.
/// </summary>
public interface IPermissionsCatalogClient
{
    /// <summary>
    /// Отправить набор permissions модуля в каталог. Идемпотентно: повторный вызов
    /// обновляет описания, не удаляет существующие.
    /// </summary>
    ValueTask SyncAsync(RegistrySyncRequest request, CancellationToken ct = default);

    /// <summary>Получить список всех permissions из каталога.</summary>
    ValueTask<IReadOnlyList<PermissionDefinitionDto>> ListAsync(string? module = null, CancellationToken ct = default);
}
