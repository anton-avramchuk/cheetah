using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Events;
using Cheetah.Modules.Tags.Application.Exceptions;
using Cheetah.Modules.Tags.Domain.Entities;
using Cheetah.Modules.Tags.Domain.Specifications;

namespace Cheetah.Modules.Tags.Application.Tags;

/// <summary>Создать тэг в словаре. Имя уникально.</summary>
public sealed record CreateTagCommand(
    string Name,
    string? Color,
    string? Description,
    string? Group) : ICommand<Guid>;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<CreateTagCommand, Guid>))]
public class CreateTagCommandHandler : ICommandHandler<CreateTagCommand, Guid>
{
    private readonly IRepository<Tag, Guid> _repository;
    private readonly IEventBus _eventBus;

    public CreateTagCommandHandler(IRepository<Tag, Guid> repository, IEventBus eventBus)
    {
        _repository = repository;
        _eventBus = eventBus;
    }

    public async ValueTask<Guid> HandleAsync(CreateTagCommand command, CancellationToken ct = default)
    {
        var exists = await _repository.ExistsAsync(new TagByNameSpecification(command.Name), ct);
        if (exists)
            throw new TagsValidationException($"Tag '{command.Name}' already exists");

        var tag = Tag.Create(command.Name, command.Color, command.Description, command.Group);
        _repository.Add(tag);
        await _repository.SaveChangesAsync(ct);

        foreach (var e in tag.DomainEvents)
            await _eventBus.PublishAsync(e, ct);
        tag.ClearDomainEvents();

        return tag.Id;
    }
}
