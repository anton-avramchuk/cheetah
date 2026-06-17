namespace Cheetah.Modules.NotesTimeline.Application.Exceptions;

/// <summary>
/// Нарушение бизнес-правил/инвариантов заметки (не найдена, некорректные данные).
/// Маппится в API на 400 Bad Request.
/// </summary>
public sealed class NoteValidationException : Exception
{
    public NoteValidationException(string message) : base(message) { }
}
