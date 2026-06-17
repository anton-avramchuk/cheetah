using Cheetah.Modules.NotesTimeline.Contracts;
using Cheetah.Modules.NotesTimeline.Domain.Entities;

namespace Cheetah.Modules.NotesTimeline.Application.Abstractions;

/// <summary>
/// Проекция конкретной заметки в конкретный DTO (включая доп. поля наследника). Реализуется
/// наследником; используется generic query-handler'ами вместо Mapster, чтобы не требовать скрытой
/// конфигурации маппинга расширенных полей.
/// </summary>
public interface INoteProjector<in TNote, out TDto>
    where TNote : NoteBase
    where TDto : NoteDtoBase
{
    TDto ToDto(TNote note);
}
