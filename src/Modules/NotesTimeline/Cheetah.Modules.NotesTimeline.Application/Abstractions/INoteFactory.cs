using Cheetah.Modules.NotesTimeline.Contracts;
using Cheetah.Modules.NotesTimeline.Domain.Entities;

namespace Cheetah.Modules.NotesTimeline.Application.Abstractions;

/// <summary>
/// Фабрика конкретной заметки из запроса на создание. Реализуется наследником — он знает, как
/// сконструировать свою сущность (включая доп. поля) и завести инварианты/события через
/// <c>InitializeCore</c>. Так generic-handler создаёт заметку, не зная конкретного типа.
/// </summary>
public interface INoteFactory<out TNote, in TCreateRequest>
    where TNote : NoteBase
    where TCreateRequest : CreateNoteRequestBase
{
    TNote Create(TCreateRequest request);
}
