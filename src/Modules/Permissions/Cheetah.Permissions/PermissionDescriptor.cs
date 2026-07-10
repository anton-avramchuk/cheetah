namespace Cheetah.Permissions;

/// <summary>
/// Описание объявленного permission. Используется реестром и catalog-API.
/// <paramref name="Feature"/> — ключ фич-флага, за которым спрятан permission (null — виден всегда).
/// </summary>
public sealed record PermissionDescriptor(string Key, string Description, string Module, string? Feature = null);
