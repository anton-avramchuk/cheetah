using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.Events;
using Cheetah.Modules.NotesTimeline.Application.Abstractions;
using Cheetah.Modules.NotesTimeline.Application.Exceptions;
using Cheetah.Modules.NotesTimeline.Contracts;
using Cheetah.Modules.NotesTimeline.Domain.Entities;

namespace Cheetah.Modules.NotesTimeline.Application.Notes;

// ── Редактирование тела/упоминаний ──────────────────────────────────────────────────────────

/// <summary>Отредактировать тело заметки и набор упоминаний.</summary>
public sealed record UpdateNoteCommand<TUpdateRequest>(Guid Id, TUpdateRequest Request) : ICommand
    where TUpdateRequest : UpdateNoteRequestBase;

public class UpdateNoteCommandHandler<TNote, TUpdateRequest> : ICommandHandler<UpdateNoteCommand<TUpdateRequest>>
    where TNote : NoteBase
    where TUpdateRequest : UpdateNoteRequestBase
{
    private readonly IRepository<TNote, Guid> _repository;
    private readonly IEventBus _eventBus;

    public UpdateNoteCommandHandler(IRepository<TNote, Guid> repository, IEventBus eventBus)
    {
        _repository = repository;
        _eventBus = eventBus;
    }

    public async ValueTask HandleAsync(UpdateNoteCommand<TUpdateRequest> command, CancellationToken ct = default)
    {
        var note = await _repository.GetByIdAsync(command.Id, ct)
            ?? throw new NoteValidationException($"Note '{command.Id}' not found");

        note.Edit(command.Request.Body, command.Request.Mentions);
        await SaveAndPublishAsync(_repository, _eventBus, note, ct);
    }

    internal static async ValueTask SaveAndPublishAsync(
        IRepository<TNote, Guid> repository, IEventBus eventBus, TNote note, CancellationToken ct)
    {
        await repository.SaveChangesAsync(ct);
        foreach (var e in note.DomainEvents)
            await eventBus.PublishAsync(e, ct);
        note.ClearDomainEvents();
    }
}

// ── Закрепление ──────────────────────────────────────────────────────────────────────────────

/// <summary>Закрепить заметку на карточке сущности.</summary>
public sealed record PinNoteCommand(Guid Id) : ICommand;

public class PinNoteCommandHandler<TNote> : ICommandHandler<PinNoteCommand>
    where TNote : NoteBase
{
    private readonly IRepository<TNote, Guid> _repository;

    public PinNoteCommandHandler(IRepository<TNote, Guid> repository) => _repository = repository;

    public async ValueTask HandleAsync(PinNoteCommand command, CancellationToken ct = default)
    {
        var note = await _repository.GetByIdAsync(command.Id, ct)
            ?? throw new NoteValidationException($"Note '{command.Id}' not found");

        note.Pin();
        await _repository.SaveChangesAsync(ct);
    }
}

/// <summary>Открепить заметку.</summary>
public sealed record UnpinNoteCommand(Guid Id) : ICommand;

public class UnpinNoteCommandHandler<TNote> : ICommandHandler<UnpinNoteCommand>
    where TNote : NoteBase
{
    private readonly IRepository<TNote, Guid> _repository;

    public UnpinNoteCommandHandler(IRepository<TNote, Guid> repository) => _repository = repository;

    public async ValueTask HandleAsync(UnpinNoteCommand command, CancellationToken ct = default)
    {
        var note = await _repository.GetByIdAsync(command.Id, ct)
            ?? throw new NoteValidationException($"Note '{command.Id}' not found");

        note.Unpin();
        await _repository.SaveChangesAsync(ct);
    }
}

// ── Удаление (soft-delete) ─────────────────────────────────────────────────────────────────

/// <summary>Удалить заметку (soft-delete).</summary>
public sealed record RemoveNoteCommand(Guid Id) : ICommand;

public class RemoveNoteCommandHandler<TNote> : ICommandHandler<RemoveNoteCommand>
    where TNote : NoteBase
{
    private readonly IRepository<TNote, Guid> _repository;
    private readonly IEventBus _eventBus;

    public RemoveNoteCommandHandler(IRepository<TNote, Guid> repository, IEventBus eventBus)
    {
        _repository = repository;
        _eventBus = eventBus;
    }

    public async ValueTask HandleAsync(RemoveNoteCommand command, CancellationToken ct = default)
    {
        var note = await _repository.GetByIdAsync(command.Id, ct)
            ?? throw new NoteValidationException($"Note '{command.Id}' not found");

        note.Remove();
        await UpdateNoteCommandHandler<TNote, UpdateNoteRequestBase>.SaveAndPublishAsync(
            _repository, _eventBus, note, ct);
    }
}
