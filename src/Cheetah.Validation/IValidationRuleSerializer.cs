namespace Cheetah.Validation;

/// <summary>
/// Сериализация/десериализация правил валидации. Правила хранятся в схеме типа документа
/// как JSON, поэтому нужна стабильная типизация через <c>$type</c>.
/// </summary>
public interface IValidationRuleSerializer
{
    /// <summary>Сериализует одно правило в JSON.</summary>
    string Serialize(IValidationRule rule);

    /// <summary>Десериализует одно правило из JSON. Бросает при неизвестном $type.</summary>
    IValidationRule Deserialize(string json);

    /// <summary>Сериализует набор правил в JSON-массив.</summary>
    string SerializeMany(IReadOnlyList<IValidationRule> rules);

    /// <summary>Десериализует набор правил.</summary>
    IReadOnlyList<IValidationRule> DeserializeMany(string json);
}
