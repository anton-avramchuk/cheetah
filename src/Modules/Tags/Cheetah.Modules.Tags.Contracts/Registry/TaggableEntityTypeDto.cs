namespace Cheetah.Modules.Tags.Contracts.Registry;

/// <summary>
/// Дескриптор применимого к тэгам типа сущности. Сервис-владелец отправляет его
/// в Tags при старте; Tags хранит каталог и проверяет по нему операции назначения.
/// </summary>
public sealed record TaggableEntityTypeDto(
    string Key,
    string DisplayName,
    string OwnerService,
    int? MaxTagsPerEntity,
    bool AllowAdHocTags,
    IReadOnlyList<string> AllowedGroups);
