using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.Events;
using Cheetah.Modules.NotesTimeline.Application.Abstractions;
using Cheetah.Modules.NotesTimeline.Contracts;
using Cheetah.Modules.NotesTimeline.Domain.Entities;

namespace Cheetah.Modules.NotesTimeline.Application.Notes;

/// <summary>Создать заметку из запроса наследника.</summary>
public sealed record CreateNoteCommand<TCreateRequest>(TCreateRequest Request) : ICommand<Guid>
    where TCreateRequest : CreateNoteRequestBase;

public class CreateNoteCommandHandler<TNote, TCreateRequest>
    : ICommandHandler<CreateNoteCommand<TCreateRequest>, Guid>
    where TNote : NoteBase
    where TCreateRequest : CreateNoteRequestBase
{
    private readonly INoteFactory<TNote, TCreateRequest> _factory;
    private readonly IRepository<TNote, Guid> _repository;
    private readonly IEventBus _eventBus;

    public CreateNoteCommandHandler(
        INoteFactory<TNote, TCreateRequest> factory,
        IRepository<TNote, Guid> repository,
        IEventBus eventBus)
    {
        _factory = factory;
        _repository = repository;
        _eventBus = eventBus;
    }

    public async ValueTask<Guid> HandleAsync(CreateNoteCommand<TCreateRequest> command, CancellationToken ct = default)
    {
        var note = _factory.Create(command.Request);
        _repository.Add(note);
        await _repository.SaveChangesAsync(ct);

        foreach (var e in note.DomainEvents)
            await _eventBus.PublishAsync(e, ct);
        note.ClearDomainEvents();

        return note.Id;
    }
}
