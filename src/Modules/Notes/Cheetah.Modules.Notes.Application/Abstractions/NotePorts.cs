using Cheetah.Modules.Notes.Contracts;
using Cheetah.Modules.Notes.Domain.Entities;

namespace Cheetah.Modules.Notes.Application.Abstractions;

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
