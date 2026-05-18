using System.Text.Json.Serialization;
using Cheetah.Validation.Rules;

namespace Cheetah.Validation;

/// <summary>
/// Декларативное правило валидации. RuleType — стабильный строковый идентификатор для JSON.
/// Реализации обычно immutable record'ы; параметры — public-свойства, сериализуются как JSON.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type", IgnoreUnrecognizedTypeDiscriminators = false)]
[JsonDerivedType(typeof(RequiredRule), nameof(RequiredRule))]
[JsonDerivedType(typeof(StringLengthRule), nameof(StringLengthRule))]
[JsonDerivedType(typeof(NumberRangeRule), nameof(NumberRangeRule))]
[JsonDerivedType(typeof(DateRangeRule), nameof(DateRangeRule))]
[JsonDerivedType(typeof(PatternRule), nameof(PatternRule))]
[JsonDerivedType(typeof(EnumValueRule), nameof(EnumValueRule))]
[JsonDerivedType(typeof(CompareRule), nameof(CompareRule))]
[JsonDerivedType(typeof(RequiredIfRule), nameof(RequiredIfRule))]
[JsonDerivedType(typeof(ExpressionRule), nameof(ExpressionRule))]
[JsonDerivedType(typeof(UniqueRule), nameof(UniqueRule))]
public interface IValidationRule
{
    /// <summary>Стабильный строковый идентификатор правила (используется в JSON).</summary>
    [JsonIgnore]
    string RuleType { get; }

    ValueTask<ValidationOutcome> ValidateAsync(
        ValidationContext context,
        CancellationToken ct = default);
}
