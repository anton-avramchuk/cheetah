using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.Domain.Exceptions;
using Cheetah.Core.Events;
using Cheetah.Modules.Notes.Contracts;
using Cheetah.Modules.Notes.Domain.Entities;

namespace Cheetah.Modules.Notes.Application.Notes;

// ── Редактирование тела/упоминаний/вложений ─────────────────────────────────────────────────

/// <summary>Отредактировать тело заметки, набор упоминаний и вложений.</summary>
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
        var note = await _repository.RequireAsync(command.Id, ct);

        // null в запросе = «не трогать»: правка одного лишь текста не стирает упоминания/вложения.
        note.Edit(command.Request.Body, command.Request.Mentions);
        note.ChangeAttachments(command.Request.AttachmentFileIds);

        await NotePersistence.SaveAndPublishAsync(_repository, _eventBus, note, ct);
    }
}

/// <summary>Общие для lifecycle-handler'ов хвосты: «найти или 404» и «сохранить + опубликовать».</summary>
internal static class NotePersistence
{
    public static async ValueTask<TNote> RequireAsync<TNote>(
        this IRepository<TNote, Guid> repository, Guid id, CancellationToken ct)
        where TNote : NoteBase
        => await repository.GetByIdAsync(id, ct)
           ?? throw EntityNotFoundException.For<TNote>(id);

    public static async ValueTask SaveAndPublishAsync<TNote>(
        IRepository<TNote, Guid> repository, IEventBus eventBus, TNote note, CancellationToken ct)
        where TNote : NoteBase
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
    private readonly IEventBus _eventBus;

    public PinNoteCommandHandler(IRepository<TNote, Guid> repository, IEventBus eventBus)
    {
        _repository = repository;
        _eventBus = eventBus;
    }

    public async ValueTask HandleAsync(PinNoteCommand command, CancellationToken ct = default)
    {
        var note = await _repository.RequireAsync(command.Id, ct);

        note.Pin();
        await NotePersistence.SaveAndPublishAsync(_repository, _eventBus, note, ct);
    }
}

/// <summary>Открепить заметку.</summary>
public sealed record UnpinNoteCommand(Guid Id) : ICommand;

public class UnpinNoteCommandHandler<TNote> : ICommandHandler<UnpinNoteCommand>
    where TNote : NoteBase
{
    private readonly IRepository<TNote, Guid> _repository;
    private readonly IEventBus _eventBus;

    public UnpinNoteCommandHandler(IRepository<TNote, Guid> repository, IEventBus eventBus)
    {
        _repository = repository;
        _eventBus = eventBus;
    }

    public async ValueTask HandleAsync(UnpinNoteCommand command, CancellationToken ct = default)
    {
        var note = await _repository.RequireAsync(command.Id, ct);

        note.Unpin();
        await NotePersistence.SaveAndPublishAsync(_repository, _eventBus, note, ct);
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
        var note = await _repository.RequireAsync(command.Id, ct);

        note.Remove();
        await NotePersistence.SaveAndPublishAsync(_repository, _eventBus, note, ct);
    }
}
