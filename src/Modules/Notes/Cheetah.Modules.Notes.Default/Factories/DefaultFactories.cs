using Cheetah.Modules.Notes.Application.Abstractions;
using Cheetah.Modules.Notes.Default.Contracts;
using Cheetah.Modules.Notes.Default.Entities;

namespace Cheetah.Modules.Notes.Default.Factories;

/// <summary>Фабрика заметки «из коробки» — заменяет <c>new</c> абстрактной сущности в generic-handler'е.</summary>
public sealed class NoteFactory : INoteFactory<Note, CreateNoteRequest>
{
    public Note Create(CreateNoteRequest request) => Note.Create(request);
}

/// <summary>Проекция заметки в DTO «из коробки». Наследник добавляет сюда свои поля.</summary>
public sealed class NoteProjector : INoteProjector<Note, NoteDto>
{
    public NoteDto ToDto(Note n) => new()
    {
        Id = n.Id,
        EntityType = n.EntityType,
        EntityId = n.EntityId,
        AuthorId = n.AuthorId,
        Body = n.Body,
        // Копии, а не живые backing-списки сущности: DTO сериализуется уже после выхода из
        // хендлера, и правка заметки в том же scope мутировала бы отданный ответ.
        Mentions = n.Mentions.ToArray(),
        AttachmentFileIds = n.AttachmentFileIds.ToArray(),
        ParentNoteId = n.ParentNoteId,
        PinnedAt = n.PinnedAt,
        CreatedAt = n.CreatedAt,
        UpdatedAt = n.UpdatedAt
    };
}
