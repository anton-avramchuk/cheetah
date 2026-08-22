using Cheetah.Modules.Notes.Default.Contracts;
using Cheetah.Modules.Notes.Domain.Entities;

namespace Cheetah.Modules.Notes.Default.Entities;

/// <summary>
/// Заметка «из коробки» — минимальное закрытие абстрактного <see cref="NoteBase"/> без доп. полей.
/// Служит одновременно рабочей реализацией и эталонным примером расширения: своё приложение
/// объявляет собственный <c>sealed class Note : NoteBase</c> с нужными полями и своей фабрикой.
/// </summary>
public sealed class Note : NoteBase
{
    private Note() { } // EF

    public static Note Create(CreateNoteRequest request)
    {
        var note = new Note();
        note.InitializeCore(
            Guid.NewGuid(),
            request.EntityType,
            request.EntityId,
            request.AuthorId,
            request.Body,
            request.Mentions,
            request.AttachmentFileIds,
            request.ParentNoteId);
        return note;
    }
}
