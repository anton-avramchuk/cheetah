using Cheetah.Core.Exceptions;

namespace Cheetah.Modules.Teams.Application.Exceptions;

/// <summary>
/// Нарушение бизнес-правил/инвариантов модуля Teams (не найдено, дубликат имени, некорректные
/// данные). Наследует <see cref="CrmException"/> → глобальный ValidationExceptionHandler отдаёт 400
/// с сообщением.
/// </summary>
public sealed class TeamsValidationException : CrmException
{
    public TeamsValidationException(string message) : base(message) { }
}
