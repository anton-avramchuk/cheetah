namespace Cheetah.Modules.Notes.Shared;

/// <summary>
/// Общие константы шаблонного модуля Notes: имя БД-подключения, ограничения длин, дефолтные имена
/// таблицы/схемы, префикс маршрутов и параметры страницы. Используются абстрактными базами
/// (<c>NoteConfigurationBase</c>, эндпоинты) как значения по умолчанию, переопределяемые наследником.
/// </summary>
public static class NotesConstants
{
    public const string ConnectionStringName = "Notes";

    public const int MaxEntityTypeLength = 64;
    public const int MaxBodyLength = 8000;

    public const string DefaultSchema = "notes";
    public const string DefaultNotesTableName = "Notes";

    public const string DefaultNotesRoutePrefix = "api/notes";

    /// <summary>Размер страницы списка заметок по умолчанию.</summary>
    public const int DefaultPageSize = 50;
    public const int MaxPageSize = 200;
}
