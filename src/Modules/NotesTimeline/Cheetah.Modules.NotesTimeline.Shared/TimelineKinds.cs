namespace Cheetah.Modules.NotesTimeline.Shared;

/// <summary>
/// «Виды» строк ленты — НЕ закрытый enum, а строковые ключи <c>"{service}.{event}"</c> (стабильный
/// контракт, как featureKey/permissionKey). Здесь — лишь известные значения ядра; потребители
/// добавляют свои, регистрируя <c>ITimelineProjector</c>.
/// </summary>
public static class TimelineKinds
{
    public const string NoteAdded = "notes.note-added";
    public const string NoteRemoved = "notes.note-removed";
    public const string EntityRemoved = "core.entity-removed";

    // Известные виды доменных модулей (материализуются их собственными проекторами).
    public const string DealStageChanged = "deals.stage-changed";
    public const string ActivityCompleted = "activities.completed";
    public const string DocumentSent = "sales.document-sent";
    public const string InvoicePaid = "sales.invoice-paid";
}
