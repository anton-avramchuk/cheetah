namespace Cheetah.Validation;

/// <summary>
/// Контекст применения правила. AllValues позволяет кросс-полевым правилам читать
/// значения других полей. Services — для правил, которым нужен I/O (UniqueRule, лукапы).
/// </summary>
public sealed record ValidationContext(
    string FieldPath,
    object? Value,
    IReadOnlyDictionary<string, object?> AllValues,
    IServiceProvider Services);
