namespace Cheetah.Permissions;

/// <summary>
/// Описание объявленного permission. Используется реестром и catalog-API.
/// </summary>
public sealed record PermissionDescriptor(string Key, string Description, string Module);
