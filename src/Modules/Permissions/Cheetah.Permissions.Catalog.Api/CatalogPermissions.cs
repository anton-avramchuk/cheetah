namespace Cheetah.Permissions.Catalog.Api;

/// <summary>
/// Permissions, требуемые самим Catalog.Api. Объявлены здесь, чтобы при старте сервиса
/// они автоматически попали в каталог через scan.
/// </summary>
public static class CatalogPermissions
{
    [Permission(Sync, "Зарегистрировать permissions в каталоге (sync)")]
    public const string Sync = "Permissions.Catalog.Sync";

    [Permission(Read, "Чтение каталога permissions")]
    public const string Read = "Permissions.Catalog.Read";
}
