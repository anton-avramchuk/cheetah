using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Events;
using Cheetah.Modules.Tags.Domain.Entities;
using Cheetah.Modules.Tags.Domain.Specifications;
using Cheetah.Modules.Tags.DomainEvents;

namespace Cheetah.Modules.Tags.Application.Assignments;

/// <summary>Снять набор тэгов с сущности. Отсутствующие привязки молча игнорируются.</summary>
public sealed record UnassignTagsCommand(
    string EntityType,
    Guid EntityId,
    IReadOnlyList<Guid> TagIds) : ICommand;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<UnassignTagsCommand>))]
public class UnassignTagsCommandHandler : ICommandHandler<UnassignTagsCommand>
{
    private readonly IRepository<TagAssignment, Guid> _assignments;
    private readonly IEventBus _eventBus;

    public UnassignTagsCommandHandler(IRepository<TagAssignment, Guid> assignments, IEventBus eventBus)
    {
        _assignments = assignments;
        _eventBus = eventBus;
    }

    public async ValueTask HandleAsync(UnassignTagsCommand command, CancellationToken ct = default)
    {
        if (command.TagIds.Count == 0)
            return;

        var toRemove = await _assignments.GetAllAsync(
            new AssignmentsByEntityAndTagsSpecification(
                command.EntityType, command.EntityId, command.TagIds.Distinct().ToArray()), ct);

        if (toRemove.Count == 0)
            return;

        foreach (var assignment in toRemove)
            _assignments.Delete(assignment);

        await _assignments.SaveChangesAsync(ct);

        foreach (var assignment in toRemove)
        {
            await _eventBus.PublishAsync(
                new TagUnassignedEvent(assignment.TagId, command.EntityType, command.EntityId), ct);
        }
    }
}
