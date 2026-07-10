namespace Cheetah.Permissions;

/// <summary>
/// DTO одного permission'а для catalog API и client. <paramref name="Feature"/> — ключ фич-флага,
/// за которым он спрятан (null — виден всегда).
/// </summary>
public sealed record PermissionDefinitionDto(string Key, string Description, string Module, string? Feature = null);

/// <summary>Запрос на регистрацию permissions от микросервиса в catalog.</summary>
public sealed record RegistrySyncRequest(string Module, IReadOnlyList<PermissionDefinitionDto> Items);
