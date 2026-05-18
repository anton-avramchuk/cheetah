using System.Text.Json;

namespace Cheetah.Validation;

/// <summary>
/// JSON-сериализатор поверх System.Text.Json. Полиморфизм описан атрибутами
/// [JsonPolymorphic]/[JsonDerivedType] на самом интерфейсе IValidationRule.
/// </summary>
public sealed class ValidationRuleSerializer : IValidationRuleSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public string Serialize(IValidationRule rule) =>
        JsonSerializer.Serialize(rule, Options);

    public IValidationRule Deserialize(string json)
        => JsonSerializer.Deserialize<IValidationRule>(json, Options)
           ?? throw new InvalidOperationException("Deserialized rule is null");

    public string SerializeMany(IReadOnlyList<IValidationRule> rules)
        => JsonSerializer.Serialize(rules, Options);

    public IReadOnlyList<IValidationRule> DeserializeMany(string json)
        => JsonSerializer.Deserialize<IReadOnlyList<IValidationRule>>(json, Options)
           ?? Array.Empty<IValidationRule>();
}
