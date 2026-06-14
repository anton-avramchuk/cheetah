using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Events;
using Cheetah.Modules.Tags.Application.Exceptions;
using Cheetah.Modules.Tags.Domain.Entities;
using Cheetah.Modules.Tags.Domain.Specifications;
using Cheetah.Modules.Tags.DomainEvents;

namespace Cheetah.Modules.Tags.Application.Assignments;

/// <summary>
/// Назначить набор тэгов сущности. Проверяет: тип зарегистрирован в каталоге,
/// тэги существуют, их группы разрешены для типа, не превышен лимит на сущность.
/// Идемпотентна по составу (уже назначенные тэги пропускаются).
/// </summary>
public sealed record AssignTagsCommand(
    string EntityType,
    Guid EntityId,
    IReadOnlyList<Guid> TagIds,
    Guid? AssignedBy = null) : ICommand;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<AssignTagsCommand>))]
public class AssignTagsCommandHandler : ICommandHandler<AssignTagsCommand>
{
    private readonly IRepository<TaggableEntityType, string> _types;
    private readonly IRepository<Tag, Guid> _tags;
    private readonly IRepository<TagAssignment, Guid> _assignments;
    private readonly IEventBus _eventBus;

    public AssignTagsCommandHandler(
        IRepository<TaggableEntityType, string> types,
        IRepository<Tag, Guid> tags,
        IRepository<TagAssignment, Guid> assignments,
        IEventBus eventBus)
    {
        _types = types;
        _tags = tags;
        _assignments = assignments;
        _eventBus = eventBus;
    }

    public async ValueTask HandleAsync(AssignTagsCommand command, CancellationToken ct = default)
    {
        if (command.TagIds.Count == 0)
            return;

        var type = await _types.GetByIdAsync(command.EntityType, ct)
            ?? throw new TagsValidationException($"Entity type '{command.EntityType}' is not registered");

        var requestedIds = command.TagIds.Distinct().ToArray();
        var tags = await _tags.GetAllAsync(new TagsByIdsSpecification(requestedIds), ct);
        if (tags.Count != requestedIds.Length)
        {
            var missing = requestedIds.Except(tags.Select(t => t.Id));
            throw new TagsValidationException($"Tags not found: {string.Join(", ", missing)}");
        }

        foreach (var tag in tags)
        {
            if (!type.IsGroupAllowed(tag.Group))
                throw new TagsValidationException(
                    $"Tag '{tag.Name}' (group '{tag.Group}') is not allowed for type '{type.Id}'");
        }

        var existing = await _assignments.GetAllAsync(
            new AssignmentsByEntitySpecification(command.EntityType, command.EntityId), ct);
        var existingTagIds = existing.Select(a => a.TagId).ToHashSet();

        var newTagIds = requestedIds.Where(id => !existingTagIds.Contains(id)).ToArray();
        if (newTagIds.Length == 0)
            return;

        if (type.MaxTagsPerEntity is { } max && existing.Count + newTagIds.Length > max)
            throw new TagsValidationException(
                $"Max tags per entity ({max}) exceeded for type '{type.Id}'");

        foreach (var tagId in newTagIds)
        {
            _assignments.Add(TagAssignment.Create(
                tagId, command.EntityType, command.EntityId, command.AssignedBy));
        }

        await _assignments.SaveChangesAsync(ct);

        foreach (var tagId in newTagIds)
        {
            await _eventBus.PublishAsync(
                new TagAssignedEvent(tagId, command.EntityType, command.EntityId), ct);
        }
    }
}
