namespace Cheetah.Modules.Tags.Contracts.Assignments;

/// <summary>Назначить набор тэгов сущности другого сервиса.</summary>
public sealed record AssignTagsRequest(
    string EntityType,
    Guid EntityId,
    IReadOnlyList<Guid> TagIds);
