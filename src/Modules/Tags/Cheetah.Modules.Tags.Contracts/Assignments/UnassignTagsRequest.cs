namespace Cheetah.Modules.Tags.Contracts.Assignments;

/// <summary>Снять набор тэгов с сущности другого сервиса.</summary>
public sealed record UnassignTagsRequest(
    string EntityType,
    Guid EntityId,
    IReadOnlyList<Guid> TagIds);
