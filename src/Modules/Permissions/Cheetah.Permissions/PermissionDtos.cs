namespace Cheetah.Permissions;

/// <summary>DTO одного permission'а для catalog API и client.</summary>
public sealed record PermissionDefinitionDto(string Key, string Description, string Module);

/// <summary>Запрос на регистрацию permissions от микросервиса в catalog.</summary>
public sealed record RegistrySyncRequest(string Module, IReadOnlyList<PermissionDefinitionDto> Items);
