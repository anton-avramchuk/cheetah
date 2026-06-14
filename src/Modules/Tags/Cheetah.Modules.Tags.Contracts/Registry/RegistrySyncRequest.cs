namespace Cheetah.Modules.Tags.Contracts.Registry;

/// <summary>
/// Запрос регистрации применимых типов сущностей одного сервиса. Идемпотентно:
/// существующие типы (по <see cref="TaggableEntityTypeDto.Key"/>) обновляются, новые создаются.
/// </summary>
public sealed record RegistrySyncRequest(string OwnerService, IReadOnlyList<TaggableEntityTypeDto> Items);
