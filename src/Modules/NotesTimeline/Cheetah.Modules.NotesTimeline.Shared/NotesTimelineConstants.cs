namespace Cheetah.Modules.NotesTimeline.Shared;

/// <summary>
/// Общие константы шаблонного модуля NotesTimeline: имя БД-подключения, ограничения длин, дефолтные
/// имена таблиц/схемы и префиксы маршрутов. Используются абстрактными базами (<c>NoteConfigurationBase</c>,
/// <c>NoteEndpointsBase</c>) как значения по умолчанию, которые наследник может переопределить.
/// </summary>
public static class NotesTimelineConstants
{
    public const string ConnectionStringName = "NotesTimeline";

    public const int MaxEntityTypeLength = 64;
    public const int MaxBodyLength = 8000;
    public const int MaxKindLength = 128;
    public const int MaxTitleLength = 512;

    public const string DefaultSchema = "notes_timeline";
    public const string DefaultNotesTableName = "Notes";
    public const string DefaultTimelineTableName = "TimelineEntries";

    public const string DefaultNotesRoutePrefix = "api/notes";
    public const string DefaultTimelineRoutePrefix = "api/timeline";

    /// <summary>Размер страницы ленты по умолчанию (курсорная пагинация).</summary>
    public const int DefaultTimelinePageSize = 50;
    public const int MaxTimelinePageSize = 200;
}
