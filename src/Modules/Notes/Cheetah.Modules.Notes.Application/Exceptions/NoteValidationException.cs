using Cheetah.Core.Exceptions;

namespace Cheetah.Modules.Notes.Application.Exceptions;

/// <summary>
/// Нарушение бизнес-правил/инвариантов заметки (некорректный родитель треда, недопустимые данные).
/// Наследует <see cref="CrmException"/> → глобальный <c>ValidationExceptionHandler</c> отдаёт
/// 400 Bad Request с сообщением. «Заметка не найдена» — это не сюда, а
/// <see cref="Cheetah.Core.Domain.Exceptions.EntityNotFoundException"/> (404).
/// </summary>
public sealed class NoteValidationException : CrmException
{
    public NoteValidationException(string message) : base(message) { }
}
