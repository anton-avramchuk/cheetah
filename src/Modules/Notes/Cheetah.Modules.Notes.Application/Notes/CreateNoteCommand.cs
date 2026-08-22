using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.Events;
using Cheetah.Modules.Notes.Application.Abstractions;
using Cheetah.Modules.Notes.Application.Exceptions;
using Cheetah.Modules.Notes.Contracts;
using Cheetah.Modules.Notes.Domain.Entities;

namespace Cheetah.Modules.Notes.Application.Notes;

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
        await ValidateParentAsync(command.Request, ct);

        var note = _factory.Create(command.Request);
        _repository.Add(note);
        await _repository.SaveChangesAsync(ct);

        foreach (var e in note.DomainEvents)
            await _eventBus.PublishAsync(e, ct);
        note.ClearDomainEvents();

        return note.Id;
    }

    /// <summary>
    /// Ответ должен ссылаться на существующую активную заметку той же сущности, и треды — плоские
    /// (ответ на ответ запрещён). Иначе заметка «повиснет»: в выдаче по сущности её видно, а ветки
    /// у неё нет.
    /// </summary>
    private async ValueTask ValidateParentAsync(TCreateRequest request, CancellationToken ct)
    {
        if (request.ParentNoteId is not { } parentId)
            return;

        var parent = await _repository.GetByIdAsync(parentId, ct)
            ?? throw new NoteValidationException($"Parent note '{parentId}' not found");

        if (parent.ParentNoteId is not null)
            throw new NoteValidationException(
                $"Note '{parentId}' is already a reply — threads are flat, reply to the root note instead");

        if (parent.EntityType != request.EntityType?.Trim() || parent.EntityId != request.EntityId)
            throw new NoteValidationException(
                $"Parent note '{parentId}' belongs to a different entity");
    }
}
