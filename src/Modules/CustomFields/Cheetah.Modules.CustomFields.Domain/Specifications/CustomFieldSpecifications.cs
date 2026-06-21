using System.Linq.Expressions;
using Cheetah.Core.Specification;
using Cheetah.Modules.CustomFields.Domain.Entities;

namespace Cheetah.Modules.CustomFields.Domain.Specifications;

/// <summary>Определения типа в скоупе тенанта (включая глобальные шаблоны <c>TenantId == null</c>).</summary>
public sealed class DefinitionsByEntityTypeSpecification : Specification<CustomFieldDefinition>
{
    private readonly Guid? _tenantId;
    private readonly string _entityType;
    private readonly bool _onlyActive;

    public DefinitionsByEntityTypeSpecification(Guid? tenantId, string entityType, bool onlyActive)
        => (_tenantId, _entityType, _onlyActive) = (tenantId, entityType, onlyActive);

    public override Expression<Func<CustomFieldDefinition, bool>> ToExpression()
        => d => d.EntityType == _entityType
                && (d.TenantId == _tenantId || d.TenantId == null)
                && (!_onlyActive || d.IsActive);
}

/// <summary>Определение по ключу в точном скоупе (TenantId, EntityType).</summary>
public sealed class DefinitionByKeySpecification : Specification<CustomFieldDefinition>
{
    private readonly Guid? _tenantId;
    private readonly string _entityType;
    private readonly string _key;

    public DefinitionByKeySpecification(Guid? tenantId, string entityType, string key)
        => (_tenantId, _entityType, _key) = (tenantId, entityType, key);

    public override Expression<Func<CustomFieldDefinition, bool>> ToExpression()
        => d => d.EntityType == _entityType && d.Key == _key && d.TenantId == _tenantId;
}

/// <summary>Набор значений конкретной сущности.</summary>
public sealed class ValueSetByEntitySpecification : Specification<CustomFieldValueSet>
{
    private readonly Guid? _tenantId;
    private readonly string _entityType;
    private readonly string _entityId;

    public ValueSetByEntitySpecification(Guid? tenantId, string entityType, string entityId)
        => (_tenantId, _entityType, _entityId) = (tenantId, entityType, entityId);

    public override Expression<Func<CustomFieldValueSet, bool>> ToExpression()
        => v => v.EntityType == _entityType && v.EntityId == _entityId && v.TenantId == _tenantId;
}

/// <summary>Наборы значений для списка id одного типа (анти-N+1 для batch-get).</summary>
public sealed class ValueSetsByEntityIdsSpecification : Specification<CustomFieldValueSet>
{
    private readonly Guid? _tenantId;
    private readonly string _entityType;
    private readonly IReadOnlyCollection<string> _ids;

    public ValueSetsByEntityIdsSpecification(Guid? tenantId, string entityType, IReadOnlyCollection<string> ids)
        => (_tenantId, _entityType, _ids) = (tenantId, entityType, ids);

    public override Expression<Func<CustomFieldValueSet, bool>> ToExpression()
        => v => v.EntityType == _entityType && v.TenantId == _tenantId && _ids.Contains(v.EntityId);
}

/// <summary>Зарегистрированный тип по ключу.</summary>
public sealed class EntityTypeByKeySpecification : Specification<CustomFieldEntityType>
{
    private readonly string _key;
    public EntityTypeByKeySpecification(string key) => _key = key;

    public override Expression<Func<CustomFieldEntityType, bool>> ToExpression()
        => t => t.Key == _key;
}
