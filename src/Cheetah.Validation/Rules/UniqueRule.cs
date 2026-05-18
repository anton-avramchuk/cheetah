namespace Cheetah.Validation.Rules;

/// <summary>
/// Уникальность значения. Реальная проверка требует доступа к репозиторию документов,
/// поэтому в Cheetah.Validation это PLACEHOLDER — всегда возвращает Valid.
///
/// Конкретная реализация будет добавлена в Cheetah.Documents, где появится репозиторий
/// и понятие "тип документа" (Scope). Тогда UniqueRule зарегистрирует кастомного
/// валидатора через DI, который заменит этот no-op.
/// </summary>
public sealed class UniqueRule : IValidationRule
{
    /// <summary>Область уникальности: имя справочника, типа документа и т.п.</summary>
    public string Scope { get; init; } = "";

    public string RuleType => nameof(UniqueRule);

    public ValueTask<ValidationOutcome> ValidateAsync(ValidationContext context, CancellationToken ct = default)
    {
        // Placeholder — расширяется в Cheetah.Documents.
        return ValueTask.FromResult(ValidationOutcome.Valid);
    }
}
