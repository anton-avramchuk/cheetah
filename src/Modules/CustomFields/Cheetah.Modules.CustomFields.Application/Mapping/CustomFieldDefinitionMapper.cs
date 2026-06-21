using Cheetah.Core.DependencyInjection;
using Cheetah.Modules.CustomFields.Contracts;
using Cheetah.Modules.CustomFields.Domain.Abstractions;
using Cheetah.Modules.CustomFields.Domain.Entities;
using Cheetah.Validation;
using Cheetah.Validation.Rules;

namespace Cheetah.Modules.CustomFields.Application.Mapping;

/// <summary>
/// Проекция определений в DTO и сборка правил валидации поля. Разбор/сборка правил — через
/// <see cref="IValidationRuleSerializer"/> (полиморфный JSON ядра <c>Cheetah.Validation</c>).
/// </summary>
[Export(LifetimeType.Scoped, typeof(CustomFieldDefinitionMapper))]
public sealed class CustomFieldDefinitionMapper
{
    private readonly IValidationRuleSerializer _rules;

    public CustomFieldDefinitionMapper(IValidationRuleSerializer rules) => _rules = rules;

    public string? SerializeRules(IReadOnlyList<IValidationRule>? rules)
        => rules is null || rules.Count == 0 ? null : _rules.SerializeMany(rules);

    public IReadOnlyList<IValidationRule> DeserializeRules(string? json)
        => string.IsNullOrWhiteSpace(json) ? [] : _rules.DeserializeMany(json);

    public CustomFieldDefinitionDto ToDto(CustomFieldDefinition d)
        => new(d.Id, d.TenantId, d.EntityType, d.Key, d.Label, d.DataType, d.Required,
            d.Options, DeserializeRules(d.ValidationRulesJson), d.VisibilityRule, d.Order, d.IsActive,
            d.CreatedAt, d.UpdatedAt);

    public CustomFieldDefinitionDto ToDto(CustomFieldDefinitionSnapshot s)
        => new(s.Id, s.TenantId, s.EntityType, s.Key, s.Label, s.DataType, s.Required,
            s.Options, DeserializeRules(s.ValidationRulesJson), s.VisibilityRule, s.Order, IsActive: true,
            CreatedAt: null, UpdatedAt: null);

    /// <summary>Полный набор правил поля для движка валидации: <c>RequiredRule</c> (если обязательно) + декларированные.</summary>
    public IReadOnlyList<IValidationRule> BuildRules(CustomFieldDefinitionSnapshot s)
    {
        var declared = DeserializeRules(s.ValidationRulesJson);
        if (!s.Required) return declared;
        var list = new List<IValidationRule>(declared.Count + 1) { new RequiredRule() };
        list.AddRange(declared);
        return list;
    }
}
