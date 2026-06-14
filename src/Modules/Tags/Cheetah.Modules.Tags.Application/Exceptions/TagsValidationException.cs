namespace Cheetah.Modules.Tags.Application.Exceptions;

/// <summary>
/// Нарушение бизнес-правил тэгирования (тип не зарегистрирован, превышен лимит,
/// группа тэга не разрешена и т.п.). Маппится в API на 400 Bad Request.
/// </summary>
public sealed class TagsValidationException : Exception
{
    public TagsValidationException(string message) : base(message) { }
}
